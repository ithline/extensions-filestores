using System.Diagnostics;
using Ithline.Extensions.FileStores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Ithline.AspNetCore.FileStores;

internal static class FileStoreHelpers
{
    public static bool IsGetOrHeadMethod(string method)
    {
        return HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
    }

    public static bool PathEndsInSlash(PathString path)
    {
        return path.HasValue && path.Value!.EndsWith("/", StringComparison.Ordinal);
    }

    public static bool TryMatchPath(HttpContext context, PathString matchUrl, bool forDirectory, out PathString subpath)
    {
        var path = context.Request.Path;

        if (forDirectory && !PathEndsInSlash(path))
        {
            path += new PathString("/");
        }

        if (path.StartsWithSegments(matchUrl, out subpath))
        {
            return true;
        }
        return false;
    }

    public static bool IsImageContentType(string? contentType)
    {
        return contentType is not null && contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsSuppressDefault(HttpContext httpContext)
    {
        return httpContext.Request.Query.TryGetValue("suppress", out var str)
            && bool.TryParse(str, out var suppress)
            && suppress;
    }

    /// <summary>
    /// Returns the normalized form of the requested range if the Range Header in the <see cref="HttpContext.Request"/> is valid.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> associated with the request.</param>
    /// <param name="requestHeaders">The <see cref="RequestHeaders"/> associated with the given <paramref name="context"/>.</param>
    /// <param name="length">The total length of the file representation requested.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <returns>A boolean value which represents if the <paramref name="requestHeaders"/> contain a single valid
    /// range request. A <see cref="RangeItemHeaderValue"/> which represents the normalized form of the
    /// range parsed from the <paramref name="requestHeaders"/> or <c>null</c> if it cannot be normalized.</returns>
    /// <remark>If the Range header exists but cannot be parsed correctly, or if the provided length is 0, then the range request cannot be satisfied (status 416).
    /// This results in (<c>true</c>,<c>null</c>) return values.</remark>
    public static (bool isRangeRequest, RangeItemHeaderValue? range) ParseRange(
        HttpContext context,
        RequestHeaders requestHeaders,
        long length,
        ILogger logger)
    {
        var rawRangeHeader = context.Request.Headers.Range;
        if (StringValues.IsNullOrEmpty(rawRangeHeader))
        {
            logger.LogTrace("Range header's value is empty.");
            return (false, null);
        }

        // Perf: Check for a single entry before parsing it
        if (rawRangeHeader.Count > 1 || (rawRangeHeader[0] ?? string.Empty).Contains(','))
        {
            logger.LogDebug("Multiple ranges are not supported.");

            // The spec allows for multiple ranges but we choose not to support them because the client may request
            // very strange ranges (e.g. each byte separately, overlapping ranges, etc.) that could negatively
            // impact the server. Ignore the header and serve the response normally.
            return (false, null);
        }

        var rangeHeader = requestHeaders.Range;
        if (rangeHeader == null)
        {
            logger.LogDebug("Range header's value is invalid.");
            // Invalid
            return (false, null);
        }

        // Already verified above
        Debug.Assert(rangeHeader.Ranges.Count == 1);

        var ranges = rangeHeader.Ranges;
        if (ranges == null)
        {
            logger.LogDebug("Range header's value is invalid.");
            return (false, null);
        }

        if (ranges.Count == 0)
        {
            return (true, null);
        }

        if (length == 0)
        {
            return (true, null);
        }

        // Normalize the ranges
        var range = NormalizeRange(ranges.Single(), length);

        // Return the single range
        return (true, range);
    }

    // Internal for testing
    public static RangeItemHeaderValue? NormalizeRange(RangeItemHeaderValue range, long length)
    {
        var start = range.From;
        var end = range.To;

        // X-[Y]
        if (start.HasValue)
        {
            if (start.Value >= length)
            {
                // Not satisfiable, skip/discard.
                return null;
            }
            if (!end.HasValue || end.Value >= length)
            {
                end = length - 1;
            }
        }
        else if (end.HasValue)
        {
            // suffix range "-X" e.g. the last X bytes, resolve
            if (end.Value == 0)
            {
                // Not satisfiable, skip/discard.
                return null;
            }

            var bytes = Math.Min(end.Value, length);
            start = length - bytes;
            end = start + bytes - 1;
        }

        return new RangeItemHeaderValue(start, end);
    }

    public static async Task SendFileAsync(this HttpResponse response, IBlobFile file, long offset, long? count, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(offset, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, file.Length);
        if (count is long countValue)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(countValue, 0, nameof(count));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(countValue, file.Length - offset, nameof(count));
        }

        await using var stream = await file.OpenReadAsync().ConfigureAwait(false);

        var useRequestAborted = !cancellationToken.CanBeCanceled;
        var localCancel = useRequestAborted
            ? response.HttpContext.RequestAborted
            : cancellationToken;

        try
        {
            localCancel.ThrowIfCancellationRequested();
            if (offset > 0)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }

            await StreamCopyOperation.CopyToAsync(stream, response.Body, count, bufferSize: 64 * 1024, localCancel);
        }
        catch (OperationCanceledException) when (useRequestAborted)
        {
        }
    }
}

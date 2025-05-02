using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Ithline.AspNetCore.FileStores;

/// <summary>
/// Defines *all* the logger messages produced by static files
/// </summary>
internal static partial class FileStoreLoggerExtensions
{
    [LoggerMessage(1, LogLevel.Debug, "{Method} requests are not supported")]
    public static partial void RequestMethodNotSupported(this ILogger logger, string method);

    [LoggerMessage(2, LogLevel.Information, "Sending file. Request path: '{VirtualPath}'. Physical path: '{PhysicalPath}'", EventName = nameof(FileServed))]
    private static partial void FileServedCore(this ILogger logger, string virtualPath, string physicalPath);

    public static void FileServed(this ILogger logger, string virtualPath, string? physicalPath)
    {
        if (string.IsNullOrEmpty(physicalPath))
        {
            physicalPath = "N/A";
        }
        FileServedCore(logger, virtualPath, physicalPath);
    }

    [LoggerMessage(3, LogLevel.Debug, "The request path {Path} does not match the path filter")]
    public static partial void PathMismatch(this ILogger logger, string path);

    [LoggerMessage(4, LogLevel.Debug, "The request path {Path} does not match a supported file type")]
    public static partial void FileTypeNotSupported(this ILogger logger, string path);

    [LoggerMessage(5, LogLevel.Debug, "The request path {Path} does not match an existing file")]
    public static partial void FileNotFound(this ILogger logger, string path);

    [LoggerMessage(6, LogLevel.Information, "The file {Path} was not modified")]
    public static partial void FileNotModified(this ILogger logger, string path);

    [LoggerMessage(7, LogLevel.Information, "Precondition for {Path} failed")]
    public static partial void PreconditionFailed(this ILogger logger, string path);

    [LoggerMessage(8, LogLevel.Debug, "Handled. Status code: {StatusCode} File: {Path}")]
    public static partial void Handled(this ILogger logger, int statusCode, string path);

    [LoggerMessage(9, LogLevel.Warning, "Range not satisfiable for {Path}")]
    public static partial void RangeNotSatisfiable(this ILogger logger, string path);

    [LoggerMessage(10, LogLevel.Information, "Sending {Range} of file {Path}")]
    public static partial void SendingFileRange(this ILogger logger, StringValues range, string path);

    [LoggerMessage(11, LogLevel.Debug, "Copying {Range} of file {Path} to the response body")]
    public static partial void CopyingFileRange(this ILogger logger, StringValues range, string path);

    [LoggerMessage(14, LogLevel.Debug, "The file transmission was cancelled")]
    public static partial void WriteCancelled(this ILogger logger, Exception ex);

    [LoggerMessage(15, LogLevel.Debug, "Static files was skipped as the request already matched an endpoint.")]
    public static partial void EndpointMatched(this ILogger logger);
}

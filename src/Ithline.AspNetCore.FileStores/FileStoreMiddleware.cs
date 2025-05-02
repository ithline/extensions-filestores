using Ithline.Extensions.FileStores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace Ithline.AspNetCore.FileStores;

internal sealed class FileStoreMiddleware
{
    private static readonly IContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();

    private readonly RequestDelegate _next;
    private readonly TimeProvider _timeProvider;
    private readonly IBlobFileStore _fileStore;
    private readonly PathString _matchUrl;
    private readonly ILogger _logger;

    public FileStoreMiddleware(
        RequestDelegate next,
        TimeProvider timeProvider,
        IBlobFileStore fileStore,
        ILogger<FileStoreMiddleware> logger,
        PathString matchUrl)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _matchUrl = matchUrl;
    }

    public Task Invoke(HttpContext httpContext)
    {
        if (!ValidateNoEndpointDelegate(httpContext))
        {
            _logger.EndpointMatched();
        }
        else if (!ValidateMethod(httpContext))
        {
            _logger.RequestMethodNotSupported(httpContext.Request.Method);
        }
        else if (!ValidatePath(httpContext, _matchUrl, out var subPath))
        {
            _logger.PathMismatch(subPath);
        }
        else if (!LookupContentType(_contentTypeProvider, subPath, out var contentType))
        {
            _logger.FileTypeNotSupported(subPath);
        }
        else
        {
            // If we get here, we can try to serve the file
            return this.TryServeStaticFile(httpContext, contentType, subPath);
        }

        return _next(httpContext);
    }

    // Return true because we only want to run if there is no endpoint delegate.
    private static bool ValidateNoEndpointDelegate(HttpContext context)
    {
        return context.GetEndpoint()?.RequestDelegate is null;
    }

    private static bool ValidateMethod(HttpContext context)
    {
        return FileStoreHelpers.IsGetOrHeadMethod(context.Request.Method);
    }

    private static bool ValidatePath(HttpContext context, PathString matchUrl, out PathString subPath)
    {
        return FileStoreHelpers.TryMatchPath(context, matchUrl, forDirectory: false, out subPath);
    }

    private static bool LookupContentType(IContentTypeProvider contentTypeProvider, PathString subPath, out string? contentType)
    {
        if (contentTypeProvider.TryGetContentType(subPath.Value!, out contentType))
        {
            return true;
        }

        return false;
    }

    private async Task TryServeStaticFile(HttpContext httpContext, string? contentType, PathString subPath)
    {
        var mediaFile = await _fileStore.GetFileAsync(subPath);
        if (mediaFile is not null)
        {
            // If we get here, we can try to serve the file
            var fileContext = new FileContext(httpContext, _timeProvider, _logger, mediaFile, contentType, subPath);
            await fileContext.ServeStaticFile();
        }
        else
        {
            // we didn't find the file, we continue to next middleware
            _logger.FileNotFound(subPath);
            await _next(httpContext);
        }
    }
}

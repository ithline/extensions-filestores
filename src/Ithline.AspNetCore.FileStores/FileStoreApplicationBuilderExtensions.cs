using Microsoft.AspNetCore.Http;
using Ithline.AspNetCore.FileStores;

namespace Microsoft.AspNetCore.Builder;

public static class FileStoreApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMediaFiles(this IApplicationBuilder app, PathString requestPath = default)
    {
        return app.UseMiddleware<FileStoreMiddleware>(requestPath);
    }
}

using Ithline.AspNetCore.FileStores;
using Ithline.Extensions.FileStores;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for adding a static file middleware serving files from <see cref="IBlobFileStore"/>.
/// </summary>
public static class FileStoreApplicationBuilderExtensions
{
    /// <summary>
    /// Adds a static file middleware serving files from <see cref="IBlobFileStore"/>.
    /// </summary>
    /// <param name="app">The request pipelines builder.</param>
    /// <param name="requestPath">Path used to narrow the scope of the files looked up.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IApplicationBuilder UseFileStore(this IApplicationBuilder app, PathString requestPath = default)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<FileStoreMiddleware>(requestPath);
    }

    /// <summary>
    /// Adds a static file middleware serving files from <see cref="IBlobFileStore"/>.
    /// </summary>
    /// <param name="app">The request pipelines builder.</param>
    /// <param name="fileStore">File store used to serve files.</param>
    /// <param name="requestPath">Path used to narrow the scope of the files looked up.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IApplicationBuilder UseFileStore(this IApplicationBuilder app, IBlobFileStore fileStore, PathString requestPath = default)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(fileStore);

        return app.UseMiddleware<FileStoreMiddleware>(fileStore, requestPath);
    }
}

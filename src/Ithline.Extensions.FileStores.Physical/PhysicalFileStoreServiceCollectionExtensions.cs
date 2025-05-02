using Ithline.Extensions.FileStores;
using Ithline.Extensions.FileStores.Physical;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding physical <see cref="IBlobFileStore"/> to the <see cref="IServiceCollection"/>.
/// </summary>
public static class PhysicalFileStoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds a physical <see cref="IBlobFileStore"/> to the <see cref="IServiceCollection"/> using the specified path as its root directory.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="root">The root directory used to store files.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="root"/> is <see langword="null"/>, empty string, contains only white-space characters or is not a rooted path.</exception>
    public static IServiceCollection AddPhysicalMediaStore(this IServiceCollection services, string root)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        if (!Path.IsPathRooted(root))
        {
            throw new ArgumentException("The path must be absolute.", nameof(root));
        }

        services.AddSingleton<IBlobFileStore>(sp => new PhysicalFileStore(root));
        return services;
    }
}

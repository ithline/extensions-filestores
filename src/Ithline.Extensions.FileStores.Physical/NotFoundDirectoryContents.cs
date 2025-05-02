namespace Ithline.Extensions.FileStores.Physical;

internal sealed class NotFoundDirectoryContents : IDirectoryContents
{
    private NotFoundDirectoryContents()
    {
    }

    public static IDirectoryContents Singleton { get; } = new NotFoundDirectoryContents();

    public async IAsyncEnumerator<IBlobFile> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }
}

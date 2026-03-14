namespace Ithline.Extensions.FileStores.Physical;

internal sealed class NotFoundDirectoryContents : IBlobDirectoryContents
{
    private NotFoundDirectoryContents()
    {
    }

    public static IBlobDirectoryContents Singleton { get; } = new NotFoundDirectoryContents();

    public async IAsyncEnumerator<IBlobFile> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        yield break;
    }
}

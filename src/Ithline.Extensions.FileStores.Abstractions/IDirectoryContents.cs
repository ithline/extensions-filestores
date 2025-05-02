namespace Ithline.Extensions.FileStores;

/// <summary>
/// Represents a directory's content in the file provider.
/// </summary>
public interface IDirectoryContents : IAsyncEnumerable<IBlobFile>
{
}

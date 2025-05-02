using System.Buffers;
using Microsoft.Extensions.Primitives;

namespace Ithline.Extensions.FileStores.Physical;

internal static class PathUtils
{
    private static readonly char[] _pathSeparators = [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar];
    private static readonly SearchValues<char> _invalidFileNameChars = SearchValues.Create(GetInvalidFileNameChars());
    private static readonly SearchValues<char> _invalidFilterChars = SearchValues.Create(GetInvalidFilterChars());

    public static bool HasInvalidPathChars(ReadOnlySpan<char> path)
    {
        return path.ContainsAny(_invalidFileNameChars);
    }

    public static bool HasInvalidFilterChars(ReadOnlySpan<char> path)
    {
        return path.ContainsAny(_invalidFilterChars);
    }

    public static string EnsureTrailingSlash(string path)
    {
        if (!string.IsNullOrEmpty(path) && path[^1] != Path.DirectorySeparatorChar)
        {
            return path + Path.DirectorySeparatorChar;
        }

        return path;
    }

    public static bool PathNavigatesAboveRoot(string path)
    {
        var depth = 0;
        var tokenizer = new StringTokenizer(path, _pathSeparators);

        foreach (var segment in tokenizer)
        {
            if (segment.Equals(".") || segment.Equals(""))
            {
                continue;
            }
            else if (segment.Equals(".."))
            {
                depth--;

                if (depth == -1)
                {
                    return true;
                }
            }
            else
            {
                depth++;
            }
        }

        return false;
    }

    public static string TrimSeparators(string path)
    {
        return path.TrimStart(_pathSeparators);
    }

    private static char[] GetInvalidFileNameChars()
    {
        return Path.GetInvalidFileNameChars()
            .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar)
            .ToArray();
    }

    private static char[] GetInvalidFilterChars()
    {
        return GetInvalidFileNameChars()
            .Where(c => c is not '*' and not '|' and not '?')
            .ToArray();
    }
}

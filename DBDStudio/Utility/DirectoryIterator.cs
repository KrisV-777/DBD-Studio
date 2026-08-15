using Noggog;

namespace DBDStudio.Utility
{
    public static class DirectoryIterator
    {
        public class IteratorDetails(string rootFolder, int iterDepth)
        {
            public DirectoryInfo? RootFolder { get; set; } =
                rootFolder.IsNullOrEmpty() || !Directory.Exists(rootFolder) ? null : new DirectoryInfo(rootFolder);
            public int IterDepth { get; set; } = iterDepth;
        }

        public static IEnumerable<FileInfo> EnumerateProjectFiles(IEnumerable<IteratorDetails> rootPaths, string infix, string searchPattern)
        {
            return rootPaths
                .SelectMany(rp => CollectDirectoriesOfDepth(rp.RootFolder, rp.IterDepth))
                .SelectMany(directory => EnumerateDirectories(directory.FullName, infix))
                .SelectMany(directory => directory.EnumerateFiles(searchPattern));
        }

        private static IEnumerable<DirectoryInfo> EnumerateDirectories(string root, string pattern)
        {
            IEnumerable<DirectoryInfo> current = [new DirectoryInfo(root)];

            var patterns = pattern.Split(
                [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in patterns) {
                current = current.SelectMany(d => d.EnumerateDirectories(part));
            }

            return current;
        }

        private static IEnumerable<DirectoryInfo> CollectDirectoriesOfDepth(DirectoryInfo? directory, int depth)
        {
            if (directory == null)
                yield break;

            ArgumentOutOfRangeException.ThrowIfNegative(depth);
            if (depth == 0) {
                yield return directory;
                yield break;
            }

            foreach (var subdirectory in directory.EnumerateDirectories()) {
                foreach (var result in CollectDirectoriesOfDepth(subdirectory, depth - 1))
                    yield return result;
            }
        }
    }
}

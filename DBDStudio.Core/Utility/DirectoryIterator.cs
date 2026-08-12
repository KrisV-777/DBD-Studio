namespace DBDStudio.Core.Utility
{
    public static class DirectoryIterator
    {
        public class IteratorDetails(string rootFolder, int iterDepth)
        {
            public DirectoryInfo RootFolder { get; set; } = new DirectoryInfo(rootFolder);
            public int IterDepth { get; set; } = iterDepth;
        }

        public static IEnumerable<FileInfo> EnumerateProjectFiles(IEnumerable<IteratorDetails> rootPaths, string infix, string searchPattern)
        {
            return rootPaths
                .SelectMany(rp => CollectDirectoriesOfDepth(rp.RootFolder, rp.IterDepth))
                .Select(directory => new DirectoryInfo(Path.Combine(directory.FullName, infix)))
                .Where(directory => directory.Exists)
                .SelectMany(directory => directory.EnumerateFiles(searchPattern));
        }

        private static IEnumerable<DirectoryInfo> CollectDirectoriesOfDepth(DirectoryInfo directory, int depth)
        {
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
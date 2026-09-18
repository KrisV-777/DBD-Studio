namespace DBDStudio.Tests;

internal sealed class TempDir : IDisposable
{
    public string Path { get; }

    public TempDir()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dbdstudio-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (!Directory.Exists(Path)) {
            return;
        }

        try {
            Directory.Delete(Path, recursive: true);
        } catch (IOException) {
            // Ignore temp cleanup races on Windows file handles.
        } catch (UnauthorizedAccessException) {
            // Ignore temp cleanup races on Windows file handles.
        }
    }
}

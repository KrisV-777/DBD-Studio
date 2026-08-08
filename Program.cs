using Avalonia;
using System.Runtime.InteropServices;

namespace DBDStudio
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                LogAndShowFatal(ex);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace();

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                LogAndShowFatal(ex);
        }

        private static void LogAndShowFatal(Exception ex)
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DBDStudio");
                Directory.CreateDirectory(logDir);
                var logPath = Path.Combine(logDir, "crash.log");
                File.WriteAllText(logPath, $"[{DateTime.Now:u}]\n{ex}");

                NativeMethods.MessageBox(
                    IntPtr.Zero,
                    $"DBD Studio crashed on startup.\n\n{ex.GetType().Name}: {ex.Message}\n\nFull crash log: {logPath}",
                    "DBD Studio - Fatal Error",
                    0x10); // MB_ICONERROR
            }
            catch { /* ignore failures in the error handler itself */ }
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = false)]
            internal static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
        }
    }
}

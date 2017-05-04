using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PluginInterop.Python
{
    public class Utils
    {
        /// <summary>
        /// Searches for python executable with perseuspy installed in PATH and installation folders.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryFindPythonExecutable(out string path)
        {
            if (CheckPythonInstallation("python"))
            {
                Debug.WriteLine("Found 'python' in PATH");
                path = "python";
                return true;
            }
            var folders = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Python"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Python"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Python"),
            };
            foreach (var folder in folders)
            {
                foreach (var pyFolder in Directory.EnumerateDirectories(folder, "Python*"))
                {
                    var pyPath = Path.Combine(pyFolder, "python.exe");
                    if (CheckPythonInstallation(pyPath))
                    {
                        Debug.WriteLine($"Found 'python' in default installation folder: {pyPath}");
                        path = pyPath;
                        return true;
                    }
                }
            }
            path = default(string);
            return false;
        }

        /// <summary>
        /// Returns true if executable path points to python and can import perseuspy.
        /// </summary>
        /// <param name="exeName"></param>
        /// <returns></returns>
        public static bool CheckPythonInstallation(string exeName)
        {
            try
            {
                Process p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = exeName,
                        Arguments = "-c \"import perseuspy; print('hello')\"",
                        RedirectStandardOutput = true,
                    }
                };
                var output = new StringBuilder();
                p.OutputDataReceived += (sender, args) =>
                {
                    output.Append(args.Data);
                };
                p.Start();
                p.BeginOutputReadLine();
                p.WaitForExit();
                return p.ExitCode == 0 && output.ToString().StartsWith("hello");
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PluginInterop.R
{
    public class Utils
    {
        /// <summary>
        /// Searches for python executable with perseuspy installed in PATH and installation folders.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool TryFindRExecutable(out string path)
        {
            if (CheckRInstallation("Rscript"))
            {
                Debug.WriteLine("Found 'Rscript' in PATH");
                path = "Rscript";
                return true;
            }
            var folders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            }.Where(f => !string.IsNullOrEmpty(f));
            foreach (var folder in folders.Select(f => Path.Combine(f, "R")).Where(Directory.Exists))
            {
                foreach (var subFolder in Directory.EnumerateDirectories(folder, "R*"))
                {
                    var exePath = Path.Combine(subFolder, "bin", "Rscript.exe");
                    if (CheckRInstallation(exePath))
                    {
                        Debug.WriteLine($"Found 'R' in default installation folder: {exePath}");
                        path = exePath;
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
        public static bool CheckRInstallation(string exeName)
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
                        Arguments = "--vanilla -e \"library('PerseusR')\"",
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
                return p.ExitCode == 0 ;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
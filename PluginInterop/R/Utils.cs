using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using BaseLibS.Param;

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

		/// <summary>
		/// Try to find R executable and show green light if found.
		/// </summary>
	    public static FileParam CreateCheckedFileParam(string interpreterLabel, string interpreterFilter, Python.Utils.TryFindExecutableDelegate tryFindExecutable)
	    {
		    void CheckFileName(string s, CheckedFileParamControl control)
		    {
			    if (string.IsNullOrWhiteSpace(s))
			    {
				    return;
			    }
			    if (CheckRInstallation(s))
			    {
				    control.selectButton.BackColor = Color.LimeGreen;
				    control.ToolTip.SetToolTip(control.selectButton, "R installation was found");
			    }
			    else
			    {
				    control.selectButton.BackColor = Color.Red;
				    control.ToolTip.SetToolTip(control.selectButton, "A valid R installation was not found. Make sure to select a R installation with 'PerseusR' installed");
			    }
		    }

		    var fileParam = new CheckedFileParamWf(interpreterLabel, CheckFileName) {Filter = interpreterFilter};
		    if (tryFindExecutable(out string path))
		    {
			    fileParam.Value = path;
		    }
		    return fileParam;
	    }

        public static bool CheckPythonInstallation(string exeName, string[] packages)
        {
            try
            {
                var imports = string.Join("; ", packages.Select(package => $"import {package}"));
                Process p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = exeName,
                        Arguments = $"-c \"{imports}; print('hello')\"",
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

        public delegate bool TryFindExecutableDelegate(out string path);

        public static FileParam CreateCheckedFileParamforupload(string interpreterLabel, string interpreterFilter, TryFindExecutableDelegate tryFindExecutable,
    string[] packages)
        {
            Action<string, CheckedFileParamControl> checkFileName = (s, control) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return;
                }
                if (CheckPythonInstallation(s, packages))
                {
                    control.selectButton.BackColor = Color.LimeGreen;
                    control.ToolTip.SetToolTip(control.selectButton, "Python installation was found");
                }
                else
                {
                    control.selectButton.BackColor = Color.Red;
                    control.ToolTip.SetToolTip(control.selectButton,
                        "A valid Python installation was not found.\n" +
                        "Could not import one or more packages:\n" +
                        string.Join(", ", packages));
                }
                ;
            };
            var fileParam = new CheckedFileParamWf(interpreterLabel, checkFileName) { Filter = interpreterFilter };
            string path;
            if (tryFindExecutable(out path))
            {
                fileParam.Value = path;
            }
            return fileParam;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Matrix;
using PerseusApi.Network;
using PerseusApi.Utils;
using PerseusLibS.Data.Network;

namespace PluginInterop
{
    public static class Utils
    {

        public static ButtonParamWf DataPreviewButton(IMatrixData mdata)
        {
            return new ButtonParamWf("Download data for preview", "save", (o, args) =>
            {
                var dialog = new SaveFileDialog
                {
                    FileName = $"{mdata.Name}.txt",
                    Filter = "tab-separated data, *.txt|*.txt"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    PerseusUtils.WriteMatrixToFile(mdata, dialog.FileName, false);
                }
            });
        }

        public static ButtonParamWf DataPreviewButton(INetworkData ndata)
        {
            return new ButtonParamWf("Download data for preview", "save", (o, args) =>
            {
                var dialog = new FolderBrowserDialog
                {
                    ShowNewFolderButton = true
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    FolderFormat.Write(ndata, dialog.SelectedPath);
                }
            });
        }

        public static ButtonParamWf ParametersPreviewButton(Parameters parameters)
        {
            return new ButtonParamWf("Download parameter for preview", "save", (o, args) =>
            {
                var dialog = new SaveFileDialog
                {
                    FileName = "parameters.xml",
                    Filter = "*.xml|*.xml"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (var f = new StreamWriter(dialog.FileName))
                    {
                        parameters.Convert(ParamUtils.ConvertBack);
                        var serializer = new XmlSerializer(parameters.GetType());
                        serializer.Serialize(f, parameters);
                        parameters.Convert(WinFormsParameterFactory.Convert);
                    }
                }
            });
        }

        /// <summary>
        /// Runs the executable with the provided arguments. Returns the exit code of the process,
        /// where 0 indicates success.
        /// </summary>
        /// <param name="remoteExe"></param>
        /// <param name="args"></param>
        /// <param name="status"></param>
        /// <param name="errorString"></param>
        /// <returns></returns>
        public static int RunProcess(string remoteExe, string args, Action<string> status, out string errorString)
        {
            errorString = null; // no error
            var externalProcessInfo = new ProcessStartInfo
            {
                FileName = remoteExe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var process = new Process {StartInfo = externalProcessInfo};
            var outputData = new List<string>();
            process.OutputDataReceived += (sender, output) =>
            {
                Debug.WriteLine(output.Data);
                status(output.Data);
                outputData.Add(output.Data);
            };
            var errorData = new List<string>();
            process.ErrorDataReceived += (sender, error) =>
            {
                Debug.WriteLine(error.Data);
                errorData.Add(error.Data);
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            var exitCode = process.ExitCode;
            Debug.WriteLine($"Process exited with exit code {exitCode}");
            if (exitCode != 0)
            {
                var statusString = String.Join("\n", outputData);
                var errString = String.Join("\n", errorData);
                errorString = String.Concat("Output\n", statusString, "\n", "Error\n", errString);
            }
            process.Dispose();
            return exitCode;
        }
    }
}
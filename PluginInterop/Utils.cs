using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using BaseLib.Param;
using BaseLibS.Param;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Network;
using PerseusApi.Utils;
using PerseusApi.Utils.Network;

namespace PluginInterop
{
	/// <summary>
	/// Provides a number of utility functions
	/// </summary>
    public static class Utils
    {
		/// <summary>
		/// Write parameters to temporary file.
		/// Useful as alternative implementation of <see cref="InteropBase.GetCommandLineArguments"/>.
		/// </summary>
	    public static string WriteParametersToFile(Parameters param)
	    {
		    var tempFile = Path.GetTempFileName();
			param.ToFile(tempFile);
		    return tempFile;
	    }

		/// <summary>
		/// Create a preview button for the GUI which can be used save the data to file.
		/// This is especially useful for development and debugging.
		/// </summary>
	    public static ButtonParamWf DataPreviewButton(IData data)
	    {
		    if (data is IMatrixData mdata)
		    {
			    return MatrixDataPreviewButton(mdata);
		    }
		    if (data is INetworkData ndata)
		    {
			    return NetworkDataPreviewButton(ndata);
		    }
			throw new NotImplementedException($"{nameof(DataPreviewButton)} not implemented for type {data.GetType()}!");
	    }

	    public static ButtonParamWf MatrixDataPreviewButton(IMatrixData mdata)
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

        public static ButtonParamWf NetworkDataPreviewButton(INetworkData ndata)
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

		/// <summary>
		/// Create a preview button for the GUI which can be used save the parameters to file.
		/// This is especially useful for development and debugging.
		/// </summary>
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

        /// <summary>
        /// Read supplementary files according to file paths and data types.
        /// </summary>
        public static IData[] ReadSupplementaryData(string[] suppFiles, DataType[] supplDataTypes, ProcessInfo processInfo)
        {
            var numSupplTables = suppFiles.Length;
            IData[] supplData = new IData[numSupplTables];
            for (int i = 0; i < numSupplTables; i++)
            {
                switch (supplDataTypes[i])
                {
                    case DataType.Matrix:
                        var mdata = PerseusFactory.CreateMatrixData();
                        PerseusUtils.ReadMatrixFromFile(mdata, processInfo, suppFiles[i], '\t');
                        supplData[i] = mdata;
                        break;
                    case DataType.Network:
                        var ndata = PerseusFactory.CreateNetworkData();
                        FolderFormat.Read(ndata, suppFiles[i], processInfo);
                        supplData[i] = ndata;
                        break;
                    default:
                        throw new NotImplementedException($"Data type {supplDataTypes[i]} not supported!");
                }
            }
            return supplData;
        }

        /// <summary>
        /// Create a temporary path for a specific data type.
        /// </summary>
        public static string CreateTemporaryPath(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Matrix:
                    return Path.GetTempFileName();
                case DataType.Network:
                    return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                default:
                    throw new NotImplementedException($"Data type {dataType} not supported!");
            }
        }
    }
}
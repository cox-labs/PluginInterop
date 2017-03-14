using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using BaseLib.Param;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PluginInterop
{
    public abstract class MatrixProcessing : IMatrixProcessing
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public float DisplayRank => 1;
        public bool IsActive => true;
        public int GetMaxThreads(Parameters parameters) => 1;
        public virtual bool HasButton => false;
        public virtual Bitmap2 DisplayImage => null; 
        public string Url { get; }
        public virtual string Heading => "External";
        public string HelpOutput { get; }
        public string[] HelpSupplTables { get; }
        public int NumSupplTables { get; }
        public string[] HelpDocuments { get; }
        public int NumDocuments { get; }

        protected virtual string CodeLabel => "Script file";
        protected virtual string InterpreterLabel => "Executable";
        protected virtual string InterpreterFilter => "Interpreter, *.exe|*.exe";
        protected virtual string CodeFilter => "Script";

        protected virtual string GetCodeFile(Parameters param)
        {
            var codeFile = param.GetParam<string>(CodeLabel).Value;
            return codeFile;
        }

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
            var inFile = Path.GetTempFileName();
            PerseusUtils.WriteMatrixToFile(mdata, inFile, false);
            var paramFile = Path.GetTempFileName();
            using (var f = new StreamWriter(paramFile))
            {
                param.Convert(ParamUtils.ConvertBack);
                var serializer = new XmlSerializer(param.GetType());
                serializer.Serialize(f, param);
            }
            var outFile = Path.GetTempFileName();
            var codeFile = GetCodeFile(param);
            var args = $"{codeFile} {paramFile} {inFile} {outFile}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
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
                processInfo.Status(output.Data);
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
            Debug.WriteLine($"Process exited with exit code {process.ExitCode}");
            if (process.ExitCode == 1)
            {
                var statusString = string.Join("\n", outputData);
                var errString = string.Join("\n", errorData);
                processInfo.ErrString = string.Concat("Output\n", statusString, "\n", "Error\n", errString);
                return;
            }
            process.Close();
            mdata.Clear();
            PerseusUtils.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');
        }

        /// <summary>
        /// Processing specific paramters. Should be overridden by derived class.
        /// </summary>
        /// <param name="mdata"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        protected virtual Parameters AddParameters(IMatrixData mdata, ref string errString)
        {
            return new Parameters(new Parameter[] { new FileParam(CodeLabel) {Filter = CodeFilter}}, "specific");
        }

        public Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            Parameters parameters = AddParameters(mdata, ref errString);
            var previewButton = new ButtonParamWf("Download data for preview", "save", (o, args) =>
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
            var parametersPreviewButton = new ButtonParamWf("Download parameter for preview", "save", (o, args) =>
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
            parameters.AddParameterGroup(new Parameter[]
            {
                new FileParam(InterpreterLabel) {Filter = InterpreterFilter},
                previewButton, parametersPreviewButton
            }, "generic", false);
            return parameters;
        }
    }
}

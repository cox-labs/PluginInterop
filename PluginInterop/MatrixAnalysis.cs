using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using BaseLib.Param;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PluginInterop
{
    public abstract class MatrixAnalysis : IMatrixAnalysis
    {
        public abstract string Name { get; }
        public virtual string Description { get; }
        public virtual float DisplayRank => 1;
        public virtual bool IsActive => true;
        public virtual int GetMaxThreads(Parameters parameters) => 1;
        public virtual bool HasButton => true;
        public virtual string Url { get; }
        public virtual Bitmap2 DisplayImage { get; } 
        public virtual string Heading => "External";

        protected virtual string CodeLabel => "Script file";
        protected virtual string InterpreterLabel => "Executable";
        protected virtual string InterpreterFilter => "Interpreter, *.exe|*.exe";
        protected virtual string CodeFilter => "Script";

        protected virtual string GetCodeFile(Parameters param)
        {
            var codeFile = param.GetParam<string>(CodeLabel).Value;
            return codeFile;
        }

        protected virtual Parameters AddParameters(IMatrixData mdata, ref string errString)
        {
            return new Parameters(new Parameter[] { new FileParam(CodeLabel) { Filter = CodeFilter } }, "specific");
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

        public virtual IAnalysisResult AnalyzeData(IMatrixData mdata, Parameters param, ProcessInfo processInfo)
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
            using (var process = Process.Start(externalProcessInfo))
            {
                var output = process.StandardOutput;
                string line;
                while ((line = output.ReadLine()) != null)
                {
                    Debug.WriteLine($"remote stdout > {line}");
                }
                var error = process.StandardOutput;
                while ((line = error.ReadLine()) != null)
                {
                    Debug.WriteLine($"remote error > {line}");
                }
                process.WaitForExit();
            }
            return GenerateResult(outFile, mdata, processInfo);
        }

        protected abstract IAnalysisResult GenerateResult(string outFile, IMatrixData mdata, ProcessInfo pinfo);
    }
}

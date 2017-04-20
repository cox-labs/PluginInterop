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
    public abstract class MatrixAnalysis : InteropBase, IMatrixAnalysis
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


        /// <summary>
        /// Create the parameters for the GUI with default of 'Code file' and 'Executable'. Includes buttons
        /// for preview downloads of 'Data' and 'Parameters' for development purposes.
        /// Overwrite this function to provide custom parameters.
        /// </summary>
        public virtual Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(new Parameter[] { CodeFileParam() }, "specific", false);
            var previewButton = Utils.DataPreviewButton(mdata);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), previewButton, parametersPreviewButton }, "generic", false);
            return parameters;
        }

        public IAnalysisResult AnalyzeData(IMatrixData mdata, Parameters param, ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
            var inFile = Path.GetTempFileName();
            PerseusUtils.WriteMatrixToFile(mdata, inFile);
            var paramFile = Path.GetTempFileName();
            param.ToFile(paramFile);
            var outFile = Path.GetTempFileName();
            var codeFile = GetCodeFile(param);
            var args = $"{codeFile} {paramFile} {inFile} {outFile}";
            var errorString = processInfo.ErrString;
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, ref errorString) != 0) { return null; }
            return GenerateResult(outFile, mdata, processInfo);
        }

        protected abstract IAnalysisResult GenerateResult(string outFile, IMatrixData mdata, ProcessInfo pinfo);
    }
}

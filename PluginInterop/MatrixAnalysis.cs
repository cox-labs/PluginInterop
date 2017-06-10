using System.IO;
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
        /// Create the parameters for the GUI with default of specific 'Code file' parameter and generic 'Executable'.
        /// Includes buttons /// for preview downloads of 'Data' and 'Parameters' for development purposes.
        /// Overwrite <see cref="SpecificParameters"/> to add specific parameter. Overwrite this function for full control.
        /// </summary>
        /// <param name="mdata"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public virtual Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(SpecificParameters(mdata, ref errString), "specific", false);
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
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string errorString) != 0)
            {
                processInfo.ErrString = errorString;
                return null;
            }
            return GenerateResult(outFile, mdata, processInfo);
        }

        /// <summary>
        /// Create specific processing parameters. Defaults to 'Code file'. You can provide custom parameters
        /// by overriding this function. Called by <see cref="GetParameters"/>.
        /// </summary>
        /// <param name="mdata"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        protected virtual Parameter[] SpecificParameters(IMatrixData mdata, ref string errString)
        {
            return new Parameter[] { CodeFileParam() };
        }

        protected abstract IAnalysisResult GenerateResult(string outFile, IMatrixData mdata, ProcessInfo pinfo);
    }
}

using System.Diagnostics;
using System.IO;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PluginInterop
{
    public abstract class MatrixUpload : InteropBase, IMatrixUpload
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public float DisplayRank => 1;
        public bool IsActive => true;
        public int GetMaxThreads(Parameters parameters) => 1;
        public bool HasButton => true;
        public abstract Bitmap2 DisplayImage { get; }
        public string Url { get; }
        public string[] HelpSupplTables { get; }
        public int NumSupplTables { get; }
        public string[] HelpDocuments { get; }
        public int NumDocuments { get; }

        /// <summary>
        /// Create the parameters for the GUI with default of 'Code file' and 'Executable'. Includes buttons
        /// for preview downloads of 'Parameters' for development purposes.
        /// Overwrite this function to provide custom parameters.
        /// </summary>
        /// <param name="errString"></param>
        /// <returns></returns>
        public virtual Parameters GetParameters(ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(SpecificParameters(ref errString), "specific", false);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), parametersPreviewButton }, "generic", false);
            return parameters;
        }

        /// <summary>
        /// Create specific processing parameters. Defaults to 'Code file'. You can provide custom parameters
        /// by overriding this function. Called by <see cref="GetParameters"/>.
        /// </summary>
        /// <param name="errString"></param>
        /// <returns></returns>
        protected virtual Parameter[] SpecificParameters(ref string errString)
        {
            return new Parameter[] { CodeFileParam() };
        }

        public void LoadData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            var remoteExe = GetExectuable(param);
            var paramFile = Path.GetTempFileName();
            param.ToFile(paramFile);
            var outFile = Path.GetTempFileName();
            var codeFile = GetCodeFile(param);
            var args = $"{codeFile} {paramFile} {outFile}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            };
            PerseusUtils.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');
        }
    }
}

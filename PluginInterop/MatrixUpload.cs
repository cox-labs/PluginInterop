using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public virtual bool HasButton => false;
        public virtual Bitmap2 DisplayImage => null;
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
            parameters.AddParameterGroup(new Parameter[] { CodeFileParam() }, "specific", false);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), parametersPreviewButton }, "generic", false);
            return parameters;
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
            var processInfoErrString = processInfo.ErrString;
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, ref processInfoErrString) != 0) return;
            PerseusUtils.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');
        }
    }
}

using System;
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
    public abstract class MatrixProcessing : InteropBase, IMatrixProcessing
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

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
            var inFile = Path.GetTempFileName();
            PerseusUtils.WriteMatrixToFile(mdata, inFile, false);
            var paramFile = Path.GetTempFileName();
            param.ToFile(paramFile);
            var outFile = Path.GetTempFileName();
            var codeFile = GetCodeFile(param);
            var args = $"{codeFile} {paramFile} {inFile} {outFile}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            var processInfoErrString = processInfo.ErrString;
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, ref processInfoErrString) != 0) return;
            mdata.Clear();
            PerseusUtils.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');
        }

        /// <summary>
        /// Create the parameters for the GUI with default of 'Code file' and 'Executable'. Includes buttons
        /// for preview downloads of 'Data' and 'Parameters' for development purposes.
        /// Overwrite this function to provide custom parameters.
        /// </summary>
        /// <param name="mdata"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public virtual Parameters GetParameters(IMatrixData mdata, ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(new Parameter[] { CodeFileParam() }, "specific", false);
            var previewButton = Utils.DataPreviewButton(mdata);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), previewButton, parametersPreviewButton }, "generic", false);
            return parameters;
        }
    }
}

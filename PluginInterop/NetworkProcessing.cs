using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Network;
using PerseusApi.Utils;
using PerseusLibS.Data.Network;

namespace PluginInterop
{
    public abstract class NetworkProcessing : InteropBase, INetworkProcessing
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual float DisplayRank => 1;
        public virtual bool IsActive => true;
        public virtual int GetMaxThreads(Parameters parameters) => 1;
        public virtual bool HasButton => false;
        public virtual Bitmap2 DisplayImage => null;
        public virtual string Url => "www.github.com/jdrudolph/plugininterop";
        public virtual string Heading => "External";
        public virtual string HelpOutput { get; }
        public virtual string[] HelpSupplTables { get; }
        public virtual int NumSupplTables { get; }
        public virtual string[] HelpDocuments { get; }
        public virtual int NumDocuments { get; }
        public virtual DataType[] SupplDataTypes => Enumerable.Repeat(DataType.Matrix, NumSupplTables).ToArray();

        /// <summary>
        /// Create specific processing parameters. Defaults to 'Code file'. You can provide custom parameters
        /// by overriding this function. Called by <see cref="GetParameters"/>.
        /// </summary>
        /// <param name="mdata"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        protected virtual Parameter[] SpecificParameters(INetworkData ndata, ref string errString)
        {
            return new Parameter[] {CodeFileParam()};
        }

        public void ProcessData(INetworkData ndata, Parameters param, ref IData[] supplData, ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
            var inFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            FolderFormat.Write(ndata, inFolder);
            var paramFile = Path.GetTempFileName();
            param.ToFile(paramFile);
            var outFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!TryGetCodeFile(param, out string codeFile))
            {
                processInfo.ErrString = $"Code file '{codeFile}' was not found";
                return;
            };
            var suppFiles = SupplDataTypes.Select(Utils.CreateTemporaryPath).ToArray();
            var args = $"{codeFile} {paramFile} {inFolder} {outFolder} {string.Join(" ", suppFiles)}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            };
            ndata.Clear();
            FolderFormat.Read(ndata, outFolder, processInfo);
            supplData = Utils.ReadSupplementaryData(suppFiles, SupplDataTypes, processInfo);
        }

        /// <summary>
        /// Create the parameters for the GUI with default of specific 'Code file' parameter and generic 'Executable'.
        /// Includes buttons /// for preview downloads of 'Data' and 'Parameters' for development purposes.
        /// Overwrite <see cref="SpecificParameters"/> to add specific parameter. Overwrite this function for full control.
        /// </summary>
        /// <param name="ndata"></param>
        /// <param name="errString"></param>
        /// <returns></returns>
        public Parameters GetParameters(INetworkData ndata, ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(SpecificParameters(ndata, ref errString), "specific", false);
            var previewButton = Utils.DataPreviewButton(ndata);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), previewButton, parametersPreviewButton }, "generic", false);
            return parameters;
        }
    }
}

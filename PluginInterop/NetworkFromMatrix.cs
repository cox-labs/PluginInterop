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
    public abstract class NetworkFromMatrix : InteropBase, INetworkFromMatrix
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

        public void ProcessData(IMatrixData inData, INetworkData outData, Parameters param, ref IData[] supplData, ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
            var inFile = Path.GetTempFileName();
            PerseusUtils.WriteMatrixToFile(inData, inFile, false);
            var paramFile = Path.GetTempFileName();
            param.ToFile(paramFile);
            var outFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!TryGetCodeFile(param, out string codeFile))
            {
                processInfo.ErrString = $"Code file '{codeFile}' was not found";
                return;
            }
            var suppFiles = SupplDataTypes.Select(Utils.CreateTemporaryPath).ToArray();
	        var additionalArguments = param.GetParam<string>(AdditionalArgumentsLabel).Value;
            var args = $"{codeFile} {additionalArguments} {inFile} {outFolder} {string.Join(" ", suppFiles)}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            };
            FolderFormat.Read(outData, outFolder, processInfo);
            supplData = Utils.ReadSupplementaryData(suppFiles, SupplDataTypes, processInfo);
        }

        /// <summary>
        /// Create the parameters for the GUI with default of generic 'Code file'
        /// and 'Additional arguments' parameters. Overwrite this function for custom structured parameters.
        /// </summary>
	    protected virtual Parameter[] SpecificParameters(IMatrixData data, ref string errString)
	    {
			return new Parameter[] {CodeFileParam(), AdditionalArgumentsParam()};	
	    }

        /// <summary>
        /// Create the parameters for the GUI with default of generic 'Executable', 'Code file' and 'Additional arguments' parameters.
        /// Includes buttons for preview downloads of 'Data' and 'Parameters' for development purposes.
        /// Overwrite <see cref="SpecificParameters"/> to add specific parameter. Overwrite this function for full control.
        /// </summary>
        public virtual Parameters GetParameters(IMatrixData data, ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(SpecificParameters(data, ref errString), "Specific", false);
            var previewButton = Utils.DataPreviewButton(data);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), previewButton, parametersPreviewButton }, "Generic", false);
            return parameters;
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Linq;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Generic;
using PerseusApi.Network;
using PerseusLibS.Data.Network;

namespace PluginInterop
{
    public abstract class NetworkUpload : InteropBase, INetworkUpload
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
        public virtual DataType[] SupplDataTypes => Enumerable.Repeat(DataType.Matrix, NumSupplTables).ToArray();

        /// <summary>
        /// Create the parameters for the GUI with default of 'Code file' and 'Executable'. Includes buttons
        /// for preview downloads of 'Parameters' for development purposes.
        /// Overwrite this function to provide custom parameters.
        /// </summary>
        public virtual Parameters GetParameters(ref string errString)
        {
            Parameters parameters = new Parameters();
            parameters.AddParameterGroup(SpecificParameters(ref errString), "Specific", false);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), parametersPreviewButton }, "Generic", false);
            return parameters;
        }

        /// <summary>
        /// Create specific processing parameters. Defaults to 'Code file'. You can provide custom parameters
        /// by overriding this function. Called by <see cref="GetParameters"/>.
        /// </summary>
        protected virtual Parameter[] SpecificParameters(ref string errString)
        {
            return new Parameter[] { CodeFileParam(), AdditionalArgumentsParam() };
        }

        public void LoadData(INetworkData ndata, Parameters param, ref IData[] supplData, ProcessInfo processInfo)
        {
            var remoteExe = GetExectuable(param);
            var paramFile = Path.GetTempFileName();
            param.ToFile(paramFile);
            var outFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!TryGetCodeFile(param, out string codeFile))
            {
                processInfo.ErrString = $"Code file '{codeFile}' was not found";
                return;
            };
            var suppFiles = SupplDataTypes.Select(Utils.CreateTemporaryPath).ToArray();
	        var additionalArguments = param.GetParam<string>(AdditionalArgumentsLabel).Value;
            var args = $"{codeFile} {additionalArguments} {outFolder} {string.Join(" ", suppFiles)}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            };
            FolderFormat.Read(ndata, outFolder, processInfo);
            supplData = Utils.ReadSupplementaryData(suppFiles, SupplDataTypes, processInfo);
        }
    }
}

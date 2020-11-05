using System.Diagnostics;
using System.IO;
using System.Linq;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Generic;
using PerseusApi.Network;
using PerseusApi.Network;
using PluginInterop.Properties;

namespace PluginInterop
{
    public abstract class NetworkProcessing : InteropBase, INetworkProcessingAnnColumns
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public virtual float DisplayRank => 1;
        public virtual bool IsActive => true;
        public virtual int GetMaxThreads(Parameters parameters) => 1;
        public virtual bool HasButton => false;
        public virtual Bitmap2 DisplayImage => null;
        public virtual string Url => Resources.ProjectUrl;
        public virtual string Heading => "External";
        public virtual string HelpOutput { get; }
        public virtual string[] HelpSupplTables { get; }
        public virtual int NumSupplTables { get; }
        public virtual string[] HelpDocuments { get; }
        public virtual int NumDocuments { get; }
        public virtual DataType[] SupplDataTypes => Enumerable.Repeat(DataType.Matrix, NumSupplTables).ToArray();

        public void ProcessData(INetworkDataAnnColumns ndata, Parameters param, ref IData[] supplData, ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
	        if (string.IsNullOrWhiteSpace(remoteExe))
	        {
		        processInfo.ErrString = Resources.RemoteExeNotSpecified;
	        }
            var inFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            FolderFormat.Write(ndata, inFolder);
            var outFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!TryGetCodeFile(param, out string codeFile))
            {
                processInfo.ErrString = $"Code file '{codeFile}' was not found";
                return;
            }
            var suppFiles = SupplDataTypes.Select(Utils.CreateTemporaryPath).ToArray();
	        var commandLineArguments = GetCommandLineArguments(param);
			var args = $"{codeFile} {commandLineArguments} {inFolder} {outFolder} {string.Join(" ", suppFiles)}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            }
            ndata.Clear();
            FolderFormat.Read(ndata, outFolder, processInfo);
            supplData = Utils.ReadSupplementaryData(suppFiles, SupplDataTypes, processInfo);
        }

        /// <summary>
        /// Create the parameters for the GUI with default of generic 'Code file'
        /// and 'Additional arguments' parameters. Overwrite this function for custom structured parameters.
        /// </summary>
	    protected virtual Parameter[] SpecificParameters(INetworkDataAnnColumns data, ref string errString)
	    {
			return new Parameter[] {CodeFileParam(), AdditionalArgumentsParam()};	
	    }

        /// <summary>
        /// Create the parameters for the GUI with default of generic 'Executable', 'Code file' and 'Additional arguments' parameters.
        /// Includes buttons for preview downloads of 'Data' and 'Parameters' for development purposes.
        /// Overwrite <see cref="SpecificParameters"/> to add specific parameter. Overwrite this function for full control.
        /// </summary>
        public virtual Parameters GetParameters(INetworkDataAnnColumns data, ref string errString)
        {
            Parameters parameters = new Parameters();
	        var specificParameters = SpecificParameters(data, ref errString);
	        if (!string.IsNullOrEmpty(errString))
	        {
		        return null;
	        }
            parameters.AddParameterGroup(specificParameters, "Specific", false);
            var previewButton = Utils.DataPreviewButton(data);
            var parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
            parameters.AddParameterGroup(new Parameter[] { ExecutableParam(), previewButton, parametersPreviewButton }, "Generic", false);
            return parameters;
        }
    }
}

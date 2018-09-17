using System.Diagnostics;
using System.IO;
using System.Linq;
using BaseLibS.Graph;
using BaseLibS.Param;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;
using PluginInterop.Properties;

namespace PluginInterop
{
	/// <summary>
	/// Language agnostic MatrixProcessing implementation.
	///
	/// This class serves as a basis for language-specific implementations.
	/// </summary>
    public abstract class MatrixProcessing : InteropBase, IMatrixProcessing
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
        protected virtual bool AdditionalMatrices => false;

        public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
            ProcessInfo processInfo)
        {
            var remoteExe = param.GetParam<string>(InterpreterLabel).Value;
	        if (string.IsNullOrWhiteSpace(remoteExe))
	        {
		        processInfo.ErrString = Resources.RemoteExeNotSpecified;
		        return;
	        }
            var inFile = Path.GetTempFileName();
            PerseusUtils.WriteMatrixToFile(mdata, inFile, AdditionalMatrices);
            var outFile = Path.GetTempFileName();
            if (!TryGetCodeFile(param, out string codeFile))
            {
                processInfo.ErrString = $"Code file '{codeFile}' was not found";
                return;
            };
            if (supplTables == null)
            {
                supplTables = Enumerable.Range(0, NumSupplTables).Select(i => PerseusFactory.CreateMatrixData()) .ToArray();
            }
            var suppFiles = supplTables.Select(i => Path.GetTempFileName()).ToArray();
	        var commandLineArguments = GetCommandLineArguments(param);
            var args = $"{codeFile} {commandLineArguments} {inFile} {outFile} {string.Join(" ", suppFiles)}";
            Debug.WriteLine($"executing > {remoteExe} {args}");
            if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
            {
                processInfo.ErrString = processInfoErrString;
                return;
            };
            mdata.Clear();
            PerseusUtils.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');
            for (int i = 0; i < NumSupplTables; i++)
            {
                PerseusUtils.ReadMatrixFromFile(supplTables[i], processInfo, suppFiles[i], '\t');
            }
        }

        /// <summary>
        /// Create the parameters for the GUI with default of generic 'Code file'
        /// and 'Additional arguments' parameters. Overwrite this function for custom structured parameters.
        /// You might have to additionally override <see cref="GetCommandLineArgument"/> to match
        /// your custom parameters. To pass parameters as XML file you can use <see cref="Utils.WriteParametersToFile"/>.
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

using System.IO;
using BaseLibS.Param;

namespace PluginInterop
{
	/// <summary>
	/// Base class for all interop functionality.
	///
	/// Provides virtual implementations for obtaining the path to the executable (e.g. Python), the path to the script,
	/// and unstructured parameters.
	/// </summary>
    public abstract class InteropBase
    {
		/// <summary>
		/// Interpreter path label in GUI.
		/// </summary>
        protected virtual string InterpreterLabel => "Executable";
		/// <summary>
		/// Interpreter file filter for file chooser in parameter GUI.
		/// </summary>
        protected virtual string InterpreterFilter => "Interpreter, *.exe|*.exe";

		/// <summary>
		/// Code file label in GUI.
		/// </summary>
        protected virtual string CodeLabel => "Script file";
		/// <summary>
		/// Code file filter for file chooser in parameter GUI
		/// </summary>
        protected virtual string CodeFilter => "Script";

		/// <summary>
		/// Unstructured parameters label in GUI.
		/// These parameters circumvent the usual XML serialization of parameters and are meant for simple scripts.
		/// </summary>
	    protected virtual string AdditionalArgumentsLabel => "Additional arguments";

        /// <summary>
        /// Extract the code file as a string. See <see cref="CodeFileParam"/>.
        /// </summary>
        protected virtual bool TryGetCodeFile(Parameters param, out string codeFile)
        {
            codeFile = param.GetParam<string>(CodeLabel).Value;
            if (string.IsNullOrEmpty(codeFile) || !File.Exists(codeFile))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Extract the executable name. See <see cref="ExecutableParam"/>.
        /// </summary>
        protected virtual string GetExectuable(Parameters param)
        {
            return param.GetParam<string>(InterpreterLabel).Value;
        }

        /// <summary>
        /// FileParam for specifying the exectable. See <see cref="GetExectuable"/>.
        /// </summary>
        protected virtual FileParam ExecutableParam()
        {
            var executableParam = new FileParam(InterpreterLabel) {Filter = InterpreterFilter};
	        if (TryFindExecutable(out var executable))
            {
                executableParam.Value = executable;
            }
            return executableParam;
        }

        /// <summary>
        /// FileParam for specifying the code file. See <see cref="TryGetCodeFile"/>.
        /// </summary>
        protected virtual FileParam CodeFileParam()
        {
            return new FileParam(CodeLabel) {Filter = CodeFilter};
        }

		/// <summary>
		/// Returns true and the path of the executable if found.
		/// </summary>
        protected virtual bool TryFindExecutable(out string path)
        {
            path = null;
            return false;
        }

		/// <summary>
		/// Create parameter for additional unstructured arguments.
		/// </summary>
	    protected virtual StringParam AdditionalArgumentsParam()
	    {
			return new StringParam(AdditionalArgumentsLabel);
	    }
    }
}
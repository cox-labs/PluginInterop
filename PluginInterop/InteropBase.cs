using BaseLibS.Param;

namespace PluginInterop
{
    public abstract class InteropBase
    {
        protected virtual string InterpreterLabel => "Executable";
        protected virtual string InterpreterFilter => "Interpreter, *.exe|*.exe";

        protected virtual string CodeFilter => "Script";
        protected virtual string CodeLabel => "Script file";



        /// <summary>
        /// Extract the code file as a string. See <see cref="CodeFileParam"/>.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected virtual string GetCodeFile(Parameters param)
        {
            var codeFile = param.GetParam<string>(CodeLabel).Value;
            return codeFile;
        }

        /// <summary>
        /// Extract the executable name. See <see cref="ExecutableParam"/>.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        protected virtual string GetExectuable(Parameters param)
        {
            return param.GetParam<string>(InterpreterLabel).Value;
        }

        /// <summary>
        /// FileParam for specifying the exectable. See <see cref="GetExectuable"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual FileParam ExecutableParam()
        {
            var executableParam = new FileParam(InterpreterLabel) {Filter = InterpreterFilter};
            string executable;
            if (TryFindExecutable(out executable))
            {
                executableParam.Value = executable;
            }
            return executableParam;
        }

        /// <summary>
        /// FileParam for specifying the code file. See <see cref="GetCodeFile"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual FileParam CodeFileParam()
        {
            return new FileParam(CodeLabel) {Filter = CodeFilter};
        }

        protected virtual bool TryFindExecutable(out string path)
        {
            path = null;
            return false;
        }
    }
}
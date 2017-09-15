using BaseLib.Graphic;
using BaseLibS.Graph;
using BaseLibS.Param;

namespace PluginInterop.Python
{
    public class MatrixProcessing : PluginInterop.MatrixProcessing
    {
        public override string Name => "Matrix => Python";
        public override string Description => "Run Python script";

        protected override string CodeFilter => "Python script, *.py | *.py";

        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Properties.Resources.python_logo_master_v3_TM_flattened);

        /// <summary>
        /// List of all required python packages.
        /// These packages will be checked by <see cref="ExecutableParam"/>.
        /// </summary>
        protected static string[] ReqiredPythonPackages = {"perseuspy"};

        protected override FileParam ExecutableParam()
        {
            return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable, ReqiredPythonPackages);
        }

        protected override bool TryFindExecutable(out string path)
        {
            return Utils.TryFindPythonExecutable(out path);
        }
    }
}
 
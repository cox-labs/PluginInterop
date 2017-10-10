using BaseLib.Graphic;
using BaseLibS.Graph;
using BaseLibS.Param;

namespace PluginInterop.Python
{
    public class NetworkProcessing : PluginInterop.NetworkProcessing
    {
        public override string Name => "Network => Python";
        public override string Description => "Run Python script";

        protected override string CodeFilter => "Python script, *.py | *.py";

        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Properties.Resources.python_logo_master_v3_TM_flattened);

        /// <summary>
        /// List of all required python packages.
        /// These packages will be checked by <see cref="ExecutableParam"/>.
        /// </summary>
        protected virtual string[] ReqiredPythonPackages => new [] {"perseuspy"};

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
 
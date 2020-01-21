using BaseLib.Graphic;
using BaseLibS.Graph;
using BaseLibS.Param;

namespace PluginInterop.Python
{
    public class MatrixUpload : PluginInterop.MatrixUpload
    {
        public override string Name => "Python upload";
        public override string Description => "Upload a matrix using Python";
        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Properties.Resources.python_logo_master_v3_TM_flattened);

        protected override string CodeFilter => "Python script, *.py | *.py";

        protected virtual string[] ReqiredPythonPackages => new[] { "perseuspy" };


        protected override FileParam ExecutableParam()
        {
            return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable, new []{"perseuspy"});
        }

        protected override bool TryFindExecutable(out string path)
        {
            return Utils.TryFindPythonExecutable(out path);
        }
    }
}

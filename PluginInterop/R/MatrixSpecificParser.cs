using BaseLib.Graphic;
using BaseLibS.Graph;
using BaseLibS.Param;

namespace PluginInterop.R
{
    public abstract class MatrixSpecificParser : PluginInterop.MatrixUpload
    {
        public abstract string Name { get; }
        public override string Description => "Upload a matrix using R";
        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Properties.Resources.Rlogo);

        protected override string CodeFilter => "R script, *.r | *.r";

        protected virtual string[] ReqiredPythonPackages => new[] { "perseusR" };


        protected override FileParam ExecutableParam()
        {
            return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable);
        }

        protected override bool TryFindExecutable(out string path)
        {
            return Utils.TryFindRExecutable(out path);
        }
    }
}

using BaseLib.Graphic;
using BaseLibS.Graph;
using PluginInterop.Properties;

namespace PluginInterop.R
{
    public class MatrixProcessing : PluginInterop.MatrixProcessing
    {
        public override string Name => "Matrix => R";
        public override string Description => "Run R script";
        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Resources.Rlogo);

        protected override string CodeFilter => "R script, *.R | *.R";
    }
}
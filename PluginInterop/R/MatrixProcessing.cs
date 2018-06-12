using System;
using System.Drawing;
using BaseLib.Graphic;
using BaseLibS.Graph;
using BaseLibS.Param;
using PluginInterop.Properties;

namespace PluginInterop.R
{
    public class MatrixProcessing : PluginInterop.MatrixProcessing
    {
        public override string Name => "Matrix => R";
        public override string Description => "Run R script";
        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Resources.Rlogo);

        protected override string CodeFilter => "R script, *.R | *.R";
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
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BaseLib.Graphic;
using BaseLibS.Graph;
using Path = System.IO.Path;

namespace PluginInterop.Python
{
    public class MatrixProcessing : PluginInterop.MatrixProcessing
    {
        public override string Name => "Matrix => Python";
        public override string Description => "Run Python script";

        protected override string CodeFilter => "Python script, *.py | *.py";

        public override Bitmap2 DisplayImage => GraphUtils.ToBitmap2(Properties.Resources.python_logo_master_v3_TM_flattened);

        protected override bool TryFindExecutable(out string path)
        {
            return Utils.TryFindPythonExecutable(out path);
        }
    }
}
 
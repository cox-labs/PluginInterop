using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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

        protected override FileParam ExecutableParam()
        {
            Action<string, CheckedFileParamControl> checkFileName = (s, control) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return;
                }
                if (Utils.CheckPythonInstallation(s))
                {
                    control.selectButton.BackColor = Color.LimeGreen;
                    control.ToolTip.SetToolTip(control.selectButton, "Python installation was found");
                }
                else
                {
                    control.selectButton.BackColor = Color.Red;
                    control.ToolTip.SetToolTip(control.selectButton, "A valid Python installation was not found. Make sure to select a Python installation with perseuspy installed");
                };
            };
            var fileParam = new CheckedFileParamWf(InterpreterLabel, checkFileName) {Filter = InterpreterFilter};
            string path;
            if (TryFindExecutable(out path))
            {
                fileParam.Value = path;
            }
            return fileParam;
        }

        protected override bool TryFindExecutable(out string path)
        {
            return Utils.TryFindPythonExecutable(out path);
        }
    }
}
 
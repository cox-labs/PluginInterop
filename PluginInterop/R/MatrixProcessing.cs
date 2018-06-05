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
            Action<string, CheckedFileParamControl> checkFileName = (s, control) =>
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    return;
                }
                if (Utils.CheckRInstallation(s))
                {
                    control.selectButton.BackColor = Color.LimeGreen;
                    control.ToolTip.SetToolTip(control.selectButton, "R installation was found");
                }
                else
                {
                    control.selectButton.BackColor = Color.Red;
                    control.ToolTip.SetToolTip(control.selectButton, "A valid R installation was not found. Make sure to select a R installation with 'PerseusR' installed");
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
            return Utils.TryFindRExecutable(out path);
        }
    }
}
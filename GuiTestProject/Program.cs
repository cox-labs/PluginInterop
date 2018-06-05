using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaseLib.Param;
using BaseLibS.Param;
using PluginInterop;

namespace GuiTestProject
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var buttonParam = new ButtonParamWf("labelText", "buttonText", (o, args) => { Console.WriteLine("Button clicked");});
            var fileParam = new CheckedFileParamWf("test", (s, control) =>
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    control.selectButton.BackColor = Color.Aqua;
                    control.ToolTip.SetToolTip(control.selectButton, "good job!");
                }
            });
            var processing = new PluginInterop.Python.MatrixProcessing();
            string err = "";
            var parameters = processing.GetParameters(null, ref err);
            var form = new ParameterForm(new Parameters(buttonParam, fileParam), "test", "test help", "help output", new List<string>());
            form.Load += (sender, args) =>
            {
                var form2 = new ParameterForm(parameters, "python", "asdf", "asdfsdf", new List<string>());
                form2.Show();
            };
            Application.Run(form);
        }
    }
}

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using NUnit.Framework;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PluginInterop.Test
{
    [TestFixture]
    public class PythonTest
    {
        [Test]
        public void TestNetworkFromMatrix()
        {
            if (!Python.Utils.TryFindPythonExecutable(out string _))
            {
                Assert.Inconclusive("Python not installed");
            }
            var codeString = Properties.Resources.matrix_to_network;
            var codeFile = Path.GetTempFileName();
            File.WriteAllText(codeFile, Encoding.UTF8.GetString(codeString));
            var processing = new Python.NetworkFromMatrix();
            var mdata = PerseusFactory.CreateMatrixData(new [,] {{0.0, 1.0}}, new List<string> {"col 1", "col 2"});
            var errString = string.Empty;
            var parameters = processing.GetParameters(mdata, ref errString);
            Assert.IsTrue(string.IsNullOrEmpty(errString));
            parameters.GetParam<string>("Script file").Value = codeFile;
            var ndata = PerseusFactory.CreateNetworkData();
            IMatrixData[] suppData = null;
            IDocumentData[] suppDocs = null;
            var pinfo = new ProcessInfo(new Settings(), s => { }, i => { }, 1, i => { });
            processing.ProcessData(mdata, ndata, parameters,ref suppData, ref suppDocs, pinfo);
            Assert.IsTrue(string.IsNullOrEmpty(pinfo.ErrString), pinfo.ErrString);
        }
    }
}

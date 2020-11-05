using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using PerseusApi.Generic;
using PerseusApi.Utils;

namespace PluginInterop.Test.Python
{
    [TestFixture]
    public class NetworkFromMatrixTest
    {
        [Test]
        public void TestNetworkFromMatrix()
        {
            if (!PluginInterop.Python.Utils.TryFindPythonExecutable(out string _))
            {
                Assert.Inconclusive("Python not installed");
            }
			Assert.Inconclusive("Cannot be tested without dependency on PerseusLibS");
            var codeString = Properties.Resources.matrix_to_network;
            var codeFile = Path.GetTempFileName();
            File.WriteAllText(codeFile, Encoding.UTF8.GetString(codeString));
            var processing = new PluginInterop.Python.NetworkFromMatrix();
            var mdata = PerseusFactory.CreateMatrixData(new [,] {{0.0, 1.0}}, new List<string> {"col 1", "col 2"});
            var errString = string.Empty;
            var parameters = processing.GetParameters(mdata, ref errString);
            Assert.IsTrue(string.IsNullOrEmpty(errString));
            parameters.GetParam<string>("Script file").Value = codeFile;
            var ndata = PerseusFactoryAnnColumns.CreateNetworkData();
            IData[] suppData = null;
            var pinfo = new ProcessInfo(new Settings(), s => { }, i => { }, 1);
            processing.ProcessData(mdata, ndata, parameters, ref suppData, pinfo);
            Assert.IsTrue(string.IsNullOrEmpty(pinfo.ErrString), pinfo.ErrString);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using PerseusApi.Document;
using PerseusApi.Generic;
using PerseusApi.Matrix;
using PerseusApi.Utils;

namespace PluginInterop.Test.Python
{
    [TestFixture]
    public class MatrixProcessingTest
    {
        [Test]
        public void TestMatrixProcessingWithSupplementaryTables()
        {
            if (!PluginInterop.Python.Utils.TryFindPythonExecutable(out string _))
            {
                Assert.Inconclusive("Python not installed");
            }
			Assert.Inconclusive("Cannot be tested without dependency on PerseusLibS");
            var codeString = Properties.Resources.matrix_with_supp_tables;
            var codeFile = Path.GetTempFileName();
            File.WriteAllText(codeFile, Encoding.UTF8.GetString(codeString));
            var processing = new MatrixProcessingWithSupplementaryTables();
            var mdata = PerseusFactory.CreateMatrixData(new [,] {{0.0, 1.0}}, new List<string> {"col 1", "col 2"});
            var errString = string.Empty;
            var parameters = processing.GetParameters(mdata, ref errString);
            Assert.IsTrue(string.IsNullOrEmpty(errString));
            parameters.GetParam<string>("Script file").Value = codeFile;
            IMatrixData[] suppData = null;
            IDocumentData[] suppDocs = null;
            var pinfo = new ProcessInfo(new Settings(), s => { }, i => { }, 1);
            processing.ProcessData(mdata, parameters,ref suppData, ref suppDocs, pinfo);
            Assert.IsTrue(string.IsNullOrEmpty(pinfo.ErrString), pinfo.ErrString);
            Assert.AreEqual(2, suppData.Length);
            foreach (var data in suppData)
            {
                Assert.NotNull(data);
            }
        }
    }

    internal class MatrixProcessingWithSupplementaryTables : PluginInterop.Python.MatrixProcessing
    {
        public override int NumSupplTables => 2;
    }
}

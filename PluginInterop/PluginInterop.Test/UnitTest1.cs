using System.Drawing;
using NUnit.Framework;

namespace PluginInterop.Test
{
    [TestFixture]
    public class UtilsTest
    {
        [Test]
        public void TestCheckInstallation()
        {
            var found = Python.Utils.TryFindPythonExecutable(out string exe);
            Assert.IsTrue(found);
        }

        [Test]
        public void TestCreateCheckedFileParam()
        {
            var param = Python.Utils.CreateCheckedFileParam("label", "filter", Python.Utils.TryFindPythonExecutable,
                new[] {"perseuspy"});
            Assert.AreEqual("python", param.Value);
            var paramFalse = Python.Utils.CreateCheckedFileParam("label", "filter", Python.Utils.TryFindPythonExecutable,
                new[] {"perseuspy", "nonExistingModule"});
            Assert.AreEqual(string.Empty, paramFalse.Value);
        }
    }
}

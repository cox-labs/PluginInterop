using NUnit.Framework;

namespace PluginInterop.Test.Python
{
    [TestFixture]
    public class UtilsTest
    {
        [Test]
        public void TestCheckInstallation()
        {
            var found = PluginInterop.Python.Utils.TryFindPythonExecutable(out string exe);
            Assert.IsTrue(found);
        }

        [Test]
        public void TestCreateCheckedFileParam()
        {
            var param = PluginInterop.Python.Utils.CreateCheckedFileParam("label", "filter", PluginInterop.Python.Utils.TryFindPythonExecutable,
                new[] {"perseuspy"});
            Assert.AreEqual("python", param.Value);
            Assert.IsTrue(PluginInterop.Python.Utils.CheckPythonInstallation(param.Value, new[] {"perseuspy"}));
            var paramFalse = PluginInterop.Python.Utils.CreateCheckedFileParam("label", "filter", PluginInterop.Python.Utils.TryFindPythonExecutable,
                new[] {"perseuspy", "nonExistingModule"});
            Assert.AreEqual("python", paramFalse.Value);
            Assert.IsFalse(PluginInterop.Python.Utils.CheckPythonInstallation(paramFalse.Value, new[] {"perseuspy", "nonExistingModule"}));
        }
    }
}

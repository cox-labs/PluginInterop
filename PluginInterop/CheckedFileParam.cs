using System;
using BaseLibS.Param;

namespace PluginInterop
{
    [Serializable]
    public class CheckedFileParamWf : FileParam
    {
        [NonSerialized]
        private CheckedFileParamControl control;
        public CheckedFileParamWf(string name, Action<string, CheckedFileParamControl> checkFileName) : this(name, "", checkFileName) { }
        public CheckedFileParamWf(string name, string value, Action<string, CheckedFileParamControl> checkFileName) : base(name, value)
        {
            CheckFileName = checkFileName;
        }
        public override ParamType Type => ParamType.WinForms;

        [NonSerialized]
        private Action<string, CheckedFileParamControl> CheckFileName;

        public override void SetValueFromControl()
        {
            CheckedFileParamControl vm = control;
            Value = vm.FileName;
        }

        public override void UpdateControlFromValue()
        {
            if (control == null)
            {
                return;
            }
            CheckedFileParamControl vm = control;
            vm.FileName = Value;
        }

        public override object CreateControl()
        {
            control = new CheckedFileParamControl(Value, Filter, ProcessFileName, CheckFileName);
            return control;
        }
    }
}

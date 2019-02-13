using System;
using System.Windows.Forms;
using BaseLibS.Param;

namespace PluginInterop
{
    [Serializable]
    public class ButtonParamWf : LabelParam
    {
        private readonly Button _button;
        public ButtonParamWf(string labelText, string buttonText, Action<object,EventArgs> clickHandler) : base(labelText)
        {
            _button = new Button {Text = buttonText};
            _button.Click += (obj, args) => clickHandler(obj, args);
        }
        public override object CreateControl()
        {
            return _button;
        }
        public override ParamType Type => ParamType.WinForms;

    }
}

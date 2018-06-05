using System;
using System.Windows.Forms;

namespace PluginInterop
{
    public partial class CheckedFileParamControl : UserControl
    {
        private readonly Func<string, string> _processFileName;
        private readonly Action<string, CheckedFileParamControl> _checkFileName;
        private bool save;

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                filePathTextBox.Text = value;
                _checkFileName(value, this);
            }
        }

        private OpenFileDialog _dialog;
        public ToolTip ToolTip;

        public CheckedFileParamControl(string value, string filter, Func<string, string> processFileName, Action<string, CheckedFileParamControl> checkFileName)
        {
            InitializeComponent();
            ToolTip = new ToolTip();
            _checkFileName = checkFileName ?? ((s, b) => { });
            FileName = value;
            _dialog = new OpenFileDialog();
            if (!string.IsNullOrEmpty(filter))
            {
                _dialog.Filter = filter;
            }
            _processFileName = processFileName ?? (s => s);
        }

        private void ChooseFile(object sender, EventArgs e)
        {
            if (_dialog.ShowDialog() == DialogResult.OK)
            {
                FileName = _processFileName(_dialog.FileName);
            }
        }
    }
}

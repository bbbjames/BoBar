using System.Drawing;

namespace BoBar
{
    public partial class EditLaunchItemForm : Form
    {
        private readonly LaunchItem _item;
        
        private TextBox _nameTextBox = null!;
        private TextBox _pathTextBox = null!;
        private TextBox _argsTextBox = null!;
        private Button _browseButton = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;

        public EditLaunchItemForm(LaunchItem item)
        {
            _item = item;
            InitializeComponent();
            LoadItemData();
        }

        private void InitializeComponent()
        {
            Text = "Edit Launch Item";
            Size = new Size(450, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // Name
            var nameLabel = new Label { Text = "Name:", Location = new Point(12, 15), Size = new Size(50, 23) };
            _nameTextBox = new TextBox { Location = new Point(80, 12), Size = new Size(350, 23) };
            Controls.AddRange(new Control[] { nameLabel, _nameTextBox });

            // Path
            var pathLabel = new Label { Text = "Path:", Location = new Point(12, 45), Size = new Size(50, 23) };
            _pathTextBox = new TextBox { Location = new Point(80, 42), Size = new Size(270, 23) };
            _browseButton = new Button { Text = "Browse...", Location = new Point(355, 41), Size = new Size(75, 25) };
            _browseButton.Click += BrowseButton_Click;
            Controls.AddRange(new Control[] { pathLabel, _pathTextBox, _browseButton });

            // Arguments
            var argsLabel = new Label { Text = "Args:", Location = new Point(12, 75), Size = new Size(50, 23) };
            _argsTextBox = new TextBox { Location = new Point(80, 72), Size = new Size(350, 23) };
            Controls.AddRange(new Control[] { argsLabel, _argsTextBox });

            // Buttons
            _okButton = new Button { Text = "OK", Location = new Point(275, 130), Size = new Size(75, 23), DialogResult = DialogResult.OK };
            _cancelButton = new Button { Text = "Cancel", Location = new Point(355, 130), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };
            
            _okButton.Click += OkButton_Click;
            Controls.AddRange(new Control[] { _okButton, _cancelButton });

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void LoadItemData()
        {
            _nameTextBox.Text = _item.Name;
            _pathTextBox.Text = _item.ExecutablePath;
            _argsTextBox.Text = _item.Arguments;
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Executable"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _pathTextBox.Text = dialog.FileName;
                
                // Auto-update name if it's currently empty or matches the old filename
                if (string.IsNullOrEmpty(_nameTextBox.Text) || 
                    _nameTextBox.Text == Path.GetFileNameWithoutExtension(_item.ExecutablePath))
                {
                    _nameTextBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_pathTextBox.Text))
            {
                MessageBox.Show("Executable path is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(_pathTextBox.Text))
            {
                MessageBox.Show("The specified executable file does not exist.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Store old path to detect changes
            var oldPath = _item.ExecutablePath;

            // Update the item
            _item.Name = _nameTextBox.Text.Trim();
            _item.ExecutablePath = _pathTextBox.Text.Trim();
            _item.Arguments = _argsTextBox.Text.Trim();

            // Re-extract icon if path changed
            if (oldPath != _item.ExecutablePath)
            {
                try
                {
                    var newItem = new LaunchItem(_item.ExecutablePath);
                    _item.IconPath = newItem.IconPath;
                }
                catch
                {
                    // Icon extraction failed, keep existing icon
                }
            }
        }
    }
}
using System.Drawing;

namespace BoBar;
public partial class EditLaunchItemForm : Form
{
    private readonly LaunchItem _item;

    private GroupBox _detailsGroup = null!;
    private TextBox _nameTextBox = null!;
    private TextBox _pathTextBox = null!;
    private TextBox _argsTextBox = null!;
    private TextBox _workDirTextBox = null!;
    private Button _browseButton = null!;
    private Button _browseWorkDirButton = null!;
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
        Size = new Size(740, 390);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        _detailsGroup = new GroupBox();
        _detailsGroup.SuspendLayout();
        SuspendLayout();

        // GroupBox for details
        _detailsGroup.Text = "Item Details";
        _detailsGroup.Font = new Font(Font, FontStyle.Bold);
        _detailsGroup.Location = new Point(20, 20);
        _detailsGroup.Size = new Size(700, 280);
        _detailsGroup.TabIndex = 0;

        int leftMargin = 20;
        int labelWidth = 90;
        int controlLeft = leftMargin + labelWidth + 10;
        int controlWidth = 480;
        int browseWidth = 90;
        int rowHeight = 45;
        int currentY = 35;

        // Name
        var nameLabel = new Label
        {
            Text = "Name:",
            Location = new Point(leftMargin, currentY + 6),
            Size = new Size(labelWidth, 28),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular)
        };
        _nameTextBox = new TextBox
        {
            Location = new Point(controlLeft, currentY),
            Size = new Size(controlWidth + browseWidth + 10, 30),
            Font = new Font(Font.FontFamily, 9.5f)
        };
        _detailsGroup.Controls.AddRange(new Control[] { nameLabel, _nameTextBox });

        currentY += rowHeight;

        // Path
        var pathLabel = new Label
        {
            Text = "Path:",
            Location = new Point(leftMargin, currentY + 6),
            Size = new Size(labelWidth, 28),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular)
        };
        _pathTextBox = new TextBox
        {
            Location = new Point(controlLeft, currentY),
            Size = new Size(controlWidth, 30),
            Font = new Font(Font.FontFamily, 9.5f)
        };
        _browseButton = new Button
        {
            Text = "Browse...",
            Location = new Point(controlLeft + controlWidth + 10, currentY - 1),
            Size = new Size(browseWidth, 32),
            Font = new Font(Font.FontFamily, 9f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        _browseButton.Click += BrowseButton_Click;
        _detailsGroup.Controls.AddRange(new Control[] { pathLabel, _pathTextBox, _browseButton });

        currentY += rowHeight;

        // Arguments
        var argsLabel = new Label
        {
            Text = "Arguments:",
            Location = new Point(leftMargin, currentY + 6),
            Size = new Size(labelWidth, 28),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular)
        };
        _argsTextBox = new TextBox
        {
            Location = new Point(controlLeft, currentY),
            Size = new Size(controlWidth + browseWidth + 10, 30),
            Font = new Font(Font.FontFamily, 9.5f),
            ForeColor = Color.DimGray
        };
        _detailsGroup.Controls.AddRange(new Control[] { argsLabel, _argsTextBox });

        currentY += rowHeight;

        // Working Directory
        var workDirLabel = new Label
        {
            Text = "Start in:",
            Location = new Point(leftMargin, currentY + 6),
            Size = new Size(labelWidth, 28),
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular)
        };
        _workDirTextBox = new TextBox
        {
            Location = new Point(controlLeft, currentY),
            Size = new Size(controlWidth, 30),
            Font = new Font(Font.FontFamily, 9.5f),
            ForeColor = Color.DimGray
        };
        _browseWorkDirButton = new Button
        {
            Text = "Browse...",
            Location = new Point(controlLeft + controlWidth + 10, currentY - 1),
            Size = new Size(browseWidth, 32),
            Font = new Font(Font.FontFamily, 9f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };
        _browseWorkDirButton.Click += BrowseWorkDirButton_Click;
        _detailsGroup.Controls.AddRange(new Control[] { workDirLabel, _workDirTextBox, _browseWorkDirButton });

        currentY += rowHeight;

        // Hint label
        var hintLabel = new Label
        {
            Text = "Leave 'Start in' empty to use the executable's folder",
            Location = new Point(controlLeft, currentY + 5),
            Size = new Size(controlWidth + browseWidth + 10, 40),
            ForeColor = Color.Gray,
            Font = new Font(Font.FontFamily, 8.5f, FontStyle.Italic)
        };
        _detailsGroup.Controls.Add(hintLabel);

        Controls.Add(_detailsGroup);

        // Buttons
        _okButton = new Button
        {
            Text = "Save",
            Location = new Point(520, 320),
            Size = new Size(95, 42),
            DialogResult = DialogResult.OK,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font(Font.FontFamily, 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _okButton.FlatAppearance.BorderSize = 0;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(625, 320),
            Size = new Size(95, 42),
            DialogResult = DialogResult.Cancel,
            Font = new Font(Font.FontFamily, 9f, FontStyle.Regular),
            Cursor = Cursors.Hand
        };

        _okButton.Click += OkButton_Click;
        Controls.AddRange(new Control[] { _okButton, _cancelButton });

        _detailsGroup.ResumeLayout(false);
        ResumeLayout(false);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void LoadItemData()
    {
        _nameTextBox.Text = _item.Name;
        _pathTextBox.Text = _item.ExecutablePath;
        _argsTextBox.Text = _item.Arguments;
        _workDirTextBox.Text = _item.WorkingDirectory;
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

            // Auto-update working directory to the new executable's directory
            try
            {
                var newDir = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(newDir))
                {
                    _workDirTextBox.Text = newDir;
                }
            }
            catch
            {
                // Ignore errors when getting directory
            }
        }
    }

    private void BrowseWorkDirButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Working Directory (Start in folder)",
            ShowNewFolderButton = true
        };

        // Set initial directory if one exists
        if (!string.IsNullOrEmpty(_workDirTextBox.Text) && Directory.Exists(_workDirTextBox.Text))
        {
            dialog.SelectedPath = _workDirTextBox.Text;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _workDirTextBox.Text = dialog.SelectedPath;
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

        // Validate working directory if specified
        var workDir = _workDirTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(workDir) && !Directory.Exists(workDir))
        {
            var result = MessageBox.Show(
                "The specified working directory does not exist. Continue anyway?",
                "Working Directory Not Found",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }
        }

        // Store old path to detect changes
        var oldPath = _item.ExecutablePath;

        // Update the item
        _item.Name = _nameTextBox.Text.Trim();
        _item.ExecutablePath = _pathTextBox.Text.Trim();
        _item.Arguments = _argsTextBox.Text.Trim();
        _item.WorkingDirectory = workDir;

        // Re-extract icon if path changed
        if (oldPath != _item.ExecutablePath)
        {
            try
            {
                var newItem = new LaunchItem(_item.ExecutablePath);
                _item.IconPath = newItem.IconPath;

                // Update working directory if not manually set
                if (string.IsNullOrWhiteSpace(_item.WorkingDirectory))
                {
                    _item.WorkingDirectory = newItem.WorkingDirectory;
                }
            }
            catch
            {
                // Icon extraction failed, keep existing icon
            }
        }
    }
}

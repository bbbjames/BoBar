using System.Drawing;

namespace BoBar;
public partial class SettingsForm : Form
{
    private readonly ConfigurationManager _configManager;
    private readonly List<LaunchItem> _launchItems;
    private readonly Action _onSaved;
    private bool _hasUnsavedChanges;
    private bool _closingHandled;

    private ListBox _itemsList = null!;
    private Panel _dropPanel = null!;
    private Button _removeButton = null!;
    private Button _editButton = null!;
    private Button _moveUpButton = null!;
    private Button _moveDownButton = null!;
    private Button _cancelButton = null!;
    private GroupBox leftGroup = null!;
    private GroupBox rightGroup = null!;
    private Label addLabel = null!;
    private Label dropInstruction = null!;
    private Label separator1 = null!;
    private Label editLabel = null!;
    private Label separator2 = null!;
    private Label reorderLabel = null!;
    private Button _saveAndCloseButton = null!;

    public SettingsForm(ConfigurationManager configManager, List<LaunchItem> launchItems, Action onSaved)
    {
        _configManager = configManager;
        _launchItems = new List<LaunchItem>(launchItems);
        _onSaved = onSaved;

        InitializeComponent();
        SetupDragDrop();
        RefreshItemsList();

        FormClosing += SettingsForm_FormClosing;
    }

    private void InitializeComponent()
    {
        leftGroup = new GroupBox();
        _itemsList = new ListBox();
        rightGroup = new GroupBox();
        addLabel = new Label();
        _dropPanel = new Panel();
        dropInstruction = new Label();
        separator1 = new Label();
        editLabel = new Label();
        _editButton = new Button();
        _removeButton = new Button();
        separator2 = new Label();
        reorderLabel = new Label();
        _moveUpButton = new Button();
        _moveDownButton = new Button();
        _saveAndCloseButton = new Button();
        _cancelButton = new Button();
        leftGroup.SuspendLayout();
        rightGroup.SuspendLayout();
        _dropPanel.SuspendLayout();
        SuspendLayout();
        //
        // leftGroup
        //
        leftGroup.Controls.Add(_itemsList);
        leftGroup.Font = new Font(Font, FontStyle.Bold);
        leftGroup.Location = new Point(25, 25);
        leftGroup.Name = "leftGroup";
        leftGroup.Size = new Size(450, 600);
        leftGroup.TabIndex = 0;
        leftGroup.TabStop = false;
        leftGroup.Text = "Launch Items";
        //
        // _itemsList
        //
        _itemsList.DisplayMember = "Name";
        _itemsList.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _itemsList.FormattingEnabled = true;
        _itemsList.ItemHeight = 18;
        _itemsList.Location = new Point(18, 35);
        _itemsList.Name = "_itemsList";
        _itemsList.Size = new Size(414, 545);
        _itemsList.TabIndex = 0;
        _itemsList.SelectedIndexChanged += ItemsList_SelectedIndexChanged;
        //
        // rightGroup
        //
        rightGroup.Controls.Add(addLabel);
        rightGroup.Controls.Add(_dropPanel);
        rightGroup.Controls.Add(separator1);
        rightGroup.Controls.Add(editLabel);
        rightGroup.Controls.Add(_editButton);
        rightGroup.Controls.Add(_removeButton);
        rightGroup.Controls.Add(separator2);
        rightGroup.Controls.Add(reorderLabel);
        rightGroup.Controls.Add(_moveUpButton);
        rightGroup.Controls.Add(_moveDownButton);
        rightGroup.Font = new Font(Font, FontStyle.Bold);
        rightGroup.Location = new Point(495, 25);
        rightGroup.Name = "rightGroup";
        rightGroup.Size = new Size(280, 600);
        rightGroup.TabIndex = 1;
        rightGroup.TabStop = false;
        rightGroup.Text = "Actions";
        //
        // addLabel
        //
        addLabel.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold);
        addLabel.Location = new Point(18, 38);
        addLabel.Name = "addLabel";
        addLabel.Size = new Size(244, 28);
        addLabel.TabIndex = 0;
        addLabel.Text = "Add Items";
        //
        // _dropPanel
        //
        _dropPanel.BackColor = Color.FromArgb(248, 248, 248);
        _dropPanel.BorderStyle = BorderStyle.FixedSingle;
        _dropPanel.Controls.Add(dropInstruction);
        _dropPanel.Location = new Point(18, 68);
        _dropPanel.Name = "_dropPanel";
        _dropPanel.Size = new Size(244, 125);
        _dropPanel.TabIndex = 1;
        //
        // dropInstruction
        //
        dropInstruction.Dock = DockStyle.Fill;
        dropInstruction.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        dropInstruction.ForeColor = Color.FromArgb(100, 100, 100);
        dropInstruction.Location = new Point(0, 0);
        dropInstruction.Name = "dropInstruction";
        dropInstruction.Size = new Size(242, 123);
        dropInstruction.TabIndex = 0;
        dropInstruction.Text = "Drag .exe files here\r\n\r\nto add to launcher";
        dropInstruction.TextAlign = ContentAlignment.MiddleCenter;
        //
        // separator1
        //
        separator1.BorderStyle = BorderStyle.FixedSingle;
        separator1.Location = new Point(18, 213);
        separator1.Name = "separator1";
        separator1.Size = new Size(244, 1);
        separator1.TabIndex = 2;
        //
        // editLabel
        //
        editLabel.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold);
        editLabel.Location = new Point(18, 233);
        editLabel.Name = "editLabel";
        editLabel.Size = new Size(244, 28);
        editLabel.TabIndex = 3;
        editLabel.Text = "Modify Selected";
        //
        // _editButton
        //
        _editButton.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _editButton.Location = new Point(18, 268);
        _editButton.Name = "_editButton";
        _editButton.Size = new Size(244, 45);
        _editButton.TabIndex = 4;
        _editButton.Text = "✎  Edit Item";
        _editButton.UseVisualStyleBackColor = true;
        _editButton.Cursor = Cursors.Hand;
        _editButton.Click += EditButton_Click;
        //
        // _removeButton
        //
        _removeButton.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _removeButton.ForeColor = Color.FromArgb(180, 0, 0);
        _removeButton.Location = new Point(18, 321);
        _removeButton.Name = "_removeButton";
        _removeButton.Size = new Size(244, 45);
        _removeButton.TabIndex = 5;
        _removeButton.Text = "✕  Remove Item";
        _removeButton.UseVisualStyleBackColor = true;
        _removeButton.Cursor = Cursors.Hand;
        _removeButton.Click += RemoveButton_Click;
        //
        // separator2
        //
        separator2.BorderStyle = BorderStyle.FixedSingle;
        separator2.Location = new Point(18, 380);
        separator2.Name = "separator2";
        separator2.Size = new Size(244, 1);
        separator2.TabIndex = 6;
        //
        // reorderLabel
        //
        reorderLabel.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold);
        reorderLabel.Location = new Point(18, 400);
        reorderLabel.Name = "reorderLabel";
        reorderLabel.Size = new Size(244, 28);
        reorderLabel.TabIndex = 7;
        reorderLabel.Text = "Change Order";
        //
        // _moveUpButton
        //
        _moveUpButton.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _moveUpButton.Location = new Point(18, 438);
        _moveUpButton.Name = "_moveUpButton";
        _moveUpButton.Size = new Size(244, 43);
        _moveUpButton.TabIndex = 8;
        _moveUpButton.Text = "▲  Move Up";
        _moveUpButton.UseVisualStyleBackColor = true;
        _moveUpButton.Cursor = Cursors.Hand;
        _moveUpButton.Click += MoveUpButton_Click;
        //
        // _moveDownButton
        //
        _moveDownButton.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _moveDownButton.Location = new Point(18, 489);
        _moveDownButton.Name = "_moveDownButton";
        _moveDownButton.Size = new Size(244, 43);
        _moveDownButton.TabIndex = 9;
        _moveDownButton.Text = "▼  Move Down";
        _moveDownButton.UseVisualStyleBackColor = true;
        _moveDownButton.Cursor = Cursors.Hand;
        _moveDownButton.Click += MoveDownButton_Click;
        //
        // _saveAndCloseButton
        //
        _saveAndCloseButton.BackColor = Color.FromArgb(0, 120, 215);
        _saveAndCloseButton.Cursor = Cursors.Hand;
        _saveAndCloseButton.FlatAppearance.BorderSize = 0;
        _saveAndCloseButton.FlatStyle = FlatStyle.Flat;
        _saveAndCloseButton.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold);
        _saveAndCloseButton.ForeColor = Color.White;
        _saveAndCloseButton.Location = new Point(440, 645);
        _saveAndCloseButton.Name = "_saveAndCloseButton";
        _saveAndCloseButton.Size = new Size(160, 50);
        _saveAndCloseButton.TabIndex = 2;
        _saveAndCloseButton.Text = "Save && Close";
        _saveAndCloseButton.UseVisualStyleBackColor = false;
        _saveAndCloseButton.Click += SaveAndCloseButton_Click;
        //
        // _cancelButton
        //
        _cancelButton.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Regular);
        _cancelButton.Location = new Point(615, 645);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(160, 50);
        _cancelButton.TabIndex = 3;
        _cancelButton.Text = "Discard Changes";
        _cancelButton.Cursor = Cursors.Hand;
        _cancelButton.UseVisualStyleBackColor = true;
        _cancelButton.Click += CancelButton_Click;
        //
        // SettingsForm
        //
        ClientSize = new Size(805, 720);
        Controls.Add(leftGroup);
        Controls.Add(rightGroup);
        Controls.Add(_saveAndCloseButton);
        Controls.Add(_cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SettingsForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "BoBar Settings - Manage Launch Items";
        leftGroup.ResumeLayout(false);
        rightGroup.ResumeLayout(false);
        _dropPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void SetupDragDrop()
    {
        _dropPanel.AllowDrop = true;
        _dropPanel.DragEnter += DropPanel_DragEnter;
        _dropPanel.DragDrop += DropPanel_DragDrop;
    }

    private void DropPanel_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            e.Effect = files.Any(f => Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
    }

    private void DropPanel_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            foreach (var file in files.Where(f => Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase)))
            {
                AddLaunchItem(file);
            }
        }
    }

    private void AddLaunchItem(string executablePath)
    {
        var item = new LaunchItem(executablePath)
        {
            Order = _launchItems.Count
        };

        _launchItems.Add(item);
        _hasUnsavedChanges = true;
        RefreshItemsList();
        _itemsList.SelectedIndex = _launchItems.Count - 1;
    }

    private void RefreshItemsList()
    {
        _itemsList.DataSource = null;
        _itemsList.DisplayMember = "Name";
        _itemsList.DataSource = _launchItems;
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _itemsList.SelectedIndex >= 0;
        var selectedIndex = _itemsList.SelectedIndex;
        
        _removeButton.Enabled = hasSelection;
        _editButton.Enabled = hasSelection;
        _moveUpButton.Enabled = hasSelection && selectedIndex > 0;
        _moveDownButton.Enabled = hasSelection && selectedIndex < _launchItems.Count - 1;
    }

    private void ItemsList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        if (_itemsList.SelectedIndex >= 0)
        {
            var itemName = _launchItems[_itemsList.SelectedIndex].Name;
            var result = MessageBox.Show(
                $"Remove '{itemName}' from launch items?",
                "Confirm Remove",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _launchItems.RemoveAt(_itemsList.SelectedIndex);
                _hasUnsavedChanges = true;
                RefreshItemsList();
            }
        }
    }

    private void EditButton_Click(object? sender, EventArgs e)
    {
        if (_itemsList.SelectedIndex >= 0)
        {
            var item = _launchItems[_itemsList.SelectedIndex];
            using var editForm = new EditLaunchItemForm(item);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                _hasUnsavedChanges = true;
                RefreshItemsList();
            }
        }
    }

    private void MoveUpButton_Click(object? sender, EventArgs e)
    {
        var index = _itemsList.SelectedIndex;
        if (index > 0)
        {
            (_launchItems[index], _launchItems[index - 1]) = (_launchItems[index - 1], _launchItems[index]);
            _hasUnsavedChanges = true;
            RefreshItemsList();
            _itemsList.SelectedIndex = index - 1;
        }
    }

    private void MoveDownButton_Click(object? sender, EventArgs e)
    {
        var index = _itemsList.SelectedIndex;
        if (index < _launchItems.Count - 1)
        {
            (_launchItems[index], _launchItems[index + 1]) = (_launchItems[index + 1], _launchItems[index]);
            _hasUnsavedChanges = true;
            RefreshItemsList();
            _itemsList.SelectedIndex = index + 1;
        }
    }

    private void SaveAndCloseButton_Click(object? sender, EventArgs e)
    {
        _closingHandled = true;
        SaveChanges();
        DialogResult = DialogResult.OK;
        Close();
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        if (_hasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Are you sure you want to discard them?",
                "Unsaved Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                return;
            }
        }

        _closingHandled = true;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void SettingsForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // If closing wasn't triggered by our buttons and we have unsaved changes, prompt to save
        if (!_closingHandled && _hasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Would you like to save them before closing?",
                "Save Changes?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveChanges();
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true; // Cancel the close operation
            }
        }
    }

    private void SaveChanges()
    {
        // Update order based on current list position
        for (int i = 0; i < _launchItems.Count; i++)
        {
            _launchItems[i].Order = i;
        }

        _configManager.SaveLaunchItems(_launchItems);
        _hasUnsavedChanges = false;
        _onSaved();
    }
}

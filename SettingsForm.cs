using System.Drawing;

namespace BoBar
{
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
            leftGroup.Location = new Point(20, 20);
            leftGroup.Name = "leftGroup";
            leftGroup.Size = new Size(420, 560);
            leftGroup.TabIndex = 0;
            leftGroup.TabStop = false;
            leftGroup.Text = "Launch Items";
            //
            // _itemsList
            //
            _itemsList.DisplayMember = "Name";
            _itemsList.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Regular);
            _itemsList.FormattingEnabled = true;
            _itemsList.Location = new Point(15, 30);
            _itemsList.Name = "_itemsList";
            _itemsList.Size = new Size(390, 515);
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
            rightGroup.Location = new Point(460, 20);
            rightGroup.Name = "rightGroup";
            rightGroup.Size = new Size(260, 560);
            rightGroup.TabIndex = 1;
            rightGroup.TabStop = false;
            rightGroup.Text = "Actions";
            //
            // addLabel
            //
            addLabel.Font = new Font(Font.FontFamily, Font.Size + 1, FontStyle.Bold);
            addLabel.Location = new Point(15, 30);
            addLabel.Name = "addLabel";
            addLabel.Size = new Size(230, 22);
            addLabel.TabIndex = 0;
            addLabel.Text = "Add Items";
            //
            // _dropPanel
            //
            _dropPanel.BackColor = Color.FromArgb(245, 245, 245);
            _dropPanel.BorderStyle = BorderStyle.FixedSingle;
            _dropPanel.Controls.Add(dropInstruction);
            _dropPanel.Location = new Point(15, 60);
            _dropPanel.Name = "_dropPanel";
            _dropPanel.Size = new Size(230, 110);
            _dropPanel.TabIndex = 1;
            //
            // dropInstruction
            //
            dropInstruction.Dock = DockStyle.Fill;
            dropInstruction.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Regular);
            dropInstruction.ForeColor = Color.FromArgb(100, 100, 100);
            dropInstruction.Location = new Point(0, 0);
            dropInstruction.Name = "dropInstruction";
            dropInstruction.Size = new Size(228, 108);
            dropInstruction.TabIndex = 0;
            dropInstruction.Text = "Drag .exe files here\r\n\r\nto add to launcher";
            dropInstruction.TextAlign = ContentAlignment.MiddleCenter;
            //
            // separator1
            //
            separator1.BorderStyle = BorderStyle.Fixed3D;
            separator1.Location = new Point(15, 190);
            separator1.Name = "separator1";
            separator1.Size = new Size(230, 2);
            separator1.TabIndex = 2;
            //
            // editLabel
            //
            editLabel.Font = new Font(Font.FontFamily, Font.Size + 1, FontStyle.Bold);
            editLabel.Location = new Point(15, 210);
            editLabel.Name = "editLabel";
            editLabel.Size = new Size(230, 22);
            editLabel.TabIndex = 3;
            editLabel.Text = "Modify Selected";
            //
            // _editButton
            //
            _editButton.Font = new Font(Font.FontFamily, Font.Size + 0.5f, FontStyle.Regular);
            _editButton.Location = new Point(15, 245);
            _editButton.Name = "_editButton";
            _editButton.Size = new Size(230, 40);
            _editButton.TabIndex = 4;
            _editButton.Text = "✎  Edit Item";
            _editButton.UseVisualStyleBackColor = true;
            _editButton.Click += EditButton_Click;
            //
            // _removeButton
            //
            _removeButton.Font = new Font(Font.FontFamily, Font.Size + 0.5f, FontStyle.Regular);
            _removeButton.ForeColor = Color.FromArgb(180, 0, 0);
            _removeButton.Location = new Point(15, 295);
            _removeButton.Name = "_removeButton";
            _removeButton.Size = new Size(230, 40);
            _removeButton.TabIndex = 5;
            _removeButton.Text = "✕  Remove Item";
            _removeButton.UseVisualStyleBackColor = true;
            _removeButton.Click += RemoveButton_Click;
            //
            // separator2
            //
            separator2.BorderStyle = BorderStyle.Fixed3D;
            separator2.Location = new Point(15, 355);
            separator2.Name = "separator2";
            separator2.Size = new Size(230, 2);
            separator2.TabIndex = 6;
            //
            // reorderLabel
            //
            reorderLabel.Font = new Font(Font.FontFamily, Font.Size + 1, FontStyle.Bold);
            reorderLabel.Location = new Point(15, 375);
            reorderLabel.Name = "reorderLabel";
            reorderLabel.Size = new Size(230, 22);
            reorderLabel.TabIndex = 7;
            reorderLabel.Text = "Change Order";
            //
            // _moveUpButton
            //
            _moveUpButton.Font = new Font(Font.FontFamily, Font.Size + 0.5f, FontStyle.Regular);
            _moveUpButton.Location = new Point(15, 410);
            _moveUpButton.Name = "_moveUpButton";
            _moveUpButton.Size = new Size(230, 35);
            _moveUpButton.TabIndex = 8;
            _moveUpButton.Text = "▲  Move Up";
            _moveUpButton.UseVisualStyleBackColor = true;
            _moveUpButton.Click += MoveUpButton_Click;
            //
            // _moveDownButton
            //
            _moveDownButton.Font = new Font(Font.FontFamily, Font.Size + 0.5f, FontStyle.Regular);
            _moveDownButton.Location = new Point(15, 450);
            _moveDownButton.Name = "_moveDownButton";
            _moveDownButton.Size = new Size(230, 35);
            _moveDownButton.TabIndex = 9;
            _moveDownButton.Text = "▼  Move Down";
            _moveDownButton.UseVisualStyleBackColor = true;
            _moveDownButton.Click += MoveDownButton_Click;
            //
            // _saveAndCloseButton
            //
            _saveAndCloseButton.BackColor = Color.FromArgb(0, 120, 215);
            _saveAndCloseButton.Cursor = Cursors.Hand;
            _saveAndCloseButton.FlatAppearance.BorderSize = 0;
            _saveAndCloseButton.FlatStyle = FlatStyle.Flat;
            _saveAndCloseButton.Font = new Font(Font.FontFamily, Font.Size + 1, FontStyle.Bold);
            _saveAndCloseButton.ForeColor = Color.White;
            _saveAndCloseButton.Location = new Point(410, 600);
            _saveAndCloseButton.Name = "_saveAndCloseButton";
            _saveAndCloseButton.Size = new Size(150, 45);
            _saveAndCloseButton.TabIndex = 2;
            _saveAndCloseButton.Text = "Save && Close";
            _saveAndCloseButton.UseVisualStyleBackColor = false;
            _saveAndCloseButton.Click += SaveAndCloseButton_Click;
            //
            // _cancelButton
            //
            _cancelButton.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Regular);
            _cancelButton.Location = new Point(570, 600);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new Size(150, 45);
            _cancelButton.TabIndex = 3;
            _cancelButton.Text = "Discard Changes";
            _cancelButton.UseVisualStyleBackColor = true;
            _cancelButton.Click += CancelButton_Click;
            //
            // SettingsForm
            //
            ClientSize = new Size(750, 670);
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
}
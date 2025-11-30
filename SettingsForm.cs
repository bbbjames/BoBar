using System.Drawing;

namespace BoBar
{
    public partial class SettingsForm : Form
    {
        private readonly ConfigurationManager _configManager;
        private readonly List<LaunchItem> _launchItems;
        private readonly Action _onSaved;

        private ListBox _itemsList = null!;
        private Panel _dropPanel = null!;
        private Button _removeButton = null!;
        private Button _editButton = null!;
        private Button _moveUpButton = null!;
        private Button _moveDownButton = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;

        public SettingsForm(ConfigurationManager configManager, List<LaunchItem> launchItems, Action onSaved)
        {
            _configManager = configManager;
            _launchItems = new List<LaunchItem>(launchItems);
            _onSaved = onSaved;
            
            InitializeComponent();
            SetupDragDrop();
            RefreshItemsList();
        }

        private void InitializeComponent()
        {
            Text = "BoBar Settings";
            Size = new Size(500, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // Items list
            _itemsList = new ListBox
            {
                Location = new Point(12, 12),
                Size = new Size(300, 250),
                DisplayMember = "Name"
            };
            _itemsList.SelectedIndexChanged += ItemsList_SelectedIndexChanged;
            Controls.Add(_itemsList);

            // Drag-drop panel
            _dropPanel = new Panel
            {
                Location = new Point(330, 12),
                Size = new Size(140, 100),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };
            
            var dropLabel = new Label
            {
                Text = "Drag .exe files here",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkGray
            };
            _dropPanel.Controls.Add(dropLabel);
            Controls.Add(_dropPanel);

            // Buttons
            _removeButton = new Button { Text = "Remove", Location = new Point(330, 130), Size = new Size(65, 23) };
            _editButton = new Button { Text = "Edit", Location = new Point(405, 130), Size = new Size(65, 23) };
            _moveUpButton = new Button { Text = "Move Up", Location = new Point(330, 160), Size = new Size(65, 23) };
            _moveDownButton = new Button { Text = "Move Down", Location = new Point(405, 160), Size = new Size(65, 23) };
            
            _saveButton = new Button { Text = "Save", Location = new Point(315, 330), Size = new Size(75, 23) };
            _cancelButton = new Button { Text = "Cancel", Location = new Point(395, 330), Size = new Size(75, 23) };

            _removeButton.Click += RemoveButton_Click;
            _editButton.Click += EditButton_Click;
            _moveUpButton.Click += MoveUpButton_Click;
            _moveDownButton.Click += MoveDownButton_Click;
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { _removeButton, _editButton, _moveUpButton, _moveDownButton, _saveButton, _cancelButton });
            
            UpdateButtonStates();
        }

        private void SetupDragDrop()
        {
            _dropPanel.AllowDrop = true;
            _dropPanel.DragEnter += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                    e.Effect = files.Any(f => Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        ? DragDropEffects.Copy
                        : DragDropEffects.None;
                }
            };

            _dropPanel.DragDrop += (s, e) =>
            {
                if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                    foreach (var file in files.Where(f => Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        AddLaunchItem(file);
                    }
                }
            };
        }

        private void AddLaunchItem(string executablePath)
        {
            var item = new LaunchItem(executablePath)
            {
                Order = _launchItems.Count
            };
            
            _launchItems.Add(item);
            RefreshItemsList();
            _itemsList.SelectedIndex = _launchItems.Count - 1;
        }

        private void RefreshItemsList()
        {
            _itemsList.DataSource = null;
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
                _launchItems.RemoveAt(_itemsList.SelectedIndex);
                RefreshItemsList();
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
                RefreshItemsList();
                _itemsList.SelectedIndex = index + 1;
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            // Update order based on current list position
            for (int i = 0; i < _launchItems.Count; i++)
            {
                _launchItems[i].Order = i;
            }

            _configManager.SaveLaunchItems(_launchItems);
            _onSaved();
            Close();
        }
    }
}
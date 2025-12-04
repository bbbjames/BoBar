using System.Diagnostics;
using System.Drawing;

namespace BoBar
{
    public partial class Form1 : Form
    {
        private const int SnapDistance = 10;
        private const int SeparatorWidth = 1;
        private const int SeparatorTopMargin = 17;
        private static readonly Color SeparatorColor = Color.FromArgb(80, 80, 80);
        
        private readonly ConfigurationManager _configManager = new();
        private AppConfiguration _config = new();
        private List<LaunchItem> _launchItems = new();
        private bool _loadingPosition;

        public Form1()
        {
            InitializeComponent();
            LoadLaunchItems();
            CreateDynamicButtons();
            AddButtonSeparators();
            EnsureTrayIcon();
            LocationChanged += Form1_LocationChanged;
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void AddButtonSeparators()
        {
            if (flowLayoutPanel1 == null) return;

            var buttons = flowLayoutPanel1.Controls
                .OfType<Button>()
                .OrderBy(b => b.TabIndex)
                .ToList();

            if (buttons.Count == 0) return;

            flowLayoutPanel1.SuspendLayout();
            try
            {
                var existingSeparators = flowLayoutPanel1.Controls
                    .OfType<Panel>()
                    .Where(p => p.Tag?.ToString() == "Separator")
                    .ToList();

                foreach (var sep in existingSeparators)
                {
                    flowLayoutPanel1.Controls.Remove(sep);
                    sep.Dispose();
                }

                for (int i = 0; i < buttons.Count - 1; i++)
                {
                    int insertIndex = flowLayoutPanel1.Controls.IndexOf(buttons[i]) + 1;
                    var separator = CreateSeparator(buttons[i]);
                    flowLayoutPanel1.Controls.Add(separator);
                    flowLayoutPanel1.Controls.SetChildIndex(separator, insertIndex);
                }
            }
            finally
            {
                flowLayoutPanel1.ResumeLayout(true);
            }
        }

        private Panel CreateSeparator(Button referenceButton)
        {
            int buttonHeight = referenceButton.Height;
            int buttonTopMargin = referenceButton.Margin.Top;
            int buttonLeftMargin = referenceButton.Margin.Left;
            
            int separatorHeight = (int)(buttonHeight * 0.65);
            int topMargin = buttonTopMargin + (buttonHeight - separatorHeight) / 2;

            return new Panel
            {
                Width = SeparatorWidth,
                Height = separatorHeight,
                BackColor = SeparatorColor,
                Margin = new Padding(buttonLeftMargin, topMargin, 0, 0),
                Tag = "Separator",
                AccessibleName = "Separator",
                TabStop = false
            };
        }

        private void EnsureTrayIcon()
        {
            try
            {
                // Prefer the form icon if set via resources/designer
                var icon = this.Icon;

                // Fallback to assets icon on disk
                if (icon is null)
                {
                    var assetPath = Path.Combine(AppContext.BaseDirectory, "assets", "bobar-256.ico");
                    if (File.Exists(assetPath))
                    {
                        icon = new Icon(assetPath);
                    }
                }

                // Fallback to the executable's associated icon
                if (icon is null)
                {
                    icon = IconExtractor.ExtractHighQualityIcon(Application.ExecutablePath);
                }

                // Final fallback to a default application icon
                icon ??= SystemIcons.Application;

                // Apply to both form and tray icon
                this.Icon ??= icon;
                if (notifyIcon1 != null)
                {
                    notifyIcon1.Icon = icon;
                    notifyIcon1.Visible = true;
                }
            }
            catch
            {
                // As a last resort ensure tray icon is visible with a default icon
                if (notifyIcon1 != null)
                {
                    notifyIcon1.Icon = SystemIcons.Application;
                    notifyIcon1.Visible = true;
                }
            }
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            _loadingPosition = true;

            BackColor = Color.Fuchsia;
            TransparencyKey = Color.Fuchsia;

            SetupDragDrop();
            TryLoadWindowLocation();
            _loadingPosition = false;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            TrySaveWindowLocation();
        }

        private void TryLoadWindowLocation()
        {
            _config = _configManager.LoadConfiguration();

            if (_config.WindowLocation.HasValue)
            {
                StartPosition = FormStartPosition.Manual;
                Location = _config.WindowLocation.Value;
            }

            TopMost = _config.AlwaysOnTop;
            alwaysOnTopToolStripMenuItem.Checked = _config.AlwaysOnTop;

            darkModeToolStripMenuItem.Checked = _config.DarkMode;
            ApplyTheme();
        }

        private void TrySaveWindowLocation()
        {
            _config.AlwaysOnTop = TopMost;
            _configManager.SaveConfiguration(_config, Location);
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(_configManager, _launchItems, RefreshButtons);
            settingsForm.ShowDialog(this);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "BoBar Launcher v1.0", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                TopMost = item.Checked;
                _config.AlwaysOnTop = item.Checked;
            }
        }

        private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                _config.DarkMode = item.Checked;
                ApplyTheme();
            }
        }

        private void ApplyTheme()
        {
            var buttons = flowLayoutPanel1.Controls.OfType<Button>();

            foreach (var button in buttons)
            {
                if (_config.DarkMode)
                {
                    button.BackColor = Color.FromArgb(45, 45, 45);
                    button.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 30, 30);
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
                }
                else
                {
                    button.BackColor = SystemColors.MenuBar;
                    button.FlatAppearance.BorderColor = SystemColors.ScrollBar;
                    button.FlatAppearance.MouseDownBackColor = SystemColors.ScrollBar;
                    button.FlatAppearance.MouseOverBackColor = SystemColors.MenuBar;
                }
            }
        }

        private void FormDrag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0); // WM_NCLBUTTONDOWN + HTCAPTION
            }
        }

        private void notifyIcon1_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            this.Show();
            this.Activate();
        }

        // Snap to screen edges when moving
        private void Form1_LocationChanged(object? sender, EventArgs e)
        {
            if (_loadingPosition) return;

            var screen = Screen.FromPoint(Location);
            var wa = screen.WorkingArea; // exclude taskbar

            var newX = Left;
            var newY = Top;

            // snap left
            if (Math.Abs(Left - wa.Left) <= SnapDistance) newX = wa.Left;
            // snap right
            if (Math.Abs((Left + Width) - wa.Right) <= SnapDistance) newX = wa.Right - Width;
            // snap top
            if (Math.Abs(Top - wa.Top) <= SnapDistance) newY = wa.Top;
            // snap bottom
            if (Math.Abs((Top + Height) - wa.Bottom) <= SnapDistance) newY = wa.Bottom - Height;

            // only set if changed to avoid feedback loops
            if (newX != Left || newY != Top)
            {
                Location = new Point(newX, newY);
            }
        }

        private void LoadLaunchItems()
        {
            _launchItems = _configManager.LoadLaunchItems();
        }

        private void CreateDynamicButtons()
        {
            if (flowLayoutPanel1 == null) return;

            // Clear existing dynamic buttons (keep only static ones for now)
            var dynamicButtons = flowLayoutPanel1.Controls.OfType<Button>()
                .Where(b => b.Tag?.ToString() == "Dynamic")
                .ToList();

            foreach (var button in dynamicButtons)
            {
                flowLayoutPanel1.Controls.Remove(button);
                button.Dispose();
            }

            // Create buttons for each launch item
            for (int i = 0; i < _launchItems.Count; i++)
            {
                var item = _launchItems[i];
                var button = new Button
                {
                    Size = new Size(40, 40),
                    TabIndex = i,
                    UseVisualStyleBackColor = false,
                    FlatStyle = FlatStyle.Flat,
                    Tag = "Dynamic",
                    BackColor = _config.DarkMode ? Color.FromArgb(45, 45, 45) : SystemColors.MenuBar,
                    Margin = new Padding(3, 17, 0, 0)
                };

                button.FlatAppearance.BorderSize = 1;
                if (_config.DarkMode)
                {
                    button.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
                    button.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 30, 30);
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
                }
                else
                {
                    button.FlatAppearance.BorderColor = SystemColors.ScrollBar;
                    button.FlatAppearance.MouseDownBackColor = SystemColors.ScrollBar;
                    button.FlatAppearance.MouseOverBackColor = SystemColors.MenuBar;
                }

                // Set button icon
                var iconBitmap = item.GetIconBitmap();
                if (iconBitmap != null)
                {
                    button.BackgroundImage = iconBitmap;
                    button.BackgroundImageLayout = ImageLayout.Zoom;
                }

                // Set click handler
                button.Click += (s, e) => item.Launch();

                flowLayoutPanel1.Controls.Add(button);
            }
        }

        private void RefreshButtons()
        {
            LoadLaunchItems();
            CreateDynamicButtons();
            AddButtonSeparators();
            ApplyTheme();
        }

        private void SetupDragDrop()
        {
            AllowDrop = true;
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
        }

        private void Form1_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                e.Effect = files.Any(f => Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                    ? DragDropEffects.Copy
                    : DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                foreach (var file in files.Where(f => Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase)))
                {
                    AddLaunchItemFromDrop(file);
                }
            }
        }

        private void AddLaunchItemFromDrop(string executablePath)
        {
            try
            {
                var newItem = new LaunchItem(executablePath)
                {
                    Order = _launchItems.Count
                };

                _launchItems.Add(newItem);
                _configManager.SaveLaunchItems(_launchItems);
                RefreshButtons();

                AnimateNewButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to add {Path.GetFileName(executablePath)}: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AnimateNewButton()
        {
            var dynamicButtons = flowLayoutPanel1.Controls.OfType<Button>()
                .Where(b => b.Tag?.ToString() == "Dynamic")
                .ToList();

            if (dynamicButtons.Count == 0) return;

            var newButton = dynamicButtons[^1];
            var originalColor = newButton.BackColor;
            var glowColor = _config.DarkMode 
                ? Color.FromArgb(100, 150, 255) 
                : Color.FromArgb(135, 206, 250);

            var timer = new System.Windows.Forms.Timer { Interval = 50 };
            int step = 0;
            const int totalSteps = 20;

            timer.Tick += (s, e) =>
            {
                step++;
                if (step <= totalSteps)
                {
                    double progress = step / (double)totalSteps;
                    double pulseIntensity = Math.Sin(progress * Math.PI * 2) * 0.5 + 0.5;

                    newButton.BackColor = BlendColors(originalColor, glowColor, pulseIntensity * 0.6);
                }
                else
                {
                    newButton.BackColor = originalColor;
                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
        }

        private Color BlendColors(Color color1, Color color2, double ratio)
        {
            ratio = Math.Clamp(ratio, 0, 1);
            int r = (int)(color1.R + (color2.R - color1.R) * ratio);
            int g = (int)(color1.G + (color2.G - color1.G) * ratio);
            int b = (int)(color1.B + (color2.B - color1.B) * ratio);
            return Color.FromArgb(r, g, b);
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        }
    }
}
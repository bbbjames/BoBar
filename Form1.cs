using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private const int SnapDistance = 10; // pixels
        private readonly string _iniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BobsBar", "settings.ini");
        private bool _loadingPosition;

        public Form1()
        {
            InitializeComponent();
            LocationChanged += Form1_LocationChanged;
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            _loadingPosition = true;
            
            // Force exact dimensions to prevent any scaling issues
            // 4px top padding + 48px button height + 2px bottom padding = 54 height
            // 3×48 buttons + 2×2px spacing = 152 width (doubled to 292 for extra space)
            ClientSize = new Size(292, 54);
            
            TryLoadWindowLocation();
            _loadingPosition = false;
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            TrySaveWindowLocation();
        }

        private void TryLoadWindowLocation()
        {
            try
            {
                if (!File.Exists(_iniPath)) return;

                int? x = null;
                int? y = null;
                bool? alwaysOnTop = null;

                foreach (var line in File.ReadAllLines(_iniPath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("X=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(trimmed[2..], out var xValue))
                            x = xValue;
                    }
                    else if (trimmed.StartsWith("Y=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(trimmed[2..], out var yValue))
                            y = yValue;
                    }
                    else if (trimmed.StartsWith("AlwaysOnTop=", StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(trimmed[12..], out var topValue))
                            alwaysOnTop = topValue;
                    }
                }

                if (x.HasValue && y.HasValue)
                {
                    var pt = new Point(x.Value, y.Value);
                    if (Screen.AllScreens.Any(scr => scr.WorkingArea.Contains(pt)))
                    {
                        StartPosition = FormStartPosition.Manual;
                        Location = pt;
                    }
                }

                if (alwaysOnTop.HasValue)
                {
                    TopMost = alwaysOnTop.Value;
                    alwaysOnTopToolStripMenuItem.Checked = alwaysOnTop.Value;
                }
            }
            catch
            {
                // Swallow exceptions to avoid startup failure
            }
        }

        private void TrySaveWindowLocation()
        {
            try
            {
                var dir = Path.GetDirectoryName(_iniPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllLines(_iniPath, new[]
                {
                    "[Window]",
                    $"X={Left}",
                    $"Y={Top}",
                    $"AlwaysOnTop={TopMost}"
                });
            }
            catch
            {
                // Swallow exceptions to avoid close failure
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Clicked");
            //System.Diagnostics.Process.Start("notepad.exe"); 
            Process.Start(new ProcessStartInfo("notepad.exe") { UseShellExecute = true });
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //MessageBox.Show("Clicked");
            //System.Diagnostics.Process.Start("notepad.exe"); 
            Process.Start(new ProcessStartInfo("notepad.exe") { UseShellExecute = true });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Clicked");
            //System.Diagnostics.Process.Start("notepad.exe"); 
            Process.Start(new ProcessStartInfo("calc.exe") { UseShellExecute = true });
        }

        private void moveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Settings not implemented.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Example launcher v1", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Toggle TopMost based on menu check state
            if (sender is ToolStripMenuItem item)
            {
                TopMost = item.Checked;
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

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        }
    }
}
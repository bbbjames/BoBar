namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private const int SnapDistance = 10; // pixels

        public Form1()
        {
            InitializeComponent();
            LocationChanged += Form1_LocationChanged;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("calc.exe");
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
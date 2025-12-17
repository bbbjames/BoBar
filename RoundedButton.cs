using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedButton : Button
{
    public int BorderRadius { get; set; } = 20;

    protected override void OnPaint(PaintEventArgs pevent)
    {
        base.OnPaint(pevent);

        Graphics graphics = pevent.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Create a rounded rectangle path
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(0, 0, BorderRadius, BorderRadius, 180, 90);
            path.AddArc(Width - BorderRadius, 0, BorderRadius, BorderRadius, 270, 90);
            path.AddArc(Width - BorderRadius, Height - BorderRadius, BorderRadius, BorderRadius, 0, 90);
            path.AddArc(0, Height - BorderRadius, BorderRadius, BorderRadius, 90, 90);
            path.CloseFigure();

            // Set the button's region to the rounded rectangle
            this.Region = new Region(path);

            // Draw the border
            using (Pen pen = new Pen(this.FlatAppearance.BorderColor, this.FlatAppearance.BorderSize))
            {
                graphics.DrawPath(pen, path);
            }
        }
    }
}
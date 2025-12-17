using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundedButton : Button
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int BorderRadius { get; set; } = 20;

    private SymbolType _symbol = SymbolType.None;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public SymbolType Symbol
    {
        get => _symbol;
        set
        {
            if (_symbol != value)
            {
                _symbol = value;
                Invalidate();
            }
        }
    }

    public enum SymbolType
    {
        None,
        ChevronUp,
        ChevronDown,
        ChevronLeft,
        ChevronRight
    }

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

        if (Symbol != SymbolType.None)
        {
            DrawSymbol(graphics);
        }
    }

    private void DrawSymbol(Graphics g)
    {
        using var pen = new Pen(ForeColor, 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        int cx = Width / 2;
        int cy = Height / 2;
        int size = 6;

        switch (Symbol)
        {
            case SymbolType.ChevronUp:
                g.DrawLines(pen, new[] { new Point(cx - size, cy + 3), new Point(cx, cy - 3), new Point(cx + size, cy + 3) });
                break;
            case SymbolType.ChevronDown:
                g.DrawLines(pen, new[] { new Point(cx - size, cy - 3), new Point(cx, cy + 3), new Point(cx + size, cy - 3) });
                break;
            case SymbolType.ChevronLeft:
                g.DrawLines(pen, new[] { new Point(cx + 3, cy - size), new Point(cx - 3, cy), new Point(cx + 3, cy + size) });
                break;
            case SymbolType.ChevronRight:
                g.DrawLines(pen, new[] { new Point(cx - 3, cy - size), new Point(cx + 3, cy), new Point(cx - 3, cy + size) });
                break;
        }
    }
}
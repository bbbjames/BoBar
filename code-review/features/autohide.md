# Auto-Hide Feature Specification

## UI/UX Considerations
To ensure a smooth user experience, the auto-hide feature must address the following:
1.  **Edge Awareness**: The bar should collapse towards the nearest screen edge (Top or Bottom) so it doesn't float in the middle of the screen.
2.  **Accidental Hiding**: The bar must not hide while the user is interacting with it (mouse hover).
3.  **Visual Feedback**: A small portion ("lip") should remain visible to indicate the bar is present and can be expanded.
4.  **Smoothness**: Transitions should be instant or animated (instant is simpler for V1).

## Implementation Plan

### 1. Add Fields to `Form1`
We need to track state, original dimensions, and the timer.

```csharp
private System.Windows.Forms.Timer _autoHideTimer;
private bool _isAutoHidden;
private bool _autoHideEnabled = false; // Default to false
private Size _expandedSize;
private Point _expandedLocation;
private const int HiddenHeight = 6; // Height of the visible "lip"
```

### 2. Initialize in Constructor
Setup the timer and subscribe to events.

```csharp
public Form1()
{
    InitializeComponent();
    // ... existing code ...

    // Initialize auto-hide timer
    _autoHideTimer = new System.Windows.Forms.Timer { Interval = 2000 }; // 2 seconds
    _autoHideTimer.Tick += AutoHideTimer_Tick;
    
    // ... existing code ...
}
```

### 3. Implement Timer Logic (The "Hide" Action)
This logic needs to be smart about where the bar is positioned.

```csharp
private void AutoHideTimer_Tick(object? sender, EventArgs e)
{
    // 1. Safety Check: Don't hide if mouse is over the form
    if (this.Bounds.Contains(Cursor.Position)) return;
    
    // 2. Don't hide if already hidden or disabled
    if (_isAutoHidden || !_autoHideEnabled) return;

    // 3. Save state before hiding
    _expandedSize = this.Size;
    _expandedLocation = this.Location;

    // 4. Determine Screen Edge (Top vs Bottom)
    var screen = Screen.FromPoint(this.Location);
    var workingArea = screen.WorkingArea;
    
    // Check if we are closer to the bottom edge
    bool isBottom = Math.Abs((Top + Height) - workingArea.Bottom) < 50; // Tolerance

    // 5. Apply Hide
    this.AutoSize = false; // Important: Disable AutoSize to allow manual resizing
    
    if (isBottom)
    {
        // If at bottom, we need to move Top DOWN so the bottom of the form stays at the bottom of the screen
        // Actually, we want the TOP of the form to move DOWN, leaving only the top lip visible? 
        // No, if it's at the bottom, we want it to slide DOWN, leaving the TOP lip visible.
        
        // Wait, if it's at the bottom, and we shrink height, the Top stays fixed, so the bottom moves UP.
        // This makes it float.
        // We want the Bottom to stay fixed.
        
        int newTop = _expandedLocation.Y + (_expandedSize.Height - HiddenHeight);
        this.SetBounds(newTop: newTop, x: _expandedLocation.X, width: _expandedSize.Width, height: HiddenHeight);
    }
    else
    {
        // If at top (or floating), just shrink height. Top stays fixed.
        this.Size = new Size(_expandedSize.Width, HiddenHeight);
    }

    _isAutoHidden = true;
}
```

### 4. Implement Restore Logic (The "Show" Action)
We use `OnMouseEnter` to trigger the restore.

```csharp
protected override void OnMouseEnter(EventArgs e)
{
    base.OnMouseEnter(e);
    RestoreWindow();
}

// Also handle mouse move in case Enter is missed (fast movement)
protected override void OnMouseMove(MouseEventArgs e)
{
    base.OnMouseMove(e);
    RestoreWindow();
}

private void RestoreWindow()
{
    if (!_isAutoHidden) return;

    // Restore
    this.Location = _expandedLocation;
    this.Size = _expandedSize;
    this.AutoSize = true; // Re-enable AutoSize
    
    _isAutoHidden = false;
    
    // Reset timer
    _autoHideTimer.Stop();
    _autoHideTimer.Start();
}
```

### 5. Add Context Menu Toggle
Add a menu item to `contextMenuStrip1` to toggle the feature.

```csharp
// In Form1.Designer.cs or InitializeComponent
private ToolStripMenuItem autoHideToolStripMenuItem;

// In Form1 constructor or InitializeComponent
autoHideToolStripMenuItem = new ToolStripMenuItem();
autoHideToolStripMenuItem.Text = "Auto-hide";
autoHideToolStripMenuItem.CheckOnClick = true;
autoHideToolStripMenuItem.Click += (s, e) => 
{
    _autoHideEnabled = autoHideToolStripMenuItem.Checked;
    if (_autoHideEnabled)
        _autoHideTimer.Start();
    else
    {
        _autoHideTimer.Stop();
        if (_isAutoHidden) RestoreWindow();
    }
};
contextMenuStrip1.Items.Add(autoHideToolStripMenuItem);
```

### 6. Cleanup
Stop timer on close.

```csharp
private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
{
    _autoHideTimer.Stop();
    // ... existing code ...
}

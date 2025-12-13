# BoBar

A lightweight, customizable Windows application launcher with a floating taskbar interface.

## Features

### Core Functionality
- **Floating Launcher Bar** - Compact, always-accessible application launcher
- **Drag & Drop Support** - Add executables (.exe) and shortcuts (.lnk) by dragging onto the launcher or settings dialog
- **Shortcut Resolution** - Automatically resolves .lnk shortcuts to extract target path, arguments, and working directory
- **High-Quality Icon Extraction** - Extracts native high-resolution icons directly from PE resources without Windows upscaling
- **Icon Caching** - Saves extracted icons as PNG files for fast loading
- **Persistent Configuration** - Saves window position, launch items, and preferences
- **System Tray Integration** - Minimizes to system tray, double-click to restore
- **Load on Startup** - Optional Windows startup integration via registry

### Visual Features
- **Visual Button Separators** - Subtle dividers between launcher buttons for better organization
- **Animated Button Addition** - Smooth pulse animation when adding new items
- **Dark/Light Themes** - Toggle between dark and light color schemes
- **Snap to Edge** - Automatically snaps to screen edges when moving
- **Always on Top** - Optional window pinning

### Customization
- **Custom Order** - Reorder launch items with Move Up/Down buttons
- **Editable Items** - Modify name, path, arguments, and working directory
- **Multi-Monitor Support** - Works seamlessly across multiple displays

### Settings Management
- **Visual Settings Dialog** - Easy-to-use interface for managing launch items
- **Change Tracking** - Prompts to save unsaved changes before closing
- **Drag & Drop Zone** - Dedicated area for adding new items
- **Confirmation Dialogs** - Prevents accidental deletions
- **Live Preview** - Changes appear immediately after saving

## Installation

### Requirements
- Windows OS
- .NET 10.0 or later

### Building from Source

```bash
# Clone the repository
git clone https://github.com/bbbjames/BoBar.git
cd BoBar

# Build the project
dotnet build

# Run the application
dotnet run
```

### Release Build

**Self-Contained (includes .NET runtime):**
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

**Framework-Dependent (requires .NET 10 installed):**
```bash
dotnet publish -c Release -r win-x64 --no-self-contained
```

The compiled executable will be in `bin\Release\net10.0-windows\win-x64\publish\`

## Usage

### First Launch

On first launch, BoBar displays with two default items:
- **Notepad** - Opens Windows Notepad
- **Calculator** - Opens Windows Calculator

### Adding Applications

**Method 1: Drag onto Main Window**
1. Drag an .exe or .lnk file onto the BoBar window
2. The item is added and saved immediately
3. A smooth animation confirms the addition

**Method 2: Via Settings Dialog**
1. Right-click on the BoBar window
2. Select **Settings**
3. Drag .exe or .lnk files onto the drop zone
4. Click **Save & Close** to apply changes

### Editing Launch Items

1. Open **Settings** from the context menu
2. Select an item from the list
3. Click **Edit Item**
4. Modify the name, path, arguments, or working directory
5. Click **OK**, then **Save & Close**

### Reordering Items

1. Open **Settings**
2. Select an item
3. Use **Move Up ↑** or **Move Down ↓** buttons
4. Click **Save & Close**

### Removing Items

1. Open **Settings**
2. Select an item
3. Click **Remove Item**
4. Confirm the deletion
5. Click **Save & Close**

### Moving the Toolbar

**Method 1: Ctrl+Drag**
- Hold **Ctrl** and drag any button to move the toolbar
- The toolbar will snap to screen edges automatically

**Method 2: Direct Drag**
- Click and drag the toolbar background (between buttons)

### Context Menu Options

Right-click on the BoBar window to access:
- **Move** - Shows instructions for moving the toolbar
- **Settings** - Open the settings dialog
- **About** - View version and developer information
- **Always on Top** - Toggle window pinning (checkmark when enabled)
- **Dark Mode** - Toggle theme (checkmark when enabled)
- **Load on Startup** - Add/remove BoBar from Windows startup (checkmark when enabled)
- **Close** - Exit the application

## Configuration

BoBar stores configuration in `%AppData%\BoBar\`:

```
C:\Users\YourName\AppData\Roaming\BoBar\
├── launchitems.json    # Launch items list
├── settings.ini        # Window position and preferences
└── Icons\              # Extracted application icons (PNG format)
    └── *.png
```

### launchitems.json

JSON array containing launch item definitions:

```json
[
  {
    "Name": "Notepad",
    "ExecutablePath": "notepad.exe",
    "Arguments": "",
    "WorkingDirectory": "",
    "IconPath": "C:\\Users\\...\\BoBar\\Icons\\notepad.png",
    "Order": 0
  },
  {
    "Name": "Visual Studio Code",
    "ExecutablePath": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
    "Arguments": "",
    "WorkingDirectory": "",
    "IconPath": "C:\\Users\\...\\BoBar\\Icons\\Code.png",
    "Order": 1
  }
]
```

### settings.ini

INI format for window preferences:

```ini
[Window]
X=2225
Y=0
AlwaysOnTop=True
DarkMode=True
```

### Windows Startup

When "Load on Startup" is enabled, BoBar adds an entry to:
```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
```

This setting is per-user and does not require administrator privileges.

## Architecture

### Project Structure

```
BoBar/
├── Form1.cs                   # Main application window
├── Form1.Designer.cs          # Designer-generated code
├── SettingsForm.cs            # Settings dialog
├── AboutForm.cs               # About dialog
├── EditLaunchItemForm.cs      # Item editor dialog
├── LaunchItem.cs              # Data model for launch items
├── ConfigurationManager.cs    # Configuration persistence (includes AppConfiguration model)
├── IconExtractor.cs           # High-quality icon extraction from PE resources
├── ShortcutResolver.cs        # Windows .lnk shortcut resolution
├── StartupManager.cs          # Windows startup registry management
├── Program.cs                 # Application entry point
└── assets/
    └── bobar-256.ico          # Application icon
```

### Key Components

| Component | Responsibility |
|-----------|----------------|
| **Form1** | Main UI, button generation, drag-drop, system tray, snap-to-edge |
| **SettingsForm** | Launch items management, reordering, CRUD operations |
| **AboutForm** | Version info, developer credits, Twitter link |
| **EditLaunchItemForm** | Individual item property editing |
| **LaunchItem** | Data model, icon extraction, process launching |
| **ConfigurationManager** | JSON/INI persistence, default items, AppConfiguration data model |
| **IconExtractor** | Native high-resolution icon extraction from executables |
| **ShortcutResolver** | Windows shortcut (.lnk) file resolution via COM |
| **StartupManager** | Windows registry integration for startup management |

### Technical Details

**Icon Extraction System**
- Extracts icons directly from PE resources using Win32 APIs
- Attempts multiple extraction methods for best quality:
  1. Direct PE resource extraction (native sizes, no scaling)
  2. System image lists (SHIL_JUMBO for 256x256, SHIL_EXTRALARGE for 48x48)
  3. ExtractIconEx for embedded icons
  4. SHGetFileInfo as fallback
- Prioritizes largest size and highest color depth (32-bit ARGB)
- Saves as PNG with transparency preservation
- Normalized to 256x256 canvas for consistency

**Shortcut Resolution**
- Uses Windows Shell COM interfaces (IShellLink, IPersistFile)
- Extracts target path, command-line arguments, and working directory
- Validates target exists before adding
- Uses shortcut name as default item name

**Button Separators**
- Dynamically generated Panel controls between buttons
- Styled to match theme (dark/light mode)
- 65% of button height, vertically centered
- Automatically refreshed when buttons change

**Snap-to-Edge**
- Detects when window is within 10px of screen edge
- Automatically aligns to working area (excludes taskbar)
- Works with multiple monitors using `Screen.FromPoint()`

**Theme System**
- Dark mode: `RGB(45, 45, 45)` background
- Light mode: System default colors
- Applies programmatically to all buttons and separators
- Smooth hover and click effects

**Startup Integration**
- Uses `HKEY_CURRENT_USER\...\Run` registry key
- Per-user setting (no admin required)
- Stores absolute path to executable with quotes
- Checkbox reflects current registry state on load

**Change Tracking**
- Tracks adds, edits, removes, reorders in settings dialog
- Prompts on close if unsaved changes exist
- Three-way dialog: Save, Discard, Cancel

## Development

### Technology Stack
- **Framework**: .NET 10.0
- **UI**: Windows Forms
- **Language**: C# 14.0 with nullable reference types
- **Serialization**: System.Text.Json
- **Icon Storage**: PNG format (preserves transparency)
- **COM Interop**: Windows Shell for shortcut resolution

### Code Style
- Named event handlers (minimal inline lambdas)
- Programmatic UI generation with dynamic controls
- Null-safe with nullable annotations enabled
- Clean separation of concerns
- Extensive debug logging for icon extraction

### Building for Development

```bash
# Debug build
dotnet build

# Run with console output
dotnet run

# Clean build artifacts
dotnet clean
```

### Testing Workflow

1. **Basic Functionality**
   - Run the application
   - Test drag-drop for .exe files
   - Test drag-drop for .lnk shortcuts
   - Verify icon extraction quality

2. **Settings Dialog**
   - Open Settings dialog
   - Add/edit/remove/reorder items
   - Test save prompts (X button, Discard button)
   - Verify JSON persistence

3. **Visual Features**
   - Test theme toggle (dark/light)
   - Verify button separators appear
   - Test animation when adding items
   - Test Always on Top toggle

4. **System Integration**
   - Test system tray behavior
   - Test Load on Startup registry integration
   - Verify window position persistence
   - Test multi-monitor snap-to-edge

5. **Shortcut Support**
   - Drag .lnk files from Start Menu
   - Verify target resolution
   - Test items with arguments
   - Verify working directory handling

## Troubleshooting

### Launch items not persisting
- Check that `%AppData%\BoBar\` directory exists
- Verify write permissions to AppData folder
- Check `launchitems.json` is valid JSON

### Icons not displaying
- Ensure .exe files still exist at their original paths
- Check `%AppData%\BoBar\Icons\` for cached icons (PNG files)
- Try removing and re-adding the item
- Check debug output for icon extraction errors

### Window position not saving
- Check `settings.ini` exists in `%AppData%\BoBar\`
- Verify window was on a visible screen when closed
- Position validation ensures window appears on valid display

### Shortcuts not working
- Verify the shortcut target exists
- Check that the shortcut is a valid .lnk file
- Try creating the shortcut fresh from the Start Menu
- Check debug output for resolution errors

### Startup not working
- Verify registry permissions (shouldn't require admin)
- Check registry key: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- Look for "BoBar" entry with path to executable
- Try unchecking and re-checking the option

### Separators not showing
- Ensure there are at least 2 buttons visible
- Check that theme is applied correctly
- Verify flowLayoutPanel is not suspended

## Version History

📋 **[View Full Changelog](CHANGELOG.md)** - Detailed version history with all changes

### v0.1.0.2-alpha (2025-01-03)
- ✅ Added Windows startup integration via registry
- ✅ Added "Load on Startup" menu option
- ✅ Per-user startup configuration (no admin required)
- ✅ StartupManager utility for registry management

### Recent Improvements (2024-12)
- ✅ Added .lnk shortcut support with full resolution
- ✅ Implemented ShortcutResolver with COM interop
- ✅ Enhanced icon extraction with PE resource parsing
- ✅ Added visual button separators
- ✅ Animated button addition with pulse effect
- ✅ Improved icon caching (PNG format)
- ✅ Added About dialog with developer info
- ✅ Refactored event handlers to named methods
- ✅ Redesigned Settings dialog with better layout
- ✅ Added change tracking and save prompts
- ✅ Fixed ListBox display bug (DisplayMember)
- ✅ Improved button sizing and labels
- ✅ Added confirmation dialogs
- ✅ Enhanced user experience

### Initial Release (2024-11)
- ✅ Dynamic launch items system
- ✅ JSON persistence
- ✅ Icon extraction and caching
- ✅ Dark/light theme support
- ✅ Drag-drop support
- ✅ System tray integration
- ✅ Snap-to-edge functionality

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Follow existing code style
4. Test thoroughly (especially icon extraction and COM interop)
5. Submit a pull request with detailed description

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

Developed by **bbbjames**  
🌐 [Bob James AI Dev](https://bobjames.dev)  
🐦 [Bob's Twitter](https://bobjames.dev/twitter)

## Support

For issues, questions, or feature requests, please open an issue on GitHub:
📝 [GitHub Issues](https://github.com/bbbjames/BoBar/issues)

---

**BoBar** - Your lightweight Windows application launcher ⚡

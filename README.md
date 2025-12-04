# BoBar

A lightweight, customizable Windows application launcher with a floating taskbar interface.

## Features

### Core Functionality
- **Floating Launcher Bar** - Compact, always-accessible application launcher
- **Drag & Drop Support** - Add executables by dragging .exe files onto the launcher or settings dialog
- **Icon Extraction** - Automatically extracts and caches icons from executables
- **Persistent Configuration** - Saves window position, launch items, and preferences
- **System Tray Integration** - Minimizes to system tray, double-click to restore

### Customization
- **Dark/Light Themes** - Toggle between dark and light color schemes
- **Always on Top** - Optional window pinning
- **Snap to Edge** - Automatically snaps to screen edges when moving
- **Custom Order** - Reorder launch items with Move Up/Down buttons
- **Editable Items** - Modify name, path, and command-line arguments

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

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

The compiled executable will be in `bin\Release\net10.0-windows\win-x64\publish\`

## Usage

### First Launch

On first launch, BoBar displays with two default items:
- **Notepad** - Opens Windows Notepad
- **Calculator** - Opens Windows Calculator

### Adding Applications

**Method 1: Drag onto Main Window**
1. Drag an .exe file onto the BoBar window
2. The item is added and saved immediately
3. A confirmation message appears

**Method 2: Via Settings Dialog**
1. Right-click on the BoBar window
2. Select **Settings**
3. Drag .exe files onto the drop zone
4. Click **Save & Close** to apply changes

### Editing Launch Items

1. Open **Settings** from the context menu
2. Select an item from the list
3. Click **Edit Item**
4. Modify the name, path, or arguments
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

### Context Menu Options

Right-click on the BoBar window to access:
- **Move** - Drag the window to reposition
- **Settings** - Open the settings dialog
- **Always on Top** - Toggle window pinning
- **Dark Mode** - Toggle theme
- **About** - View version information
- **Close** - Exit the application

## Configuration

BoBar stores configuration in `%AppData%\BoBar\`:

```
C:\Users\YourName\AppData\Roaming\BoBar\
├── launchitems.json    # Launch items list
├── settings.ini        # Window position and preferences
└── Icons\              # Extracted application icons
    └── *.ico
```

### launchitems.json

JSON array containing launch item definitions:

```json
[
  {
    "Name": "Notepad",
    "ExecutablePath": "notepad.exe",
    "Arguments": "",
    "IconPath": "C:\\Users\\...\\BoBar\\Icons\\notepad.ico",
    "Order": 0
  },
  {
    "Name": "Visual Studio Code",
    "ExecutablePath": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
    "Arguments": "",
    "IconPath": "C:\\Users\\...\\BoBar\\Icons\\Code.ico",
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

## Architecture

### Project Structure

```
BoBar/
├── Form1.cs                   # Main application window
├── Form1.Designer.cs          # Designer-generated code
├── SettingsForm.cs            # Settings dialog
├── EditLaunchItemForm.cs      # Item editor dialog
├── LaunchItem.cs              # Data model for launch items
├── ConfigurationManager.cs    # Configuration persistence
├── Program.cs                 # Application entry point
└── assets/
    └── bobar-256.ico          # Application icon
```

### Key Components

| Component | Responsibility |
|-----------|----------------|
| **Form1** | Main UI, button generation, drag-drop, system tray |
| **SettingsForm** | Launch items management, reordering, CRUD operations |
| **EditLaunchItemForm** | Individual item property editing |
| **LaunchItem** | Data model, icon extraction, process launching |
| **ConfigurationManager** | JSON/INI persistence, default items |

### Features Detail

**Icon Extraction**
- Extracts icons from executables using `Icon.ExtractAssociatedIcon()`
- Saves to `%AppData%\BoBar\Icons\` for fast loading
- Falls back to live extraction if cached icon missing

**Snap-to-Edge**
- Detects when window is within 10px of screen edge
- Automatically aligns to working area (excludes taskbar)
- Works with multiple monitors

**Theme System**
- Dark mode: `RGB(45, 45, 45)` background
- Light mode: System default colors
- Applies programmatically to all buttons

**Change Tracking**
- Tracks adds, edits, removes, reorders
- Prompts on close if unsaved changes exist
- Three-way dialog: Save, Discard, Cancel

## Development

### Technology Stack
- **Framework**: .NET 10.0
- **UI**: Windows Forms
- **Language**: C# with nullable reference types
- **Serialization**: System.Text.Json

### Code Style
- Named event handlers (no inline lambdas except closures)
- Programmatic UI generation (no designer dependencies)
- Null-safe with nullable annotations
- Clean separation of concerns

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

1. Run the application
2. Test drag-drop functionality
3. Open Settings dialog
4. Add/edit/remove/reorder items
5. Test save prompts (X button, Discard button)
6. Verify JSON persistence
7. Test theme toggle
8. Test Always on Top
9. Test system tray behavior

## Troubleshooting

### Launch items not persisting
- Check that `%AppData%\BoBar\` directory exists
- Verify write permissions to AppData folder
- Check `launchitems.json` is valid JSON

### Icons not displaying
- Ensure .exe files still exist at their original paths
- Check `%AppData%\BoBar\Icons\` for cached icons
- Try removing and re-adding the item

### Window position not saving
- Check `settings.ini` exists in `%AppData%\BoBar\`
- Verify window was on a visible screen when closed
- Position validation ensures window appears on valid display

### Items showing "BoBar.LaunchItem"
- This was a bug fixed in recent commits
- Ensure you're running the latest version
- The ListBox `DisplayMember` must be set to "Name"

## Version History

### Recent Improvements (2024-11-30)
- ✅ Refactored event handlers to named methods
- ✅ Redesigned Settings dialog with better layout
- ✅ Added change tracking and save prompts
- ✅ Fixed ListBox display bug
- ✅ Improved button sizing and labels
- ✅ Added confirmation dialogs
- ✅ Enhanced user experience

### Initial Release
- ✅ Dynamic launch items system
- ✅ JSON persistence
- ✅ Icon extraction and caching
- ✅ Dark/light theme support
- ✅ Drag-drop support
- ✅ System tray integration

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Follow existing code style
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

Developed by bbbjames

## Support

For issues, questions, or feature requests, please open an issue on GitHub.

---

**BoBar** - Your lightweight Windows application launcher

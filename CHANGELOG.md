# Changelog

All notable changes to BoBar will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Release automation with GitHub Actions
- Automatic changelog generation via Release Drafter

### Changed
- Icon loading priority to prefer disk icon over embedded resource

### Fixed
- System tray icon not updating when .ico file is changed

## [0.1.0.2] - 2025-01-XX

### Added
- Initial release
- Floating launcher bar with customizable buttons
- Drag & drop support for .exe and .lnk files
- Icon extraction and caching
- System tray integration
- Dark/Light themes
- Always on Top option
- Snap to screen edges
- Settings dialog for managing launch items
- Persistent configuration (JSON + INI)

### Features
- **Dynamic Launch Items** - Add, edit, remove, and reorder shortcuts
- **Custom Order** - Move items up/down in settings
- **Editable Properties** - Modify name, path, and arguments
- **Auto Icon Extraction** - Automatically extracts icons from executables
- **Window Position Memory** - Remembers position across sessions

[Unreleased]: https://github.com/bbbjames/BoBar/compare/v0.1.0.2...HEAD
[0.1.0.2]: https://github.com/bbbjames/BoBar/releases/tag/v0.1.0.2

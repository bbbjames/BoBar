# Changelog Automation Documentation

## Overview

BoBar now has fully automated changelog management with three integrated systems:

1. **Release Drafter** - Automatically drafts release notes from PRs
2. **CHANGELOG.md** - Keep a Changelog format with version history
3. **Auto-Update Workflows** - Automatically updates CHANGELOG.md on release

## File Structure

```
BoBar/
??? CHANGELOG.md                              # Main changelog file
??? .github/
?   ??? release-drafter.yml                   # Release Drafter config
?   ??? workflows/
?       ??? release-drafter.yml               # Drafts releases on PR merge
?       ??? release.yml                        # Builds and publishes releases
?       ??? update-changelog.yml              # Auto-updates CHANGELOG on release
?       ??? generate-changelog.yml            # Manual changelog generation
```

## How It Works

### 1. During Development (Pull Requests)

When you create and merge PRs:

```bash
# Create a feature branch
git checkout -b feature/add-new-feature

# Make changes and commit
git add .
git commit -m "Add new feature"

# Push and create PR
git push origin feature/add-new-feature
```

**What Happens:**
- Release Drafter automatically labels your PR based on branch name/title
- When merged, it updates the draft release with categorized changes
- The draft release body is stored for later use

### 2. When You Release

When you're ready to release:

```bash
# Tag the release
git tag v0.2.0
git push origin v0.2.0
```

**What Happens:**
1. **Build Workflow** (`release.yml`) triggers:
   - Builds BoBar
   - Fetches the draft release notes
   - Creates the release with the drafted notes
   - Uploads the ZIP file

2. **Changelog Updater** (`update-changelog.yml`) triggers:
   - Automatically updates `CHANGELOG.md`
   - Adds the release notes under a new version section
   - Commits the changes back to master

### 3. Manual Changelog Entry (Optional)

If you need to manually add a changelog entry:

**Via GitHub UI:**
1. Go to **Actions** tab
2. Select **Generate Changelog Entry**
3. Click **Run workflow**
4. Enter the version (e.g., `0.2.0`)
5. Click **Run workflow**

**What Happens:**
- Generates changelog entry from git commits
- Inserts into CHANGELOG.md
- Commits to master

## CHANGELOG.md Format

Your CHANGELOG.md follows the [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
# Changelog

## [Unreleased]
### Added
- Features in development

### Changed
- Changes in development

### Fixed
- Fixes in development

## [0.2.0] - 2025-01-15
### Added
- New feature A
- New feature B

### Changed
- Improved X

### Fixed
- Fixed bug Y

## [0.1.0] - 2025-01-01
### Added
- Initial release

[Unreleased]: https://github.com/bbbjames/BoBar/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/bbbjames/BoBar/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/bbbjames/BoBar/releases/tag/v0.1.0
```

## Release Notes Template

When you create a release, users will see:

```markdown
## What's Changed

?? New Features
- Add new shortcut manager @bbbjames (#42)
- Support for .lnk files @contributor (#45)

?? Bug Fixes
- Fix tray icon not updating @bbbjames (#43)

?? UI/UX Improvements
- Improve settings dialog layout @bbbjames (#44)

? Performance
- Optimize icon loading @contributor (#47)

## Installation

1. Download `BoBar-0.2.0-win-x64.zip` from the assets below
2. Extract the ZIP file to your desired location
3. Run `BoBar.exe`

## Requirements

- Windows 10 or later
- .NET 10.0 Desktop Runtime

## Usage Tips

- **First time users**: Right-click the tray icon to access settings
- **Add shortcuts**: Drag & drop `.exe` or `.lnk` files onto BoBar
- **Move toolbar**: Hold `Ctrl` and drag any button
- **Customize**: Right-click ? Settings to edit, reorder, or remove items

## Documentation

- ?? [Full Changelog](https://github.com/bbbjames/BoBar/blob/master/CHANGELOG.md)
- ?? [README](https://github.com/bbbjames/BoBar/blob/master/README.md)
- ?? [Compare Changes](https://github.com/bbbjames/BoBar/compare/v0.1.0...v0.2.0)
```

## Best Practices

### For Each PR:

1. **Use descriptive branch names:**
   ```bash
   feature/add-shortcut-manager
   fix/tray-icon-update
   ui/improve-settings-dialog
   ```

2. **Write clear PR titles:**
   - ? "Add shortcut manager feature"
   - ? "Fix tray icon not updating"
   - ? "Updates"
   - ? "Changes"

3. **Add labels** (if auto-labeler doesn't catch it):
   - `feature` or `enhancement` for new features
   - `bug` for bug fixes
   - `ui`, `ux` for UI/UX improvements
   - `performance` for optimizations
   - `documentation` for docs
   - `refactor` for code improvements

### For Releases:

1. **Review the draft release** before tagging
2. **Edit if needed** (you can manually edit the draft)
3. **Tag with semantic version:**
   - `v0.2.0` - Minor version (new features)
   - `v0.1.1` - Patch version (bug fixes)
   - `v1.0.0` - Major version (breaking changes)

4. **Push the tag** to trigger the build

### Editing CHANGELOG.md:

You can manually edit CHANGELOG.md at any time:

1. **Add to [Unreleased]** for in-progress work
2. **Follow the categories:**
   - `Added` for new features
   - `Changed` for changes
   - `Deprecated` for soon-to-be removed features
   - `Removed` for removed features
   - `Fixed` for bug fixes
   - `Security` for security fixes

## Troubleshooting

### Release notes not showing up

**Problem:** Release created without draft notes

**Solution:**
- Make sure PRs are merged to `master`
- Check that Release Drafter ran after the merge
- Manually edit the release and copy from the draft

### CHANGELOG.md not updating

**Problem:** Auto-update workflow didn't run

**Solution:**
- Check Actions tab for workflow runs
- Verify `update-changelog.yml` workflow exists
- Manually run `generate-changelog.yml` workflow

### Duplicate version in CHANGELOG

**Problem:** Version appears twice in CHANGELOG.md

**Solution:**
- Manually edit CHANGELOG.md to remove duplicate
- Commit the fix

## URLs and Links

- **Releases**: https://github.com/bbbjames/BoBar/releases
- **Actions**: https://github.com/bbbjames/BoBar/actions
- **CHANGELOG**: https://github.com/bbbjames/BoBar/blob/master/CHANGELOG.md

## Summary

? **CHANGELOG.md created** - Keep a Changelog format  
? **Release Drafter configured** - Links to CHANGELOG  
? **Auto-update workflow** - Updates CHANGELOG on release  
? **Manual workflow** - Generate entries from commits  
? **README updated** - Links to CHANGELOG  

Your changelog is now fully automated! ??

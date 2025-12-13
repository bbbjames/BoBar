# Quick Release Guide

## ?? Creating a New Release

### 1. Prepare Your Changes
```bash
# Create feature branch
git checkout -b feature/my-new-feature

# Make changes, commit
git add .
git commit -m "Add my new feature"

# Push and create PR
git push origin feature/my-new-feature
```

### 2. Label Your PR
Add appropriate labels:
- `feature` / `enhancement` ? New features (bumps minor version)
- `bug` ? Bug fixes (bumps patch version)
- `ui` / `ux` ? UI improvements
- `performance` ? Performance improvements
- `documentation` ? Documentation changes
- `refactor` ? Code refactoring

### 3. Merge PR
- Merge PR to `master`
- Release Drafter automatically updates the draft release

### 4. Review Draft Release
- Go to: https://github.com/bbbjames/BoBar/releases
- Check the draft release notes
- Edit if needed (you can manually adjust)

### 5. Create and Push Tag
```bash
# Decide version number (semantic versioning):
# Major.Minor.Patch (e.g., 0.2.0)
# - Patch (0.1.X): Bug fixes
# - Minor (0.X.0): New features
# - Major (X.0.0): Breaking changes

# Create tag
git tag v0.2.0

# Push tag to trigger release
git push origin v0.2.0
```

### 6. Wait for Build
- GitHub Actions builds the app (~2-5 minutes)
- Creates release with draft notes
- Uploads ZIP file
- Auto-updates CHANGELOG.md

### 7. Done! ?
Your release is live at:
- Release: https://github.com/bbbjames/BoBar/releases/tag/v0.2.0
- Changelog: https://github.com/bbbjames/BoBar/blob/master/CHANGELOG.md

---

## ??? Label Reference

| Label | Category | Version Bump |
|-------|----------|--------------|
| `feature` | ?? New Features | Minor (0.X.0) |
| `enhancement` | ?? New Features | Minor (0.X.0) |
| `bug` | ?? Bug Fixes | Patch (0.0.X) |
| `ui` | ?? UI/UX Improvements | — |
| `ux` | ?? UI/UX Improvements | — |
| `performance` | ? Performance | — |
| `documentation` | ?? Documentation | — |
| `refactor` | ?? Refactoring | — |

---

## ?? Manual Changelog Entry

If you need to manually add a changelog entry without going through the full release process:

### Via GitHub UI:
1. Go to **Actions** ? **Generate Changelog Entry**
2. Click **Run workflow**
3. Enter version (e.g., `0.2.0`)
4. Click **Run workflow**

### Manually:
1. Edit `CHANGELOG.md`
2. Add your changes under `[Unreleased]`
3. Commit and push

---

## ?? Troubleshooting

### "Release notes are empty"
**Fix:** Make sure PRs were merged to `master` and labeled correctly

### "CHANGELOG not updated"
**Fix:** Check GitHub Actions tab for workflow errors

### "Version number wrong"
**Fix:** Tags must start with `v` (e.g., `v0.2.0` not `0.2.0`)

---

## ?? Resources

- [Full Automation Docs](docs/CHANGELOG_AUTOMATION.md)
- [CHANGELOG](CHANGELOG.md)
- [GitHub Releases](https://github.com/bbbjames/BoBar/releases)
- [GitHub Actions](https://github.com/bbbjames/BoBar/actions)

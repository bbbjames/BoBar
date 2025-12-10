using System.Drawing;

namespace BoBar
{
    public class LaunchItem
    {
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public int Order { get; set; }

        public LaunchItem() { }

        public LaunchItem(string executablePath)
        {
            ExecutablePath = executablePath;
            Name = Path.GetFileNameWithoutExtension(executablePath);

            // Set working directory to the executable's directory
            try
            {
                WorkingDirectory = Path.GetDirectoryName(executablePath) ?? string.Empty;
            }
            catch
            {
                WorkingDirectory = string.Empty;
            }

            // Try to extract high-quality icon from executable
            try
            {
                var icon = IconExtractor.ExtractHighQualityIcon(executablePath);
                if (icon != null)
                {
                    // Save icon to app data directory for persistence
                    var iconDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "BoBar", "Icons");
                    Directory.CreateDirectory(iconDir);

                    // Use PNG extension to preserve high quality (32-bit ARGB)
                    var iconFileName = $"{Path.GetFileNameWithoutExtension(executablePath)}.png";
                    IconPath = Path.Combine(iconDir, iconFileName);

                    IconExtractor.SaveIconToFile(icon, IconPath);
                }
            }
            catch
            {
                // If icon extraction fails, leave IconPath empty
                IconPath = string.Empty;
            }
        }

        public Icon? GetIcon()
        {
            try
            {
                var expandedIconPath = Environment.ExpandEnvironmentVariables(IconPath);
                if (!string.IsNullOrEmpty(expandedIconPath) && File.Exists(expandedIconPath))
                {
                    return IconExtractor.LoadIconFromFile(expandedIconPath);
                }

                if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
                {
                    return IconExtractor.ExtractHighQualityIcon(ExecutablePath);
                }
            }
            catch
            {
                // Return null if icon loading fails
            }

            return null;
        }

        public Bitmap? GetIconBitmap()
        {
            try
            {
                // If we have a cached icon file, load it directly
                var expandedIconPath = Environment.ExpandEnvironmentVariables(IconPath);
                if (!string.IsNullOrEmpty(expandedIconPath) && File.Exists(expandedIconPath))
                {
                    // For PNG files, load directly as bitmap for best quality
                    if (Path.GetExtension(expandedIconPath).Equals(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        return new Bitmap(expandedIconPath);
                    }
                    // For ICO files, load as icon then convert
                    else if (Path.GetExtension(expandedIconPath).Equals(".ico", StringComparison.OrdinalIgnoreCase))
                    {
                        using var icon = new Icon(expandedIconPath);
                        return icon.ToBitmap();
                    }
                }

                // Extract from executable if no cached icon
                if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
                {
                    var icon = IconExtractor.ExtractHighQualityIcon(ExecutablePath);
                    if (icon != null)
                    {
                        return icon.ToBitmap();
                    }
                }
            }
            catch
            {
                // Return null if icon loading fails
            }

            return null;
        }

        public void Launch()
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ExecutablePath,
                    Arguments = Arguments,
                    UseShellExecute = true
                };

                // Set working directory if specified
                if (!string.IsNullOrWhiteSpace(WorkingDirectory) && Directory.Exists(WorkingDirectory))
                {
                    startInfo.WorkingDirectory = WorkingDirectory;
                }
                
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch {Name}: {ex.Message}", "Launch Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
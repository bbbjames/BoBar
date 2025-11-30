using System.Drawing;

namespace BoBar
{
    public class LaunchItem
    {
        public string Name { get; set; } = string.Empty;
        public string ExecutablePath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public int Order { get; set; }

        public LaunchItem() { }

        public LaunchItem(string executablePath)
        {
            ExecutablePath = executablePath;
            Name = Path.GetFileNameWithoutExtension(executablePath);
            
            // Try to extract icon from executable
            try
            {
                var icon = Icon.ExtractAssociatedIcon(executablePath);
                if (icon != null)
                {
                    // Save icon to app data directory for persistence
                    var iconDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "BoBar", "Icons");
                    Directory.CreateDirectory(iconDir);
                    
                    var iconFileName = $"{Path.GetFileNameWithoutExtension(executablePath)}.ico";
                    IconPath = Path.Combine(iconDir, iconFileName);
                    
                    using var fileStream = File.Create(IconPath);
                    icon.Save(fileStream);
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
                if (!string.IsNullOrEmpty(IconPath) && File.Exists(IconPath))
                {
                    return new Icon(IconPath);
                }
                
                if (!string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
                {
                    return Icon.ExtractAssociatedIcon(ExecutablePath);
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
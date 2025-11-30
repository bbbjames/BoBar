 using System.Drawing;

namespace WinFormsApp1
{
    public class AppConfiguration
    {
        public Point? WindowLocation { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool DarkMode { get; set; } = true;
    }

    public class ConfigurationManager
    {
        private readonly string _configPath;

        public ConfigurationManager()
        {
            _configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BobsBar",
                "settings.ini"
            );
        }

        public AppConfiguration LoadConfiguration()
        {
            var config = new AppConfiguration();

            try
            {
                if (!File.Exists(_configPath))
                    return config;

                int? x = null;
                int? y = null;

                foreach (var line in File.ReadAllLines(_configPath))
                {
                    var trimmed = line.Trim();
                    
                    if (TryParseInt(trimmed, "X=", out var xValue))
                    {
                        x = xValue;
                    }
                    else if (TryParseInt(trimmed, "Y=", out var yValue))
                    {
                        y = yValue;
                    }
                    else if (TryParseBool(trimmed, "AlwaysOnTop=", out var alwaysOnTop))
                    {
                        config.AlwaysOnTop = alwaysOnTop;
                    }
                    else if (TryParseBool(trimmed, "DarkMode=", out var darkMode))
                    {
                        config.DarkMode = darkMode;
                    }
                }

                if (x.HasValue && y.HasValue)
                {
                    config.WindowLocation = new Point(x.Value, y.Value);
                }

                // Validate window location is on a visible screen
                if (config.WindowLocation.HasValue)
                {
                    var point = config.WindowLocation.Value;
                    if (!Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(point)))
                    {
                        config.WindowLocation = null;
                    }
                }
            }
            catch
            {
                // Return default configuration on any error
            }

            return config;
        }

        public void SaveConfiguration(AppConfiguration config, Point windowLocation)
        {
            try
            {
                var directory = Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lines = new[]
                {
                    "[Window]",
                    $"X={windowLocation.X}",
                    $"Y={windowLocation.Y}",
                    $"AlwaysOnTop={config.AlwaysOnTop}",
                    $"DarkMode={config.DarkMode}"
                };

                File.WriteAllLines(_configPath, lines);
            }
            catch
            {
                // Silently handle save failures to prevent application shutdown issues
            }
        }

        private static bool TryParseInt(string line, string prefix, out int value)
        {
            value = 0;
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(line[prefix.Length..], out value);
        }

        private static bool TryParseBool(string line, string prefix, out bool value)
        {
            value = false;
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            return bool.TryParse(line[prefix.Length..], out value);
        }
    }
}
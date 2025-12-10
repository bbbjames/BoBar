using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace BoBar;

/// <summary>
/// Resolves Windows shortcut (.lnk) files to extract target path, arguments, and working directory.
/// </summary>
public static class ShortcutResolver
{
    /// <summary>
    /// Resolves a .lnk shortcut file and extracts its properties.
    /// </summary>
    /// <param name="shortcutPath">Path to the .lnk file</param>
    /// <returns>ShortcutInfo containing target path, arguments, working directory, and shortcut name</returns>
    public static ShortcutInfo? ResolveShortcut(string shortcutPath)
    {
        if (!File.Exists(shortcutPath))
            return null;

        if (!shortcutPath.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var link = (IShellLink)new ShellLink();
            ((IPersistFile)link).Load(shortcutPath, 0);

            var targetPath = new StringBuilder(260);
            link.GetPath(targetPath, targetPath.Capacity, IntPtr.Zero, 0);

            var arguments = new StringBuilder(260);
            link.GetArguments(arguments, arguments.Capacity);

            var workingDirectory = new StringBuilder(260);
            link.GetWorkingDirectory(workingDirectory, workingDirectory.Capacity);

            var shortcutName = Path.GetFileNameWithoutExtension(shortcutPath);

            return new ShortcutInfo
            {
                TargetPath = targetPath.ToString(),
                Arguments = arguments.ToString(),
                WorkingDirectory = workingDirectory.ToString(),
                ShortcutName = shortcutName
            };
        }
        catch
        {
            return null;
        }
    }

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink { }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxArgs);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}

/// <summary>
/// Contains information extracted from a Windows shortcut file.
/// </summary>
public class ShortcutInfo
{
    public string TargetPath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string ShortcutName { get; set; } = string.Empty;
}

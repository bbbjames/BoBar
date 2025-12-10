using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BoBar;
public static class IconExtractor
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

    [DllImport("shell32.dll", EntryPoint = "#727")]
    private static extern int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

    [DllImport("shell32.dll", EntryPoint = "#727")]
    private static extern int SHGetImageListHandle(int iImageList, ref Guid riid, out IntPtr handle);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [ComImport]
    [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IImageList
    {
        [PreserveSig]
        int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
        [PreserveSig]
        int ReplaceIcon(int i, IntPtr hicon, ref int pi);
        [PreserveSig]
        int SetOverlayImage(int iImage, int iOverlay);
        [PreserveSig]
        int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
        [PreserveSig]
        int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
        [PreserveSig]
        int Draw(ref IMAGELISTDRAWPARAMS pimldp);
        [PreserveSig]
        int Remove(int i);
        [PreserveSig]
        int GetIcon(int i, int flags, ref IntPtr picon);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IMAGELISTDRAWPARAMS
    {
        public int cbSize;
        public IntPtr himl;
        public int i;
        public IntPtr hdcDst;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public int xBitmap;
        public int yBitmap;
        public int rgbBk;
        public int rgbFg;
        public int fStyle;
        public int dwRop;
        public int fState;
        public int Frame;
        public int crEffect;
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;
    private const uint SHGFI_SYSICONINDEX = 0x000004000;
    private const int SHIL_JUMBO = 0x4;
    private const int SHIL_EXTRALARGE = 0x2;
    private const int ILD_TRANSPARENT = 0x00000001;

    /// <summary>
    /// Extracts a high-quality icon from an executable file.
    /// This method attempts to extract the largest, best quality icon available (up to 256x256 with 32-bit color depth).
    /// </summary>
    public static Icon? ExtractHighQualityIcon(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            // Try to get jumbo icon (256x256) first - highest quality
            var jumboIcon = GetJumboIcon(filePath);
            if (jumboIcon != null)
                return jumboIcon;

            // Try extra large icon (48x48)
            var extraLargeIcon = GetExtraLargeIcon(filePath);
            if (extraLargeIcon != null)
                return extraLargeIcon;

            // Try to extract large icons using ExtractIconEx (typically 32x32)
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];

            int iconCount = ExtractIconEx(filePath, 0, largeIcons, smallIcons, 1);

            if (iconCount > 0 && largeIcons[0] != IntPtr.Zero)
            {
                try
                {
                    Icon icon = (Icon)Icon.FromHandle(largeIcons[0]).Clone();
                    return icon;
                }
                finally
                {
                    // Clean up icon handles
                    if (largeIcons[0] != IntPtr.Zero)
                        DestroyIcon(largeIcons[0]);
                    if (smallIcons[0] != IntPtr.Zero)
                        DestroyIcon(smallIcons[0]);
                }
            }

            // Fallback to SHGetFileInfo
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr result = SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);

            if (result != IntPtr.Zero && shinfo.hIcon != IntPtr.Zero)
            {
                try
                {
                    Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                    return icon;
                }
                finally
                {
                    DestroyIcon(shinfo.hIcon);
                }
            }
        }
        catch
        {
            // Fall through to final fallback
        }

        // Final fallback to the standard method
        try
        {
            return Icon.ExtractAssociatedIcon(filePath);
        }
        catch
        {
            return null;
        }
    }

    private static Icon? GetJumboIcon(string filePath)
    {
        try
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr result = SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_SYSICONINDEX);

            if (result == IntPtr.Zero || shinfo.iIcon < 0)
                return null;

            Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            IImageList? iml = null;

            int hr = SHGetImageList(SHIL_JUMBO, ref iidImageList, ref iml!);

            if (hr == 0 && iml != null)
            {
                IntPtr hIcon = IntPtr.Zero;
                hr = iml.GetIcon(shinfo.iIcon, ILD_TRANSPARENT, ref hIcon);

                if (hr == 0 && hIcon != IntPtr.Zero)
                {
                    try
                    {
                        Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
                        return icon;
                    }
                    finally
                    {
                        DestroyIcon(hIcon);
                    }
                }
            }
        }
        catch
        {
            // Return null if failed
        }

        return null;
    }

    private static Icon? GetExtraLargeIcon(string filePath)
    {
        try
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr result = SHGetFileInfo(filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_SYSICONINDEX);

            if (result == IntPtr.Zero || shinfo.iIcon < 0)
                return null;

            Guid iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            IImageList? iml = null;

            int hr = SHGetImageList(SHIL_EXTRALARGE, ref iidImageList, ref iml!);

            if (hr == 0 && iml != null)
            {
                IntPtr hIcon = IntPtr.Zero;
                hr = iml.GetIcon(shinfo.iIcon, ILD_TRANSPARENT, ref hIcon);

                if (hr == 0 && hIcon != IntPtr.Zero)
                {
                    try
                    {
                        Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();
                        return icon;
                    }
                    finally
                    {
                        DestroyIcon(hIcon);
                    }
                }
            }
        }
        catch
        {
            // Return null if failed
        }

        return null;
    }

    /// <summary>
    /// Saves an icon to a file with the best quality preservation.
    /// Saves as PNG to preserve 32-bit color depth and alpha channel.
    /// </summary>
    public static void SaveIconToFile(Icon icon, string outputPath)
    {
        // Convert to bitmap to preserve quality
        using var bitmap = icon.ToBitmap();

        // Create a new bitmap with 32-bit ARGB format
        using var highQualityBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(highQualityBitmap))
        {
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }

        // Save as PNG to preserve quality
        highQualityBitmap.Save(outputPath, ImageFormat.Png);
    }

    /// <summary>
    /// Loads an icon from a PNG or ICO file.
    /// </summary>
    public static Icon? LoadIconFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            // If it's an ICO file, load directly
            if (Path.GetExtension(filePath).Equals(".ico", StringComparison.OrdinalIgnoreCase))
            {
                return new Icon(filePath);
            }

            // If it's a PNG file, convert to icon
            if (Path.GetExtension(filePath).Equals(".png", StringComparison.OrdinalIgnoreCase))
            {
                using var bitmap = new Bitmap(filePath);
                IntPtr hIcon = bitmap.GetHicon();
                try
                {
                    // Clone the icon so we can safely destroy the handle
                    Icon tempIcon = Icon.FromHandle(hIcon);
                    Icon clonedIcon = (Icon)tempIcon.Clone();
                    return clonedIcon;
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    /// <summary>
    /// Converts an icon to a 24-bit or 32-bit bitmap for display.
    /// </summary>
    public static Bitmap IconToBitmap(Icon icon)
    {
        // Convert icon to bitmap preserving alpha channel (32-bit)
        using var bitmap = icon.ToBitmap();
        var result = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);

        using (var graphics = Graphics.FromImage(result))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }

        return result;
    }
}

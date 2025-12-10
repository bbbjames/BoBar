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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);

    [DllImport("shell32.dll", EntryPoint = "#727")]
    private static extern int SHGetImageList(int iImageList, ref Guid riid, out IImageList ppv);

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

    [StructLayout(LayoutKind.Sequential)]
    private struct ICONINFO
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    /// <summary>
    /// Extracts a high-quality icon from an executable file.
    /// This method attempts to extract the largest, best quality icon available.
    /// It directly extracts from the executable to get true native sizes without Windows upscaling.
    /// </summary>
    public static Icon? ExtractHighQualityIcon(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            // First, try to extract icon directly from PE resources (best quality, no Windows scaling)
            Icon? directIcon = ExtractIconFromPEResources(filePath);
            if (directIcon != null)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: Extracted icon directly from PE resources");
                return directIcon;
            }

            System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: PE extraction failed, trying alternative methods");

            // Fallback: try system icon cache which might have larger versions
            var jumboIcon = GetJumboIcon(filePath);
            if (jumboIcon != null)
            {
                // Check if this is a native large icon or upscaled
                using var testBitmap = jumboIcon.ToBitmap();
                System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: Jumbo icon size: {testBitmap.Width}x{testBitmap.Height}");
                
                // If we got a large native icon (not just upscaled), use it
                if (testBitmap.Width >= 128 && testBitmap.Height >= 128)
                {
                    System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: Using jumbo icon (large native size)");
                    return jumboIcon;
                }
                jumboIcon.Dispose();
            }

            // Try extra large icon (48x48)
            var extraLargeIcon = GetExtraLargeIcon(filePath);
            if (extraLargeIcon != null)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: Using extra large icon");
                return extraLargeIcon;
            }

            // Try ExtractIconEx which gets embedded icons
            Icon? bestIcon = ExtractLargestIconFromFile(filePath);
            if (bestIcon != null)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: Using icon from ExtractIconEx");
                return bestIcon;
            }

            // Try to extract large icons using ExtractIconEx directly (typically 32x32)
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExtractHighQualityIcon: Exception - {ex.Message}");
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

    /// <summary>
    /// Extracts all icon resources directly from an executable file and returns the largest one.
    /// This avoids Windows upscaling and gets true native icon sizes.
    /// </summary>
    private static Icon? ExtractLargestIconFromFile(string filePath)
    {
        Icon? bestIcon = null;
        int bestQuality = 0;

        try
        {
            // Method 1: Try ExtractIconEx to get all embedded icons
            int iconCount = ExtractIconEx(filePath, -1, null, null, 0);
            System.Diagnostics.Debug.WriteLine($"ExtractLargestIconFromFile: Found {iconCount} icon resources via ExtractIconEx");

            if (iconCount > 0)
            {
                IntPtr[] largeIcons = new IntPtr[iconCount];
                IntPtr[] smallIcons = new IntPtr[iconCount];

                int extracted = ExtractIconEx(filePath, 0, largeIcons, smallIcons, iconCount);
                System.Diagnostics.Debug.WriteLine($"ExtractLargestIconFromFile: Extracted {extracted} icons");

                if (extracted > 0)
                {
                    try
                    {
                        // Check each large icon - use Math.Min to avoid array bounds issues
                        int largeCount = Math.Min(extracted, largeIcons.Length);
                        for (int i = 0; i < largeCount; i++)
                        {
                            if (largeIcons[i] != IntPtr.Zero)
                            {
                                using var tempIcon = Icon.FromHandle(largeIcons[i]);
                                using var bitmap = tempIcon.ToBitmap();
                                
                                int size = bitmap.Width * bitmap.Height;
                                int bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
                                int quality = size * bpp;

                                System.Diagnostics.Debug.WriteLine($"  Large Icon {i}: {bitmap.Width}x{bitmap.Height}, {bitmap.PixelFormat} ({bpp}bpp), quality={quality}");

                                if (quality > bestQuality)
                                {
                                    bestQuality = quality;
                                    bestIcon?.Dispose();
                                    bestIcon = (Icon)tempIcon.Clone();
                                    System.Diagnostics.Debug.WriteLine($"  -> New best icon");
                                }
                            }
                        }

                        // Also check small icons - use Math.Min to avoid array bounds issues
                        int smallCount = Math.Min(extracted, smallIcons.Length);
                        for (int i = 0; i < smallCount; i++)
                        {
                            if (smallIcons[i] != IntPtr.Zero)
                            {
                                using var tempIcon = Icon.FromHandle(smallIcons[i]);
                                using var bitmap = tempIcon.ToBitmap();
                                
                                int size = bitmap.Width * bitmap.Height;
                                int bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
                                int quality = size * bpp;

                                System.Diagnostics.Debug.WriteLine($"  Small Icon {i}: {bitmap.Width}x{bitmap.Height}, {bitmap.PixelFormat} ({bpp}bpp), quality={quality}");

                                if (quality > bestQuality)
                                {
                                    bestQuality = quality;
                                    bestIcon?.Dispose();
                                    bestIcon = (Icon)tempIcon.Clone();
                                    System.Diagnostics.Debug.WriteLine($"  -> New best icon");
                                }
                            }
                        }
                    }
                    finally
                    {
                        // Clean up all handles safely
                        for (int i = 0; i < largeIcons.Length; i++)
                        {
                            if (largeIcons[i] != IntPtr.Zero)
                                DestroyIcon(largeIcons[i]);
                        }
                        for (int i = 0; i < smallIcons.Length; i++)
                        {
                            if (smallIcons[i] != IntPtr.Zero)
                                DestroyIcon(smallIcons[i]);
                        }
                    }
                }
            }

            // If we found an icon via ExtractIconEx, return it (don't try system methods)
            if (bestIcon != null)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractLargestIconFromFile: Selected icon with quality={bestQuality}");
                return bestIcon;
            }

            // Only try system methods if ExtractIconEx found nothing
            System.Diagnostics.Debug.WriteLine("  No icons via ExtractIconEx, trying system image lists...");
            
            // Method 2: Try system image lists (SHIL_EXTRALARGE = 48x48, typically native size)
            var extraLarge = GetExtraLargeIcon(filePath);
            if (extraLarge != null)
            {
                using var bitmap = extraLarge.ToBitmap();
                int size = bitmap.Width * bitmap.Height;
                int bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
                int quality = size * bpp;

                System.Diagnostics.Debug.WriteLine($"  ExtraLarge (48x48): {bitmap.Width}x{bitmap.Height}, {bitmap.PixelFormat} ({bpp}bpp), quality={quality}");
                System.Diagnostics.Debug.WriteLine($"ExtractLargestIconFromFile: Selected ExtraLarge icon with quality={quality}");
                return extraLarge;
            }

            System.Diagnostics.Debug.WriteLine("ExtractLargestIconFromFile: No suitable icons found");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExtractLargestIconFromFile: Failed - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"  Stack trace: {ex.StackTrace}");
            bestIcon?.Dispose();
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

            int hr = SHGetImageList(SHIL_JUMBO, ref iidImageList, out iml!);

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

            int hr = SHGetImageList(SHIL_EXTRALARGE, ref iidImageList, out iml!);

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
    /// Icons are centered on a 256x256 canvas for consistency.
    /// Extracts the largest, highest color depth image from multi-resolution icons.
    /// </summary>
    public static void SaveIconToFile(Icon icon, string outputPath)
    {
        const int targetSize = 256;
        
        // Extract the best quality bitmap from the icon
        using var bitmap = ExtractBestIconImage(icon);

        // Debug logging
        System.Diagnostics.Debug.WriteLine($"SaveIconToFile: Source icon size: {bitmap.Width}x{bitmap.Height}, format: {bitmap.PixelFormat}");

        // Create a new bitmap with 32-bit ARGB format at target size
        using var highQualityBitmap = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(highQualityBitmap))
        {
            // Use best quality rendering settings
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            
            // Clear the background to transparent
            graphics.Clear(Color.Transparent);
            
            // Calculate scaling to fit the icon within the target size while maintaining aspect ratio
            int sourceWidth = bitmap.Width;
            int sourceHeight = bitmap.Height;
            
            float scale = Math.Min((float)targetSize / sourceWidth, (float)targetSize / sourceHeight);
            
            // Calculate the dimensions for the scaled icon
            int scaledWidth = (int)(sourceWidth * scale);
            int scaledHeight = (int)(sourceHeight * scale);
            
            // Center the icon on the canvas
            int x = (targetSize - scaledWidth) / 2;
            int y = (targetSize - scaledHeight) / 2;
            
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"SaveIconToFile: Scale={scale:F2}, Scaled size: {scaledWidth}x{scaledHeight}, Position: ({x},{y})");
            
            // Draw the icon centered and scaled on the canvas
            // Use destination rectangle for precise sizing
            graphics.DrawImage(bitmap, 
                new Rectangle(x, y, scaledWidth, scaledHeight),
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                GraphicsUnit.Pixel);
        }

        // Save as PNG with best quality
        highQualityBitmap.Save(outputPath, ImageFormat.Png);
        System.Diagnostics.Debug.WriteLine($"SaveIconToFile: Saved to {outputPath}");
    }

    /// <summary>
    /// Extracts the best quality image from an icon that may contain multiple sizes and color depths.
    /// Prioritizes: 1) Largest size, 2) Highest color depth (32-bit > 24-bit > 8-bit > 4-bit).
    /// Preserves transparency (alpha channel).
    /// Extracts raw icon data directly without Windows scaling.
    /// </summary>
    private static Bitmap ExtractBestIconImage(Icon icon)
    {
        try
        {
            // Save icon to memory to analyze all embedded sizes
            using var ms = new MemoryStream();
            icon.Save(ms);
            
            // Try each embedded icon size and pick the largest/best quality
            Bitmap? bestBitmap = TryExtractNativeIconSizes(ms);
            if (bestBitmap != null)
            {
                return bestBitmap;
            }

            System.Diagnostics.Debug.WriteLine("ExtractBestIconImage: Native extraction failed, using ToBitmap fallback");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExtractBestIconImage: Exception: {ex.Message}");
        }

        // Final fallback: use standard ToBitmap and convert to ARGB
        using var fallback = icon.ToBitmap();
        var argbFallback = new Bitmap(fallback.Width, fallback.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(argbFallback))
        {
            g.Clear(Color.Transparent);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.DrawImage(fallback, 0, 0, fallback.Width, fallback.Height);
        }
        System.Diagnostics.Debug.WriteLine($"ExtractBestIconImage: Fallback: {argbFallback.Width}x{argbFallback.Height}, {argbFallback.PixelFormat}");
        return argbFallback;
    }

    /// <summary>
    /// Tries to extract native icon sizes from the ICO stream without Windows scaling
    /// </summary>
    private static Bitmap? TryExtractNativeIconSizes(MemoryStream iconStream)
    {
        try
        {
            iconStream.Position = 0;
            var reader = new BinaryReader(iconStream);
            
            reader.ReadUInt16(); // Reserved
            reader.ReadUInt16(); // Type
            ushort imageCount = reader.ReadUInt16();

            System.Diagnostics.Debug.WriteLine($"TryExtractNativeIconSizes: Found {imageCount} embedded images");

            Bitmap? bestBitmap = null;
            int bestQuality = 0;

            // Try to load each embedded icon at its native size
            for (int i = 0; i < imageCount; i++)
            {
                long entryPos = 6 + (i * 16); // ICO header is 6 bytes, each entry is 16 bytes
                iconStream.Position = entryPos;

                byte width = reader.ReadByte();
                byte height = reader.ReadByte();
                reader.ReadByte(); // Palette
                reader.ReadByte(); // Reserved
                ushort planes = reader.ReadUInt16();
                ushort bitCount = reader.ReadUInt16();
                uint imageSize = reader.ReadUInt32();
                uint imageOffset = reader.ReadUInt32();

                int actualWidth = width == 0 ? 256 : width;
                int actualHeight = height == 0 ? 256 : height;

                try
                {
                    // Try to load this specific icon size
                    iconStream.Position = 0;
                    using var tempIcon = new Icon(iconStream, actualWidth, actualHeight);
                    using var tempBitmap = tempIcon.ToBitmap();
                    
                    // Check if we got the size we asked for (not scaled by Windows)
                    bool isNativeSize = tempBitmap.Width == actualWidth && tempBitmap.Height == actualHeight;
                    int size = tempBitmap.Width * tempBitmap.Height;
                    int actualBpp = Image.GetPixelFormatSize(tempBitmap.PixelFormat);
                    int quality = size * actualBpp;

                    System.Diagnostics.Debug.WriteLine($"  Size {i}: requested {actualWidth}x{actualHeight}, got {tempBitmap.Width}x{tempBitmap.Height}, {tempBitmap.PixelFormat} ({actualBpp}bpp), quality={quality}, native={isNativeSize}");

                    // Prefer native sizes over scaled ones
                    if (isNativeSize && quality > bestQuality)
                    {
                        bestQuality = quality;
                        bestBitmap?.Dispose();
                        
                        // Clone the bitmap
                        bestBitmap = new Bitmap(tempBitmap.Width, tempBitmap.Height, PixelFormat.Format32bppArgb);
                        using (var g = Graphics.FromImage(bestBitmap))
                        {
                            g.Clear(Color.Transparent);
                            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            g.DrawImage(tempBitmap, 0, 0);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"  -> New best!");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  Size {i}: Failed to load - {ex.Message}");
                }
            }

            if (bestBitmap != null)
            {
                System.Diagnostics.Debug.WriteLine($"TryExtractNativeIconSizes: Selected {bestBitmap.Width}x{bestBitmap.Height}, {bestBitmap.PixelFormat}");
            }

            return bestBitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TryExtractNativeIconSizes: Failed - {ex.Message}");
            return null;
        }
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

    /// <summary>
    /// Extracts an icon directly from an executable file.
    /// This method attempts to extract the largest, best quality icon available.
    /// It directly extracts from the executable to get true native sizes without Windows upscaling.
    /// </summary>
    private static Icon? ExtractIconFromPEResources(string filePath)
    {
        try
        {
            IntPtr hModule = LoadLibraryEx(filePath, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
            if (hModule == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine("ExtractIconFromPEResources: LoadLibraryEx failed");
                return null;
            }

            try
            {
                // Try multiple icon group IDs - some apps use different IDs
                int[] iconGroupIds = { 1, 2, 3, 32512, 101, 102, 103, 104, 105 };
                
                foreach (int iconId in iconGroupIds)
                {
                    IntPtr hResInfo = FindResource(hModule, new IntPtr(iconId), new IntPtr(RT_GROUP_ICON));
                    if (hResInfo != IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine($"ExtractIconFromPEResources: Found icon group with ID {iconId}");
                        
                        IntPtr hResData = LoadResource(hModule, hResInfo);
                        if (hResData == IntPtr.Zero)
                            continue;

                        IntPtr pResData = LockResource(hResData);
                        if (pResData == IntPtr.Zero)
                            continue;

                        uint resSize = SizeofResource(hModule, hResInfo);
                        
                        // Copy the icon group data
                        byte[] iconGroupData = new byte[resSize];
                        Marshal.Copy(pResData, iconGroupData, 0, (int)resSize);

                        // Parse the icon group to build a complete ICO file
                        byte[]? icoData = BuildIcoFromResources(hModule, iconGroupData);
                        if (icoData != null)
                        {
                            using var ms = new MemoryStream(icoData);
                            var icon = new Icon(ms);
                            System.Diagnostics.Debug.WriteLine($"ExtractIconFromPEResources: Successfully extracted icon from PE resources (ID={iconId})");
                            return icon;
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("ExtractIconFromPEResources: No icon group resource found after trying all IDs");
            }
            finally
            {
                FreeLibrary(hModule);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ExtractIconFromPEResources: Failed - {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Builds a complete ICO file from PE icon resources
    /// </summary>
    private static byte[]? BuildIcoFromResources(IntPtr hModule, byte[] iconGroupData)
    {
        try
        {
            using var reader = new BinaryReader(new MemoryStream(iconGroupData));
            reader.ReadUInt16(); // Reserved
            reader.ReadUInt16(); // Type
            ushort count = reader.ReadUInt16(); // Image count

            System.Diagnostics.Debug.WriteLine($"BuildIcoFromResources: Found {count} icons in group");

            if (count == 0)
                return null;

            // Read all icon directory entries
            var entries = new List<(byte width, byte height, byte colors, byte reserved, ushort planes, ushort bitCount, uint size, ushort id)>();
            
            for (int i = 0; i < count; i++)
            {
                byte width = reader.ReadByte();
                byte height = reader.ReadByte();
                byte colors = reader.ReadByte();
                byte reserved = reader.ReadByte();
                ushort planes = reader.ReadUInt16();
                ushort bitCount = reader.ReadUInt16();
                uint bytesInRes = reader.ReadUInt32();
                ushort id = reader.ReadUInt16();

                entries.Add((width, height, colors, reserved, planes, bitCount, bytesInRes, id));
                
                int w = width == 0 ? 256 : width;
                int h = height == 0 ? 256 : height;
                System.Diagnostics.Debug.WriteLine($"  Icon {i}: {w}x{h}, {bitCount}bpp, {bytesInRes} bytes, ID={id}");
            }

            // Build ICO file
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Write ICO header
            writer.Write((ushort)0); // Reserved
            writer.Write((ushort)1); // Type (ICO)
            writer.Write((ushort)count); // Image count

            uint currentOffset = (uint)(6 + (count * 16)); // Header + directory entries

            // Write directory entries and collect image data
            var imageDataList = new List<byte[]>();
            foreach (var entry in entries)
            {
                // Write directory entry
                writer.Write(entry.width);
                writer.Write(entry.height);
                writer.Write(entry.colors);
                writer.Write(entry.reserved);
                writer.Write(entry.planes);
                writer.Write(entry.bitCount);
                writer.Write(entry.size);
                writer.Write(currentOffset);

                // Load the actual icon image data
                IntPtr hResInfo = FindResource(hModule, new IntPtr(entry.id), new IntPtr(RT_ICON));
                if (hResInfo != IntPtr.Zero)
                {
                    IntPtr hResData = LoadResource(hModule, hResInfo);
                    if (hResData != IntPtr.Zero)
                    {
                        IntPtr pResData = LockResource(hResData);
                        uint size = SizeofResource(hModule, hResInfo);
                        
                        byte[] imageData = new byte[size];
                        Marshal.Copy(pResData, imageData, 0, (int)size);
                        imageDataList.Add(imageData);

                        currentOffset += size;
                    }
                }
            }

            // Write all image data
            foreach (var imageData in imageDataList)
            {
                writer.Write(imageData);
            }

            return ms.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BuildIcoFromResources: Failed - {ex.Message}");
            return null;
        }
    }

    // PE Resource APIs
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LockResource(IntPtr hResData);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

    private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
    private const int RT_ICON = 3;
    private const int RT_GROUP_ICON = 14;
}

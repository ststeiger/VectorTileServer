
namespace SpriteGenerator
{
    
    public class PngFileInfo
    {
        public string? NameWithoutExtension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[]? ImageBytes { get; set; }


        public int ColorRgb;
    }

    public static class PngEnumerator
    {
        public static System.Collections.Generic.IEnumerable<PngFileInfo> EnumeratePngFiles(
            string directoryPath
        )
        {
            if (!System.IO.Directory.Exists(directoryPath))
                yield break;

            System.Collections.Generic.IEnumerable<string> files = System.IO.Directory.EnumerateFiles(directoryPath, "*.*", System.IO.SearchOption.TopDirectoryOnly);

            foreach (string filePath in files)
            {
                if (!filePath.EndsWith(".png", true, System.Globalization.CultureInfo.InvariantCulture))
                    continue;

                byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                {
#if SystemDrawing
                using (System.Drawing.Image img = System.Drawing.Image.FromStream(ms, useEmbeddedColorManagement: false, validateImageData: true))
                {
                    yield return new PngFileInfo
                    {
                        NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath),
                        Width = img.Width,
                        Height = img.Height,
                        ImageBytes = imageBytes
                    };
                }
#elif ImageSharp
                using (SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = 
                    SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imageBytes))
                {
                    yield return new PngFileInfo
                    {
                        NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath),
                        Width = image.Width,
                        Height = image.Height,
                        ImageBytes = imageBytes
                    };
                }
#endif

                    using (SkiaSharp.SKBitmap bitmap = SkiaSharp.SKBitmap.Decode(ms))
                    {
                        if (bitmap == null)
                            continue; // skip invalid or corrupted PNG

                        yield return new PngFileInfo
                        {
                            NameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath),
                            Width = bitmap.Width,
                            Height = bitmap.Height,
                            ImageBytes = imageBytes
                        };
                    } // End Using bitmap 

                } // End Using ms 

            } // Next filePath 

        } // End Generator EnumeratePngFiles 


    } // End Class PngEnumerator


} // End Namespace 


namespace SpriteGenerator
{


    internal class SvgRasterizer
    {


        public static void Test()
        {
            string basePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "data");
            basePath = System.IO.Path.GetFullPath(basePath);
            
            string inputFolderPath = System.IO.Path.Combine(basePath, "icons");

            string outputFolderPath = System.IO.Path.Combine(basePath, "rasterized");
            DeleteFolderIfExists(outputFolderPath);
            RasterizeAllSvgs(inputFolderPath, outputFolderPath, 1.0f);

            outputFolderPath = System.IO.Path.Combine(basePath, "rasterized2");
            DeleteFolderIfExists(outputFolderPath);
            RasterizeAllSvgs(inputFolderPath, outputFolderPath, 2.0f);
        } // End Sub Test 


        public static void DeleteFolderIfExists(string folderPath)
        {
            if (System.IO.Directory.Exists(folderPath))
                System.IO.Directory.Delete(folderPath, recursive: true);

            // Recreate the folder so it's ready for output
            // System.IO.Directory.CreateDirectory(folderPath);
        } // End Sub DeleteFolderIfExists 


        public static void RasterizeAllSvgs(string inputFolderPath, string outputFolderPath, float pixelRatioM)
        {
            // Ensure the output directory exists
            System.IO.Directory.CreateDirectory(outputFolderPath);

            // Get all SVG files from the input folder
            string[] svgFiles = System.IO.Directory.GetFiles(inputFolderPath, "*.svg");

            foreach (string inputSvgPath in svgFiles)
            {
                // Get the file name without the extension
                string fileName = System.IO.Path.GetFileNameWithoutExtension(inputSvgPath);

                // Construct the output PNG path
                string outputPngPath = System.IO.Path.Combine(outputFolderPath, $"{fileName}.png");

                // Call the RasterizeSvg method for each file
                RasterizeSvg(inputSvgPath, outputPngPath, pixelRatioM);
            } // Next inputSvgPath 

        } // End Function RasterizeAllSvgs 


        // inputSvgPath: Path to your SVG file
        // outputPngPath: Path to save the rasterized PNG
        // pixelRatio: Use 2 for @2x (retina), 1 for standard
        private static void RasterizeSvg(string inputSvgPath, string outputPngPath, float pixelRatio)
        {
            // string svgContent = System.IO.File.ReadAllText(inputSvgPath);
            // System.IO.MemoryStream strm = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));

            SkiaSharp.Extended.Svg.SKSvg svg = new SkiaSharp.Extended.Svg.SKSvg();
            svg.Load(inputSvgPath);
            // svg.Load(strm);

            SkiaSharp.SKSize originalSize = svg.CanvasSize;

            // Scale dimensions for pixel ratio
            int scaledWidth = (int)(originalSize.Width * pixelRatio);
            int scaledHeight = (int)(originalSize.Height * pixelRatio);

            using SkiaSharp.SKBitmap bitmap = new SkiaSharp.SKBitmap(scaledWidth, scaledHeight);
            using SkiaSharp.SKCanvas canvas = new SkiaSharp.SKCanvas(bitmap);
            canvas.Clear(SkiaSharp.SKColors.Transparent);
            canvas.Scale(pixelRatio); // Important! Use scale, not resize

            canvas.DrawPicture(svg.Picture);
            canvas.Flush();

            using SkiaSharp.SKImage image = SkiaSharp.SKImage.FromBitmap(bitmap);
            using SkiaSharp.SKData data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            using System.IO.FileStream stream = System.IO.File.OpenWrite(outputPngPath);
            data.SaveTo(stream);
        } // End Function RasterizeSvg 


    } // End Class SvgRasterizer 


} // End Namespace 

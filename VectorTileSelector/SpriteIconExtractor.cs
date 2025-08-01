
namespace VectorTileSelector
{
    
    using SixLabors.ImageSharp.Processing;
    

    class SpriteIconExtractor
    {
        /*
        public class SpriteInfo
        {
            public int width { get; set; }
            public int height { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public float pixelRatio { get; set; }
        }
        */

        public partial class SpriteInfo
        {
            [System.Text.Json.Serialization.JsonPropertyName("width")]
            public int Width { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("height")]
            public int Height { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("x")]
            public int X { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("y")]
            public int Y { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("pixelRatio")]
            public int PixelRatio { get; set; }
        }



        public static void Test()
        {
            string jsonPath = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\styles\bright\sprite.json";
            string pngPath = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\styles\bright\sprite.png";

            string outputDir = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\styles\bright\extracted_icons";

            if (!System.IO.Directory.Exists(outputDir))
                System.IO.Directory.CreateDirectory(outputDir);

            // Read and parse the JSON
            string json = System.IO.File.ReadAllText(jsonPath);
            System.Collections.Generic.Dictionary<string, SpriteInfo> sprites = 
                System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, SpriteInfo>>(json);

            // Load the sprite PNG
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> spriteImage = 
                SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(pngPath);

            foreach (System.Collections.Generic.KeyValuePair<string, SpriteInfo> kvp in sprites)
            {
                string name = kvp.Key;
                SpriteInfo info = kvp.Value;

                // Define the rectangle to crop
                SixLabors.ImageSharp.Rectangle cropRect = new SixLabors.ImageSharp.Rectangle(info.X, info.Y, info.Width, info.Height);

                // Clone and crop the image
                using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> icon = 
                    spriteImage.Clone(ctx => ctx.Crop(cropRect));

                // Sanitize filename (just in case)
                string safeName = string.Join("_", name.Split(System.IO.Path.GetInvalidFileNameChars()));
                string outputPath = System.IO.Path.Combine(outputDir, $"{safeName}.png");

                // Save as PNG
                // icon.Save(outputPath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
                SixLabors.ImageSharp.ImageExtensions.Save(icon, outputPath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());


                System.Console.WriteLine($"Saved: {outputPath}");
            }

            System.Console.WriteLine("Done.");
        }
    }

}


namespace VectorTileSelector
{

    // Install the SixLabors.ImageSharp NuGet package before running this code:
    // dotnet add package SixLabors.ImageSharp
    // This program requires .NET 6 or later.

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Drawing.Processing; // <-- This is the missing piece
    

    // Define a C# class to match the structure of each icon in the JSON.
    public partial class SpriteIcon
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

    class SpriteDecompositor
    {
     
        // WARNING: GEMINI crap - do not use - overwrite file 
        public static async System.Threading.Tasks.Task Test()
        {
            // --- 1. Define File Paths and Directory Names ---
            //
            // Replace "path/to/your/spritesheet.png" with the actual path to your sprite PNG file.
            // Replace "path/to/your/sprites.json" with the actual path to your sprite JSON file.
            // For this example, we'll simulate the JSON data.

            string spritePngPath = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\styles\bright\sprite.png"; 
            string outputDirectory = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\styles\bright\extracted_icons";


            string spriteJsonPath = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\styles\bright\sprite.json";


            // The user provided JSON data. We'll use this directly for the example.
            string jsonContent = await System.IO.File.ReadAllTextAsync(spriteJsonPath, System.Text.Encoding.UTF8);

            // --- 2. Create a Dummy Sprite Sheet for this Example ---
            //
            // This part is for demonstration only. In a real application, you would load your existing PNG.
            // We'll create a 128x128 image with colored squares to represent the icons.
            try
            {
                using (var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(128, 128))
                {
                    image.Mutate(ctx =>
                    {
                        // Draw the first icon's area (64x64, red)
                        ctx.Fill(SixLabors.ImageSharp.Color.Red, new SixLabors.ImageSharp.Rectangle(0, 0, 64, 64));
                        // Draw the second icon's area (32x22, blue)
                        ctx.Fill(SixLabors.ImageSharp.Color.Blue, new SixLabors.ImageSharp.Rectangle(64, 0, 32, 22));
                    });
                    await image.SaveAsync(spritePngPath);
                }
                System.Console.WriteLine($"Example sprite sheet '{spritePngPath}' created successfully.");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error creating example sprite sheet: {ex.Message}");
                return;
            }


            System.Console.WriteLine("\n--- Starting Sprite Extraction Process ---");

            // --- 3. Deserialize the JSON data ---
            //
            // We deserialize the JSON string into a dictionary where keys are icon names
            // and values are SpriteIcon objects.
            System.Collections.Generic.Dictionary<string, SpriteIcon>? sprites;
            try
            {
                sprites = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, SpriteIcon>>(jsonContent);
                if (sprites == null)
                {
                    System.Console.WriteLine("Error: Could not deserialize JSON data.");
                    return;
                }
                System.Console.WriteLine($"Successfully parsed JSON for {sprites.Count} icons.");
            }
            catch (System.Text.Json.JsonException ex)
            {
                System.Console.WriteLine($"Error parsing JSON: {ex.Message}");
                return;
            }

            // --- 4. Load the Sprite PNG file ---
            //
            // Load the main sprite sheet image from the specified path.
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> spriteSheet = 
                await SixLabors.ImageSharp.Image.LoadAsync<SixLabors.ImageSharp.PixelFormats.Rgba32>(spritePngPath);

            System.Console.WriteLine($"Successfully loaded sprite sheet from '{spritePngPath}'.");

            // --- 5. Create the Output Directory ---
            //
            // Ensure the directory to store the extracted icons exists.
            if (!System.IO.Directory.Exists(outputDirectory))
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
                System.Console.WriteLine($"Created output directory: '{outputDirectory}'");
            }

            // --- 6. Iterate and Extract Each Icon ---
            //
            // Loop through each entry in our dictionary of sprites.
            foreach (var sprite in sprites)
            {
                string iconName = sprite.Key;
                SpriteIcon iconData = sprite.Value;

                try
                {
                    // Create a Rectangle that defines the crop area based on the JSON data.
                    SixLabors.ImageSharp.Rectangle cropRectangle = new SixLabors.ImageSharp.Rectangle(iconData.X, iconData.Y, iconData.Width, iconData.Height);

                    // Clone (crop) the section of the sprite sheet that contains the icon.
                    using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> croppedImage = 
                        spriteSheet.Clone(ctx => ctx.Crop(cropRectangle));

                    // Define the path to save the new icon file.
                    string outputFilePath = System.IO.Path.Combine(outputDirectory, $"{iconName}.png");

                    // Save the cropped image as a new PNG file.
                    await croppedImage.SaveAsync(outputFilePath);

                    System.Console.WriteLine($"\tExtracted icon: '{iconName}' -> '{outputFilePath}'");
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"Error processing icon '{iconName}': {ex.Message}");
                }
            }

            System.Console.WriteLine("\n--- Extraction Complete! ---");
        }
    }

}

namespace SpriteGenerator
{

    using SpriteGenerator.Impl.WithSystemDrawing;
    using SpriteGenerator.Impl.WithSkiaSharp;
    using SpriteGenerator.Impl.WithImageSharp;


    /// <summary>
    /// Provides methods for compiling multiple source images into a single compact sprite sheet.
    /// </summary>
    public class SpriteBuilder
    {


        /// <summary>
        /// Gets or sets a value indicating the number of empty pixels to add horizontally
        /// between images in a sprite sheet.
        /// </summary>
        public int HorizontalPadding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the number of empty pixels to add vertically
        /// between images in a sprite sheet.
        /// </summary>
        public int VerticalPadding { get; set; }



        /// <summary>
        /// Creates a new SpriteBuilder instance using the provided value for both
        /// horizontal and vertical padding.
        /// </summary>
        /// <param name="padding">The amount of padding to add both horizontally and
        /// vertically between images in a sprite sheet.</param>
        public SpriteBuilder(int padding)
            : this(padding, padding)
        { } // End Constructor 


        /// <summary>
        /// Creates a new SpriteBuilder instance using the provided values for
        /// horizontal and vertical padding.
        /// </summary>
        /// <param name="horizontalPadding">The amount of padding to add horizontally
        /// between images in a sprite sheet.</param>
        /// <param name="verticalPadding">The amount of padding to add vertically
        /// between images in a sprite sheet.</param>
        public SpriteBuilder(int horizontalPadding, int verticalPadding)
        {
            HorizontalPadding = horizontalPadding;
            VerticalPadding = verticalPadding;
        } // End Constructor 


        /// <summary>
        /// Creates a sprite sheet image file and XML definition file using the list of <see cref="ImageInfo"/>
        /// instances provided.
        /// </summary>
        /// <param name="outputName">Name to use for the sprite sheet (this is not a file name).</param>
        /// <param name="sheetName">Name of the sprite sheet image file.</param>
        /// <param name="xmlName">Name of the sprite sheet XML definition file.</param>
        /// <param name="size">Size of the final output image used in the sprite sheet.</param>
        /// <param name="sprites">List of <see cref="ImageInfo"/> instances to use when building the sprite sheet.</param>
        private void WriteOutput(
             string outputName,
             string sheetName,
             string xmlName,
             ImageDimensions size,
             System.Collections.Generic.List<Sprite> sprites,
             IGraphicsBackend backend,
             int pixelRatio
        )
        {

            using IGraphicsCanvas canvas = backend.CreateCanvas(size.Width, size.Height);
            canvas.ClearTransparent();

            foreach (Sprite sprite in sprites)
            {
                canvas.DrawImage(sprite.Image, sprite.SheetPosition.X, sprite.SheetPosition.Y);
            } // Next sprite 

            System.IO.File.WriteAllBytes(sheetName, canvas.GetImageBytes());

            sprites.Sort((file1, file2) => string.Compare(file1.Name, file2.Name, true));

            WriteOutputXml(outputName, sheetName, xmlName, size, sprites);

            WriteOutputJson(
                outputName,
                sheetName,
                System.IO.Path.ChangeExtension(xmlName, ".json"),
                size,
                sprites, 
                pixelRatio 
            );

        } // End Function WriteOutput 


        private void WriteOutputXml(
            string outputName,
            string sheetName,
            string xmlName,
            ImageDimensions size,
            System.Collections.Generic.List<Sprite> sprites
        )
        {
            using (System.Xml.XmlWriter writer = System.Xml.XmlWriter.Create(
                    xmlName, new System.Xml.XmlWriterSettings { Indent = true }
                )
            )
            {
                writer.WriteStartDocument(true);
                writer.WriteStartElement("spritesheet");
                writer.WriteElementString("name", outputName);
                writer.WriteElementString("file", System.IO.Path.GetFileName(sheetName));
                writer.WriteElementString("count", sprites.Count.ToString());
                writer.WriteElementString("width", size.Width.ToString());
                writer.WriteElementString("height", size.Height.ToString());
                writer.WriteStartElement("sprites");

                foreach (Sprite sprite in sprites)
                {
                    writer.WriteStartElement("sprite");
                    writer.WriteElementString("name", sprite.Name);
                    writer.WriteElementString("x", sprite.SheetPosition.X.ToString());
                    writer.WriteElementString("y", sprite.SheetPosition.Y.ToString());
                    writer.WriteElementString("width", sprite.Width.ToString());
                    writer.WriteElementString("height", sprite.Height.ToString());
                    writer.WriteEndElement(); // sprite
                } // Next sprite 

                writer.WriteEndElement(); // sprites
                writer.WriteEndElement(); // spritesheet
                writer.WriteEndDocument();
            } // End Using writer 

        } // End Function WriteOutputXml 


#if USE_NEWTONSOFT
        
        /// <summary>
        /// Writes sprite data to a JSON file using System.Text.Json's Utf8JsonWriter
        /// to directly write to the stream, similar to a JsonTextWriter.
        /// </summary>
        /// <param name="outputName">The output name for the image sheet. (Not used in this implementation)</param>
        /// <param name="sheetName">The name of the sprite sheet. (Not used in this implementation)</param>
        /// <param name="jsonName">The path to the output JSON file.</param>
        /// <param name="size">The dimensions of the overall image. (Not used in this implementation)</param>
        /// <param name="sprites">The list of sprites to serialize.</param>
        /// <param name="pixelRatio">1 for default, 2 for retina.</param>
        private void WriteOutputJson(
            string outputName,
            string sheetName,
            string jsonName,
            ImageDimensions size,
            System.Collections.Generic.List<Sprite> sprites, 
            int pixelRatio 
        )
        {
            using (System.IO.FileStream stream = new System.IO.FileStream(jsonName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(stream, System.Text.Encoding.UTF8))
            using (Newtonsoft.Json.JsonTextWriter jsonWriter = new Newtonsoft.Json.JsonTextWriter(streamWriter) { Formatting = Newtonsoft.Json.Formatting.Indented })
            {
                jsonWriter.WriteStartObject();

                foreach (Sprite sprite in sprites)
                {
                    jsonWriter.WritePropertyName(sprite.Name);
                    jsonWriter.WriteStartObject();

                    jsonWriter.WritePropertyName("width");
                    jsonWriter.WriteValue(sprite.Width);

                    jsonWriter.WritePropertyName("height");
                    jsonWriter.WriteValue(sprite.Height);

                    jsonWriter.WritePropertyName("x");
                    jsonWriter.WriteValue(sprite.SheetPosition.X);

                    jsonWriter.WritePropertyName("y");
                    jsonWriter.WriteValue(sprite.SheetPosition.Y);

                    jsonWriter.WritePropertyName("pixelRatio");
                    jsonWriter.WriteValue(pixelRatio); 

                    jsonWriter.WriteEndObject();
                } // Next sprite 

                jsonWriter.WriteEndObject();
            } // End Using jsonWriter 

        } // End Sub WriteOutputJson 

#else


        /// <summary>
        /// Writes sprite data to a JSON file using System.Text.Json's Utf8JsonWriter
        /// to directly write to the stream, similar to a JsonTextWriter.
        /// </summary>
        /// <param name="outputName">The output name for the image sheet. (Not used in this implementation)</param>
        /// <param name="sheetName">The name of the sprite sheet. (Not used in this implementation)</param>
        /// <param name="jsonName">The path to the output JSON file.</param>
        /// <param name="size">The dimensions of the overall image. (Not used in this implementation)</param>
        /// <param name="sprites">The list of sprites to serialize.</param>
        /// <param name="pixelRatio">1 for default, 2 for retina.</param>
        public void WriteOutputJson(
            string outputName, 
            string sheetName, 
            string jsonName, 
            ImageDimensions size,
            System.Collections.Generic.List<Sprite> sprites, 
            int pixelRatio 
        )
        {
            // FileMode.Create will create a new file or overwrite an existing one.
            // FileAccess.Write ensures we can write to the file.
            using (System.IO.Stream stream = new System.IO.FileStream(
                jsonName, 
                System.IO.FileMode.Create, 
                System.IO.FileAccess.Write)
            )
            {
                // Create a Utf8JsonWriter instance. This writes to a UTF-8 encoded stream.
                System.Text.Json.JsonWriterOptions options = 
                    new System.Text.Json.JsonWriterOptions
                {
                    Indented = true // Set to true to format the JSON with indentation.
                };

                using (System.Text.Json.Utf8JsonWriter writer = 
                    new System.Text.Json.Utf8JsonWriter(stream, options)
                )
                {
                    writer.WriteStartObject();

                    foreach (Sprite sprite in sprites)
                    {
                        writer.WritePropertyName(sprite.Name);
                        writer.WriteStartObject();

                        writer.WriteNumber("width", sprite.Width);
                        writer.WriteNumber("height", sprite.Height);
                        writer.WriteNumber("x", sprite.SheetPosition.X);
                        writer.WriteNumber("y", sprite.SheetPosition.Y);
                        writer.WriteNumber("pixelRatio", pixelRatio); 

                        writer.WriteEndObject();
                    } // Next sprite

                    writer.WriteEndObject();
                } // End Using writer

            } // End Using stream

        } // End Sub WriteOutputJson 

#endif
        

        private static System.IO.FileInfo[] GetFiles(
            string folderPath,
            System.Func<System.IO.FileInfo, bool>? filter
        )
        {
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(folderPath);
            System.Collections.Generic.List<System.IO.FileInfo> result = new System.Collections.Generic.List<System.IO.FileInfo>();

            System.IO.FileInfo[] allFiles = dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories);
            for (int i = 0; i < allFiles.Length; i++)
            {
                System.IO.FileInfo file = allFiles[i];

                if (filter == null || filter(file))
                    result.Add(file);
            } // Next i 

            // result.Sort((file1, file2) => string.Compare(file1.Name, file2.Name, true));

            return result.ToArray();
        } // End Function GetFiles 


        private static System.IO.FileInfo[] GetFiles(string folderPath)
        {
            return GetFiles(folderPath, null);
        } // End Function GetFiles 


        /// <summary>
        /// Uses all images found in the specified source folder to build a compacted sprite sheet.
        /// Images used maintain their original orientation, but may be cropped to remove transparency
        /// if the <paramref name="cropTransparency"/> parameter is true.
        /// </summary>
        /// <param name="sourceFolder">Folder containing images to use in the sprite sheet.
        /// All images in the folder will be used.</param>
        /// <param name="outputName">Name to use for the sprite sheet output image file and XML definition file.</param>
        /// <param name="cropTransparency">True if transparent pixels should be removed when determining
        /// the source area of the image, or false if not.</param>
        public void BuildSheet(
            string sourceFolder, 
            string outputName, 
            bool cropTransparency, 
            bool roundUpPower2Size,
            int pixelRatio 
        )
        {
            if (!System.IO.Directory.Exists(sourceFolder))
                throw new System.ArgumentException(string.Format("Source folder '{0}' does not exist", sourceFolder), "sourceFolder");

            if (string.IsNullOrWhiteSpace(outputName))
                throw new System.ArgumentNullException("outputName");

            IGraphicsBackend backend = new SkiaBackend();
            // IGraphicsBackend backend = new SystemDrawingBackend();
            // IGraphicsBackend backend = new ImageSharpBackend();


            System.IO.FileInfo[] files = GetFiles(
                sourceFolder,
                file => file.Extension.Equals(".png", System.StringComparison.OrdinalIgnoreCase)
            );


            if ((files == null) || (files.Length == 0))
            {
                // Not finding image files means nothing can be built, but this should not
                // be treated as an exception
                return;
            }

            // Replace spaces in the name with underscores to help with compatibility
            // with other tools or across platforms.
            outputName = outputName.Replace(' ', '_');
            string sheetName = System.IO.Path.Combine(sourceFolder, outputName + ".png");
            string xmlName = System.IO.Path.Combine(sourceFolder, outputName + ".xml");

            if (System.IO.File.Exists(sheetName))
                System.IO.File.Delete(sheetName);

            if (System.IO.File.Exists(xmlName))
                System.IO.File.Delete(xmlName);

            // Calculate the total area of all images and find the widest.
            // Then create a list of images, sorted from smallest to largest.
            int totalArea = 0;
            int maxWidth = 0;
            System.Collections.Generic.List<Sprite> sprites = new System.Collections.Generic.List<Sprite>();
            foreach (System.IO.FileInfo file in files)
            {
                if (file.FullName == sheetName)
                    continue;

                backend.LoadImage(file.FullName);

                Sprite sprite = new Sprite(
                    System.IO.Path.GetFileNameWithoutExtension(file.FullName).Replace(' ', '_')
                    , backend
                    , HorizontalPadding
                    , VerticalPadding
                    , cropTransparency
                );

                sprites.Add(sprite);

                totalArea += sprite.Width * sprite.Height;
                maxWidth = (sprite.Width > maxWidth) ? sprite.Width : maxWidth;
            } // Next file 

            sprites.Sort(new SpriteSizeComparer());
            sprites.Reverse();

            // Target width of the output sprite sheet is the larger of either the square root
            // of the total area of all images, rounded up, or the width of the widest image.
            int targetWidth = System.Math.Max((int)System.Math.Ceiling(System.Math.Sqrt(totalArea)), maxWidth);
            if (roundUpPower2Size)
                targetWidth = MathHelper.LeastPower2GreaterThanX(targetWidth);

            int remainingWidth = targetWidth;
            int nextX = 0;
            int nextY = 0;
            int maxHeightInRow = 0;
            for (int i = 0; i < sprites.Count; ++i)
            {
                // If the next image being added is wider than the remaining allowed
                // width of the current row of images, start a new row
                if (remainingWidth - sprites[i].Width <= 0)
                {
                    remainingWidth = targetWidth;
                    nextX = 0;
                    nextY += maxHeightInRow;
                    maxHeightInRow = 0;
                }

                sprites[i].SheetPosition = new ImagePosition(nextX, nextY);
                remainingWidth -= sprites[i].Width;
                nextX += sprites[i].Width;

                if (sprites[i].Height > maxHeightInRow)
                    maxHeightInRow = sprites[i].Height;

            } // Next i 

            int targetHeight = nextY + maxHeightInRow;
            if (roundUpPower2Size)
                targetHeight = MathHelper.LeastPower2GreaterThanX(targetHeight);

            ImageDimensions size = new ImageDimensions(targetWidth, targetHeight);
            WriteOutput(outputName, sheetName, xmlName, size, sprites, backend, pixelRatio);
        } // End Sub BuildSheet 


    } // End Class SpriteBuilder 


} // End Namespace 

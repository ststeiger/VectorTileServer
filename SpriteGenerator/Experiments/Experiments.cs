
namespace SpriteGenerator.Experiments
{

    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Drawing.Processing;
    

    internal class Experiments
    {


        private static int PackColor(SixLabors.ImageSharp.Color color)
        {
            SixLabors.ImageSharp.PixelFormats.Rgba32 p = color.ToPixel<SixLabors.ImageSharp.PixelFormats.Rgba32>();

            int colorRgb = (p.R << 16) | (p.G << 8) | p.B;
            return colorRgb;
        } // End Function PackColor 


        private static (int r, int g, int b) UnpackRgb(int rgb)
        {
            return ((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        } // End Function UnpackRgb 


        private static void AssignColors(
            System.Collections.Generic.IEnumerable<BinPacking<PngFileInfo>.Box> sprites
        )
        {
            System.Random random = new System.Random(42); // deterministic
            foreach (BinPacking<PngFileInfo>.Box rect in sprites)
            {
                int r = random.Next(64, 256);
                int g = random.Next(64, 256);
                int b = random.Next(64, 256);
                rect.State!.ColorRgb = (r << 16) | (g << 8) | b;
            } // Next rect 

        } // End Function AssignColors 


        // Assign a unique-ish color to each rect (naive version)
        public static void AssignColors(
            System.Collections.Generic.List<Rect> sprites
        )
        {
            System.Random random = new System.Random(42); // deterministic
            foreach (Rect rect in sprites)
            {
                int r = random.Next(64, 256);
                int g = random.Next(64, 256);
                int b = random.Next(64, 256);
                rect.ColorRgb = (r << 16) | (g << 8) | b;
            } // Next rect 

        } // End Sub AssignColors 


        public static void TestRectpackSharp()
        {
            System.Collections.Generic.List<Rect> sprites =
                new System.Collections.Generic.List<Rect>
            {
                new Rect { Name = "A", Width = 64, Height = 64 },
                new Rect { Name = "B", Width = 128, Height = 32 },
                new Rect { Name = "C", Width = 32, Height = 128 },
                new Rect { Name = "D", Width = 64, Height = 64 },
            };

            sprites[0].ColorRgb = PackColor(SixLabors.ImageSharp.Color.Red);
            sprites[1].ColorRgb = PackColor(SixLabors.ImageSharp.Color.Green);
            sprites[2].ColorRgb = PackColor(SixLabors.ImageSharp.Color.Blue);
            sprites[3].ColorRgb = PackColor(SixLabors.ImageSharp.Color.Yellow);

            /*

            int amount = sprites.Count;

            RectpackSharp.PackingRectangle[] rectangles = new RectpackSharp.PackingRectangle[amount];

            for (int i = 0; i < sprites.Count; ++i)
            {
                rectangles[i] = sprites[i].Pack;
            }
            */

            RectpackSharp.PackingRectangle[] rectangles =
                new System.Collections.Generic.List<RectpackSharp.PackingRectangle>
            {
                new RectpackSharp.PackingRectangle { Id = 1, Width = 64, Height = 64 },
                new RectpackSharp.PackingRectangle { Id = 2, Width = 128, Height = 32 },
                new RectpackSharp.PackingRectangle { Id = 3, Width = 32, Height = 128 },
                new RectpackSharp.PackingRectangle { Id = 4, Width = 64, Height = 64 },
            }.ToArray();


            // Set the width and height of your rectangles
            // ...

            // RectpackSharp.RectanglePacker.Pack(rectangles, out RectpackSharp.PackingRectangle bounds, RectpackSharp.PackingHints.TryByArea);
            RectpackSharp.RectanglePacker.Pack(rectangles, out RectpackSharp.PackingRectangle bounds, RectpackSharp.PackingHints.FindBest, 1, 1, null, 500);


            for (int i = 0; i < sprites.Count; ++i)
            {
                sprites[i].X = (int)rectangles[i].X;
                sprites[i].Y = (int)rectangles[i].Y;
            }


            System.Console.WriteLine(sprites);





            /*
            AssignColors(sprites);
            
            int xDim = 0;
            int yDim = 0;

            int totalArea = 0;

            foreach (Rect img in sprites)
            {
                xDim++;
                xDim += img.Width;
                xDim++;

                totalArea += img.Width * img.Height;

                yDim++;
                yDim += img.Height;
                yDim++;
            }


            int side = (int)System.Math.Ceiling(System.Math.Sqrt(totalArea));
            // Add some slack for packing inefficiency (20–50%)
            side = (int)(side * 1.5);

            MaxRectsBinPack foo = new MaxRectsBinPack(side, side, false);

            foreach (Rect img in sprites)
            {
                System.Console.WriteLine($"{img.Name}: x={img.X}, y={img.Y}, w={img.Width}, h={img.Height}");
                Rect a = foo.Insert(img.Width, img.Height, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit);
                img.X = a.X;
                img.Y = a.Y;
            }

            // System.Console.WriteLine(foo.usedRectangles);
            // AssignColors(foo.usedRectangles);
            */

            // DrawSprites(foo.usedRectangles, side, side, @"D:\lol.png");
            // DrawSprites(sprites, side, side, @"D:\lol.png");

            DrawSprites(sprites, (int)bounds.Width, (int)bounds.Height, @"D:\lol.png");
        } // End Sub TestRectpackSharp 


        public static void DrawSprites(
            System.Collections.Generic.List<Rect> sprites,
            int width,
            int height,
            string outputPath
        )
        {
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image =
                new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height, SixLabors.ImageSharp.Color.Black);

            foreach (Rect rect in sprites)
            {
                // if (rect.X == null || rect.Y == null) continue;

                (int r, int g, int b) = UnpackRgb(rect.ColorRgb);
                SixLabors.ImageSharp.PixelFormats.Rgba32 color = new SixLabors.ImageSharp.PixelFormats.Rgba32((byte)r, (byte)g, (byte)b, 255);

                SixLabors.ImageSharp.Rectangle box = new SixLabors.ImageSharp.Rectangle(
                    (int)rect.X, (int)rect.Y, rect.Width, rect.Height
                );

                image.Mutate(ctx =>
                {
                    ctx.Fill(color, box);
                    ctx.Draw(SixLabors.ImageSharp.Color.White, 1f, box); // Optional: draw outline
                });
            } // Next rect 

            // image.Save(outputPath);
            SixLabors.ImageSharp.ImageExtensions.Save(image, outputPath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        } // End Sub DrawSprites 

#if false


        public static void TestSprite2PackingZ3()
        {
            System.Collections.Generic.List<Sprite2> sprites2 =
                new System.Collections.Generic.List<Sprite2>
            {
                new Sprite2 { Width = 40, Height = 60 },
                new Sprite2 { Width = 50, Height = 30 },
                new Sprite2 { Width = 20, Height = 70 },
                new Sprite2 { Width = 60, Height = 20 }
            };


            System.Collections.Generic.IEnumerable<PngFileInfo> pngFiles = PngEnumerator.EnumeratePngFiles(
                @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\SpriteGenerator\data\extracted_icons2"
            );

            System.Collections.Generic.List<Sprite2> sprites =
                new System.Collections.Generic.List<Sprite2>();


            foreach (PngFileInfo pngFile in pngFiles)
            {
                sprites.Add(new Sprite2 { Width = pngFile.Width, Height = pngFile.Height });
            } // Next pngFile 


            Sprite2PackingZ3.Solve(sprites, binWidth: 250);
        } // End Sub TestSprite2PackingZ3 

#endif




        // https://observablehq.com/@mourner/simple-rectangle-packing
        public static void TestMapBoxRectanglePacking()
        {
            System.Collections.Generic.List<BinPacking<PngFileInfo>.Box> boxes =
                new System.Collections.Generic.List<BinPacking<PngFileInfo>.Box>();



            System.Collections.Generic.IEnumerable<PngFileInfo> pngFiles = PngEnumerator.EnumeratePngFiles(
                @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\SpriteGenerator\data\extracted_icons2"
            );

            foreach (PngFileInfo pngFile in pngFiles)
            {
                boxes.Add(new BinPacking<PngFileInfo>.Box { State = pngFile, Width = pngFile.Width, Height = pngFile.Height });
            }

            AssignColors(boxes);

            BinPacking<PngFileInfo>.PackingState result = BinPacking<PngFileInfo>.PackBoxes(boxes);



            System.Console.WriteLine("Packed:");
            foreach (BinPacking<PngFileInfo>.Box b in result.Packed!)
                System.Console.WriteLine($"  Box at ({b.X},{b.Y}) size {b.Width}x{b.Height}");

            System.Console.WriteLine("Remaining Spaces:");
            foreach (Space s in result.Spaces!)
                System.Console.WriteLine($"  Space at ({s.X},{s.Y}) size {s.Width}x{s.Height}");


            // DrawSprites(result.Packed, (int)result.Bounds.Width, (int)result.Bounds.Height, @"D:\lol.png");
            // System.Console.WriteLine(result.Packed);

            DrawSpritesSkia(result.Packed, (int)result.Bounds!.Width, (int)result.Bounds.Height, @"D:\lol.png");
            System.Console.WriteLine(result.Packed);
        } // End Sub TestMapBoxRectanglePacking 


        public static void DrawSpritesSkia(
            System.Collections.Generic.IEnumerable<BinPacking<PngFileInfo>.Box> sprites,
            int width,
            int height,
            string outputPath
        )
        {
            using SkiaSharp.SKSurface surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(width, height));
            SkiaSharp.SKCanvas canvas = surface.Canvas;

            // Fill background black
            canvas.Clear(SkiaSharp.SKColors.Black);

            foreach (BinPacking<PngFileInfo>.Box rect in sprites)
            {
                if (rect.X == null || rect.Y == null)
                    continue;

                PngFileInfo sprite = rect.State!;

                using SkiaSharp.SKBitmap spriteBitmap = SkiaSharp.SKBitmap.Decode(sprite.ImageBytes);
                if (spriteBitmap == null)
                    continue;

                SkiaSharp.SKRect destRect = new SkiaSharp.SKRect(
                    rect.X.Value,
                    rect.Y.Value,
                    rect.X.Value + rect.Width,
                    rect.Y.Value + rect.Height
                );

                // If the sprite image size doesn't match the rect size, scale it
                SkiaSharp.SKRect sourceRect = new SkiaSharp.SKRect(0, 0, spriteBitmap.Width, spriteBitmap.Height);

                using SkiaSharp.SKPaint paint = new SkiaSharp.SKPaint { FilterQuality = SkiaSharp.SKFilterQuality.High };

                canvas.DrawBitmap(spriteBitmap, sourceRect, destRect, paint);

                // Optional: draw white border
                using SkiaSharp.SKPaint borderPaint = new SkiaSharp.SKPaint
                {
                    Color = SkiaSharp.SKColors.White,
                    Style = SkiaSharp.SKPaintStyle.Stroke,
                    StrokeWidth = 1
                };
                canvas.DrawRect(destRect, borderPaint);
            }

            // Save image as PNG
            using SkiaSharp.SKImage image = surface.Snapshot();
            using SkiaSharp.SKData data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, quality: 100);
            using System.IO.FileStream fileStream = System.IO.File.OpenWrite(outputPath);
            data.SaveTo(fileStream);
        } // End Sub DrawSpritesSkia 


        public static void DrawSprites(
            System.Collections.Generic.IEnumerable<BinPacking<PngFileInfo>.Box> sprites,
            int width,
            int height,
            string outputPath
        )
        {
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image =
                new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height, SixLabors.ImageSharp.Color.Black);

            foreach (BinPacking<PngFileInfo>.Box rect in sprites)
            {
                if (rect.X == null || rect.Y == null)
                    continue;

                using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> spriteImage = 
                    SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(
                        rect.State!.ImageBytes
                );

                image.Mutate(ctx =>
                {
                    ctx.DrawImage(spriteImage, new SixLabors.ImageSharp.Point(rect.X.Value, rect.Y.Value), 1f);
                    ctx.Draw(SixLabors.ImageSharp.Color.White, 1f, new SixLabors.ImageSharp.Rectangle(rect.X.Value, rect.Y.Value, rect.Width, rect.Height)); // optional border
                });
            }

            // image.Save(outputPath);
            SixLabors.ImageSharp.ImageExtensions.Save(image, outputPath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        } // End Sub DrawSprites 


        public static void DrawSpritesRectangles(
            System.Collections.Generic.IEnumerable<BinPacking<PngFileInfo>.Box> sprites,
            int width,
            int height,
            string outputPath
        )
        {
            using SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image =
                new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height, SixLabors.ImageSharp.Color.Black);

            foreach (BinPacking<PngFileInfo>.Box rect in sprites)
            {
                if (rect.X == null || rect.Y == null)
                    continue;

                (int r, int g, int b) = UnpackRgb(rect.State!.ColorRgb);
                SixLabors.ImageSharp.PixelFormats.Rgba32 color = new SixLabors.ImageSharp.PixelFormats.Rgba32((byte)r, (byte)g, (byte)b, 255);


                SixLabors.ImageSharp.Rectangle box = new SixLabors.ImageSharp.Rectangle(
                    rect.X.Value, rect.Y.Value, rect.Width, rect.Height
                );

                image.Mutate(ctx =>
                {
                    ctx.Fill(color, box);
                    ctx.Draw(SixLabors.ImageSharp.Color.White, 1f, box); // Optional: draw outline
                });

            } // Next rect 

            // image.Save(outputPath);
            SixLabors.ImageSharp.ImageExtensions.Save(image, outputPath, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        } // End Function DrawSpritesRectangles 


    } // End Class Experiments 


} // End Namespace 

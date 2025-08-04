
namespace SpriteGenerator.Impl.WithSkiaSharp
{


    public class SkiaImage 
        : IGraphicsImage
    {
        private readonly SkiaSharp.SKBitmap _bitmap;

        public SkiaImage(SkiaSharp.SKBitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;

        public bool IsTransparent(int x, int y)
        {
            SkiaSharp.SKColor color = _bitmap.GetPixel(x, y);
            return color.Alpha == 0;
        }

        public IGraphicsImage Clone(int x, int y, int width, int height)
        {
            var subset = new SkiaSharp.SKBitmap(width, height);
            _bitmap.ExtractSubset(subset, new SkiaSharp.SKRectI(x, y, x + width, y + height));
            return new SkiaImage(subset);
        }

        public void DrawOn(IGraphicsCanvas canvas, int x, int y)
        {
            if (canvas is not SkiaCanvas skiaCanvas)
                throw new System.InvalidCastException("Canvas is not a SkiaCanvas.");

            skiaCanvas.DrawImage(this, x, y);
        }

        public SkiaSharp.SKBitmap RawBitmap => _bitmap;

        public void Dispose()
        {
            _bitmap.Dispose();
        }
    }
}

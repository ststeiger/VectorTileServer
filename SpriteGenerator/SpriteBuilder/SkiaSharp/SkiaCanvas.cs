
namespace SpriteGenerator.Impl.WithSkiaSharp
{


    public class SkiaCanvas
        : IGraphicsCanvas
    {
        private readonly SkiaSharp.SKBitmap _bitmap;
        private readonly SkiaSharp.SKCanvas _canvas;

        public SkiaCanvas(int width, int height)
        {
            // _bitmap = new SkiaSharp.SKBitmap(width, height, true);
            // _canvas = new SkiaSharp.SKCanvas(_bitmap);

            SkiaSharp.SKImageInfo info = new SkiaSharp.SKImageInfo(
                width,
                height,
                SkiaSharp.SKColorType.Bgra8888,
                SkiaSharp.SKAlphaType.Premul // Premultiplied alpha
            );

            _bitmap = new SkiaSharp.SKBitmap(info);
            _canvas = new SkiaSharp.SKCanvas(_bitmap);
        }

        public void ClearTransparent()
        {
            _canvas.Clear(SkiaSharp.SKColors.Transparent);
        }

        public void DrawImage(IGraphicsImage image, int x, int y)
        {
            if (image is not SkiaImage skiaImage)
                throw new System.InvalidCastException("Image is not a SkiaImage.");

            _canvas.DrawBitmap(skiaImage.RawBitmap, x, y);
        }

        public byte[] GetImageBytes()
        {
            using SkiaSharp.SKImage image = SkiaSharp.SKImage.FromBitmap(_bitmap);
            using SkiaSharp.SKData data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        public void Dispose()
        {
            _canvas.Dispose();
            _bitmap.Dispose();
        }
    }
}

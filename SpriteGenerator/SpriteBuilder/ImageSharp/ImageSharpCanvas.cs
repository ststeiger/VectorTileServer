
namespace SpriteGenerator.Impl.WithImageSharp
{

    using SixLabors.ImageSharp.Processing;


    public class ImageSharpCanvas 
        : IGraphicsCanvas
    {
        private readonly SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> _canvas;

        public ImageSharpCanvas(int width, int height)
        {
            _canvas = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(width, height);
            ClearTransparent();
        }

        public void ClearTransparent()
        {
            _canvas.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.Transparent));
        }

        public void DrawImage(IGraphicsImage image, int x, int y)
        {
            if (image is not ImageSharpImage sourceImage)
                throw new System.InvalidCastException("Image is not an ImageSharpImage.");

            _canvas.Mutate(ctx => ctx.DrawImage(sourceImage.RawImage, new SixLabors.ImageSharp.Point(x, y), 1f));
        }

        public byte[] GetImageBytes()
        {
            using System.IO.MemoryStream ms = new System.IO.MemoryStream();
            _canvas.Save(ms, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
            return ms.ToArray();
        }

        public void Dispose()
        {
            _canvas.Dispose();
        }
    }
}

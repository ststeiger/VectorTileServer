
namespace SpriteGenerator.Impl.WithImageSharp
{


    using SixLabors.ImageSharp.Processing;


    public class ImageSharpImage
        : IGraphicsImage
    {
        private readonly SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> _image;

        public ImageSharpImage(SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image)
        {
            _image = image;
        }

        public int Width => _image.Width;
        public int Height => _image.Height;

        public bool IsTransparent(int x, int y)
        {
            SixLabors.ImageSharp.PixelFormats.Rgba32 pixel = _image[x, y];
            return pixel.A == 0;
        }

        public IGraphicsImage Clone(int x, int y, int width, int height)
        {
            var clone = _image.Clone(ctx => ctx.Crop(new SixLabors.ImageSharp.Rectangle(x, y, width, height)));
            return new ImageSharpImage(clone);
        }

        public void DrawOn(IGraphicsCanvas canvas, int x, int y)
        {
            if (canvas is not ImageSharpCanvas imageSharpCanvas)
                throw new System.InvalidCastException("Canvas is not an ImageSharpCanvas.");

            imageSharpCanvas.DrawImage(this, x, y);
        }

        public SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> RawImage => _image;

        public void Dispose()
        {
            _image.Dispose();
        }
    }
}


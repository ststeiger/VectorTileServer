
namespace SpriteGenerator.Impl.WithSkiaSharp 
{


    public class SkiaBackend
        : IGraphicsBackend
    {
        private byte[]? m_imageBytes;

        public byte[] ImageBytes => this.m_imageBytes!;

        public IGraphicsImage LoadImage(string fullPath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
            return LoadImage(imageBytes);
        }

        public IGraphicsImage LoadImage(byte[] imageBytes)
        {
            this.m_imageBytes = imageBytes;

            using SkiaSharp.SKData skData = SkiaSharp.SKData.CreateCopy(imageBytes);
            using SkiaSharp.SKCodec codec = SkiaSharp.SKCodec.Create(skData);
            SkiaSharp.SKImageInfo info = codec.Info;

            SkiaSharp.SKBitmap image = SkiaSharp.SKBitmap.Decode(codec);
            if (image == null)
                throw new System.InvalidOperationException("Could not decode image.");

            return new SkiaImage(image);
        }

        public IGraphicsCanvas CreateCanvas(int width, int height)
        {
            return new SkiaCanvas(width, height);
        }
    }

}

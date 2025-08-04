
namespace SpriteGenerator.Impl.WithImageSharp 
{


    public class ImageSharpBackend 
        : IGraphicsBackend
    {
        private byte[]? m_imageBytes;

        public byte[] ImageBytes => m_imageBytes!;

        public IGraphicsImage LoadImage(string fullPath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
            return LoadImage(imageBytes);
        }

        public IGraphicsImage LoadImage(byte[] imageBytes)
        {
            m_imageBytes = imageBytes;
            using System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes);
            SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = 
                SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(ms);

            return new ImageSharpImage(image.Clone());
        }

        public IGraphicsCanvas CreateCanvas(int width, int height)
        {
            return new ImageSharpCanvas(width, height);
        }
    }
}



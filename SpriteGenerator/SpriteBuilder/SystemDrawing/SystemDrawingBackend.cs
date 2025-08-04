
#define WITH_SystemDrawingCommon
#if WITH_SystemDrawingCommon

namespace SpriteGenerator.Impl.WithSystemDrawing
{
    public class SystemDrawingBackend 
        : IGraphicsBackend
    {

        byte[]? m_imageBytes;

        public byte[] ImageBytes
        {
            get 
            { 
                return m_imageBytes!; }
        }
        
        public IGraphicsImage LoadImage(string fullPath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
            return this.LoadImage(imageBytes);
        }

        public IGraphicsImage LoadImage(byte[] imageBytes)
        {
            this.m_imageBytes = imageBytes;
            using System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes);
            return new SystemDrawingImage(new System.Drawing.Bitmap(ms));
        }

        public IGraphicsCanvas CreateCanvas(int width, int height)
        {
            return new SystemDrawingCanvas(width, height);
        }
    }

}


#endif


#define WITH_SystemDrawingCommon
#if WITH_SystemDrawingCommon


namespace SpriteGenerator.Impl.WithSystemDrawing
{
    public class SystemDrawingCanvas : IGraphicsCanvas
    {
        private readonly System.Drawing.Bitmap _canvas;
        private readonly System.Drawing.Graphics _graphics;

        public SystemDrawingCanvas(int width, int height)
        {
            _canvas = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _graphics = System.Drawing.Graphics.FromImage(_canvas);
            _graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
        }

        public void ClearTransparent()
        {
            _graphics.Clear(System.Drawing.Color.Transparent);
        }

        public void DrawImage(IGraphicsImage image, int x, int y)
        {
            SystemDrawingImage? img = image as SystemDrawingImage;
            _graphics.DrawImage(img.RawBitmap, x, y);
        }

        public byte[] GetImageBytes()
        {
            using System.IO.MemoryStream ms = new System.IO.MemoryStream();
            _canvas.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        public void Dispose()
        {
            _graphics.Dispose();
            _canvas.Dispose();
        }
    }

}
#endif 


#define WITH_SystemDrawingCommon
#if WITH_SystemDrawingCommon


namespace SpriteGenerator.Impl.WithSystemDrawing
{
    public class SystemDrawingImage 
        : IGraphicsImage
    {
        private readonly System.Drawing.Bitmap _bitmap;

        public SystemDrawingImage(System.Drawing.Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public int Width => _bitmap.Width;
        public int Height => _bitmap.Height;

        public bool IsTransparent(int x, int y) => _bitmap.GetPixel(x, y).A == 0;

        public IGraphicsImage Clone(int x, int y, int width, int height)
        {
            return new SystemDrawingImage(_bitmap.Clone(new System.Drawing.Rectangle(x, y, width, height), _bitmap.PixelFormat));
        }

        public void DrawOn(IGraphicsCanvas canvas, int x, int y)
        {
            canvas.DrawImage(this, x, y);
        }

        public System.Drawing.Bitmap RawBitmap => _bitmap;
    }

}

#endif 

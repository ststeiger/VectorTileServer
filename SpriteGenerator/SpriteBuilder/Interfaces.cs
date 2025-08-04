
namespace SpriteGenerator
{


    public interface IGraphicsImage
    {
        int Width { get; }
        int Height { get; }
        bool IsTransparent(int x, int y);
        IGraphicsImage Clone(int x, int y, int width, int height);
        void DrawOn(IGraphicsCanvas canvas, int x, int y);
    }


    public interface IGraphicsCanvas 
        : System.IDisposable
    {
        void ClearTransparent();
        void DrawImage(IGraphicsImage image, int x, int y);
        byte[] GetImageBytes(); // PNG encoded
    }

    public interface IGraphicsBackend
    {

        byte[] ImageBytes { get; }
        
        IGraphicsImage LoadImage(byte[] imageBytes);
        IGraphicsImage LoadImage(string fullPath);
        IGraphicsCanvas CreateCanvas(int width, int height);
    }


}


namespace SpriteGenerator
{


    public class ImageDimensions
    {

        public int Width;
        public int Height;


        public ImageDimensions(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public ImageDimensions()
            : this(0, 0)
        { }

    }


}

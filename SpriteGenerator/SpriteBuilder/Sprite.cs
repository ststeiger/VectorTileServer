
namespace SpriteGenerator
{
    public class Sprite
    {
        public string Name { get; }
        public byte[] ImageBytes { get; }
        public IGraphicsImage Image { get; }
        public ImagePosition? SheetPosition { get; set; } 
        public int Width => Image.Width;
        public int Height => Image.Height;

        public Sprite(string name, IGraphicsBackend backend, int hPad, int vPad, bool crop)
        {
            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            this.ImageBytes = backend.ImageBytes ?? throw new System.ArgumentNullException(nameof(backend.ImageBytes));

            IGraphicsImage image = backend.LoadImage(this.ImageBytes);
            if (crop)
            {
                int left = GetNonTransparentColumn(image, true);
                int right = GetNonTransparentColumn(image, false);
                int top = GetNonTransparentRow(image, true);
                int bottom = GetNonTransparentRow(image, false);
                image = image.Clone(left, top, right - left + 1, bottom - top + 1);
            }

            if (hPad > 0 || vPad > 0)
            {
                IGraphicsCanvas canvas = backend.CreateCanvas(image.Width + 2 * hPad, image.Height + 2 * vPad);
                canvas.ClearTransparent();
                canvas.DrawImage(image, hPad, vPad);
                image = backend.LoadImage(canvas.GetImageBytes()); // rewrap padded image
            }

            Image = image;
        }

        private static int GetNonTransparentColumn(IGraphicsImage image, bool leftToRight)
        {
            int x = leftToRight ? 0 : image.Width - 1;
            int step = leftToRight ? 1 : -1;
            while (x >= 0 && x < image.Width)
            {
                for (int y = 0; y < image.Height; ++y)
                {
                    if (!image.IsTransparent(x, y))
                        return x;
                }
                x += step;
            }
            return leftToRight ? image.Width - 1 : 0;
        }

        private static int GetNonTransparentRow(IGraphicsImage image, bool topToBottom)
        {
            int y = topToBottom ? 0 : image.Height - 1;
            int step = topToBottom ? 1 : -1;
            while (y >= 0 && y < image.Height)
            {
                for (int x = 0; x < image.Width; ++x)
                {
                    if (!image.IsTransparent(x, y))
                        return y;
                }
                y += step;
            }
            return topToBottom ? image.Height - 1 : 0;
        }
    }

}

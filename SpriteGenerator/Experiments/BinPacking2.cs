
namespace SpriteGenerator
{


    public class BoundingBox
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Space
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }


    /// <summary>
    /// From https://observablehq.com/@mourner/simple-rectangle-packing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class BinPacking<T>
    {

        public class Box
        {
            public T? State;


            public int Width { get; set; }
            public int Height { get; set; }
            public int? X { get; set; }  // Assigned after packing
            public int? Y { get; set; }  // Assigned after packing
        }

        public class PackingState
        {

            public BoundingBox? Bounds { get; set; }

            public System.Collections.Generic.List<BinPacking<T>.Box>? Packed { get; set; }
            public System.Collections.Generic.List<Space>? Spaces { get; set; }
        }


        public static BoundingBox GetBoundingBox(System.Collections.Generic.List<Box> packed)
        {
            int maxX = 0;
            int maxY = 0;

            foreach (Box box in packed)
            {
                if (box.X.HasValue && box.Y.HasValue)
                {
                    maxX = System.Math.Max(maxX, box.X.Value + box.Width);
                    maxY = System.Math.Max(maxY, box.Y.Value + box.Height);
                }
            }

            return new BoundingBox
            {
                Width = maxX,
                Height = maxY
            };
        }

        public static PackingState PackBoxes(System.Collections.Generic.List<Box> boxes)
        {
            if (boxes == null || boxes.Count == 0)
                return new PackingState
                {
                    Packed = new System.Collections.Generic.List<Box>(),
                    Spaces = new System.Collections.Generic.List<Space>()
                };

            // Calculate total area and max width
            int area = 0;
            int maxWidth = 0;
            foreach (Box box in boxes)
            {
                area += box.Width * box.Height;
                maxWidth = System.Math.Max(maxWidth, box.Width);
            }

            // Sort boxes descending by height
            // boxes = boxes.OrderByDescending(b => b.Height).ToList();
            boxes.Sort((b1, b2) => b2.Height.CompareTo(b1.Height));

            // Start width approximation
            int startWidth = System.Math.Max((int)System.Math.Ceiling(System.Math.Sqrt(area / 0.95)), maxWidth);

            System.Collections.Generic.List<Space> spaces = new System.Collections.Generic.List<Space>
            {
                new Space { X = 0, Y = 0, Width = startWidth, Height = int.MaxValue }
            };

            System.Collections.Generic.List<Box> packed = new System.Collections.Generic.List<Box>();

            foreach (Box box in boxes)
            {
                for (int i = spaces.Count - 1; i >= 0; i--)
                {
                    Space space = spaces[i];

                    if (box.Width > space.Width || box.Height > space.Height)
                        continue;

                    Box packedBox = new Box
                    {
                        Width = box.Width,
                        Height = box.Height,
                        State = box.State,
                        X = space.X,
                        Y = space.Y
                    };
                    packed.Add(packedBox);

                    if (box.Width == space.Width && box.Height == space.Height)
                    {
                        Space last = spaces[spaces.Count - 1];
                        spaces.RemoveAt(spaces.Count - 1);
                        if (i < spaces.Count)
                            spaces[i] = last;
                    }
                    else if (box.Height == space.Height)
                    {
                        space.X += box.Width;
                        space.Width -= box.Width;
                    }
                    else if (box.Width == space.Width)
                    {
                        space.Y += box.Height;
                        space.Height -= box.Height;
                    }
                    else
                    {
                        spaces.Add(new Space
                        {
                            X = space.X + box.Width,
                            Y = space.Y,
                            Width = space.Width - box.Width,
                            Height = box.Height
                        });

                        space.Y += box.Height;
                        space.Height -= box.Height;
                    }

                    break;
                }
            }

            return new PackingState
            {
                Packed = packed,
                Spaces = spaces,
                Bounds = GetBoundingBox(packed)
            };
        }
    }
}

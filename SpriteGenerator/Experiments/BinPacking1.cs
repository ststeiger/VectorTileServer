
namespace SpriteGenerator
{
    
    
    // using System.Linq;


    public partial class BinPacking<T>
    {


        public static System.Collections.Generic.IEnumerable<PackingState> OldPackBoxes(
            System.Collections.Generic.List<Box> boxes
        )
        {
            if (boxes == null || boxes.Count == 0)
                yield break;

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

                    // Add box to packed list with coordinates
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

                // Return current state (equivalent to yield in JS)

                // Create new lists to hold the copied objects
                System.Collections.Generic.List<Box> packedBoxes = 
                    new System.Collections.Generic.List<Box>();

                System.Collections.Generic.List<Space> freeSpaces = 
                    new System.Collections.Generic.List<Space>();

                // Iterate and copy each box
                foreach (Box b in packed)
                {
                    packedBoxes.Add(new Box
                    {
                        Width = b.Width,
                        Height = b.Height,
                        X = b.X,
                        Y = b.Y
                    });
                }

                // Iterate and copy each space
                foreach (Space s in spaces)
                {
                    freeSpaces.Add(new Space
                    {
                        X = s.X,
                        Y = s.Y,
                        Width = s.Width,
                        Height = s.Height
                    });
                }

                // Return the new PackingState object with the copied lists
                yield return new PackingState
                {
                    Packed = packedBoxes,
                    Spaces = freeSpaces
                };


                //yield return new PackingState
                //{
                //    Packed = packed.Select(b => new Box
                //    {
                //        Width = b.Width,
                //        Height = b.Height,
                //        X = b.X,
                //        Y = b.Y
                //    }).ToList(),
                //    Spaces = spaces.Select(s => new Space
                //    {
                //        X = s.X,
                //        Y = s.Y,
                //        Width = s.Width,
                //        Height = s.Height
                //    }).ToList()
                //};
            }
        }
    }

}

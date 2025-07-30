
namespace VectorTileSelector
{


    public struct Wgs84Point
    {
        public double Latitude { get; } // Breite/Breitengrad  
        public double Longitude { get;  } // Länge/Längengrad
        public double Altitude { get; } // Add Altitude for 3D WGS84

        public Wgs84Point(double latitude, double longitude, double altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }

        public override string ToString()
        {
            return $"WGS84 ({Latitude:F6}, {Longitude:F6}, {Altitude:F6})";
        }
    }


    public struct Point3D
    {
        public double X { get; }
        public double Y { get; }
        public double Z { get;  }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the WGS84 (EPSG:4326 Latitude/Longitude) representation of this point.
        /// WARNING: This is a computed property that involves a non-trivial coordinate transformation.
        /// Consider caching the result if accessed frequently, or use a method instead for clarity on computation cost.
        /// </summary>
        public Wgs84Point Wgs84
        {
            get
            {
                return ProjectEsriCoordinatesToWGS84(this);
            }
        } // End Property Wgs84 


        public static Wgs84Point ProjectEsriCoordinatesToWGS84(
            Point3D point
        )
        {
            DotSpatial.Projections.ProjectionInfo projFrom = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(2056);
            DotSpatial.Projections.ProjectionInfo projTo = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;
            // DotSpatial.Projections.ProjectionInfo projTo = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(3857); // Web Mercator projection
            // DotSpatial.Projections.ProjectionInfo projTo = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(4326); // WGS84



            double[] latLonPoints = new double[2];
            double[] z = new double[1];

            latLonPoints[0] = (double)point.X;
            latLonPoints[1] = (double)point.Y;
            z[0] = point.Z;


            // prepare for ReprojectPoints (it's mutate array)
            DotSpatial.Projections.Reproject.ReprojectPoints(
                latLonPoints, z, projFrom, projTo
                , 0, 1
            );

            Wgs84Point p = new Wgs84Point(latLonPoints[1], latLonPoints[0], z[0]);
            return p;
        }

        public override string ToString()
        {
            return $"LV95 ({X:F3}, {Y:F3}, {Z:F3})";
        }
    }

    public class GmlParser
    {
        public static System.Collections.Generic.List<Point3D> 
            ParsePosList(string posListString)
        {
            System.Collections.Generic.List<Point3D> points = 
                new System.Collections.Generic.List<Point3D>();

            // Define the array of delimiters
            char[] delimiters = new char[] { 
                ' ', '\t'
                , '\n', '\r'
                , '\v', '\f'
                ,'\u00A0' // nbsp 
            }; // Space, Tab, Newline, Carriage Return


            // Split the string by space. This will give you all individual coordinate values.
            string[] rawCoordinates = posListString.Split(delimiters
                , System.StringSplitOptions.RemoveEmptyEntries
            );

            // Check if the number of coordinates is a multiple of 3 (X, Y, Z)
            if (rawCoordinates.Length % 3 != 0)
            {
                throw new System.ArgumentException("The posList string does not contain a valid number of X, Y, Z coordinates.", nameof(posListString));
            }

            // Iterate through the raw coordinates, taking 3 at a time for X, Y, Z
            for (int i = 0; i < rawCoordinates.Length; i += 3)
            {
                // Use CultureInfo.InvariantCulture for parsing doubles to avoid issues
                // with different decimal separators (e.g., comma vs. period)
                if (
                    double.TryParse(rawCoordinates[i], System.Globalization.CultureInfo.InvariantCulture, out double x) &&
                    double.TryParse(rawCoordinates[i + 1], System.Globalization.CultureInfo.InvariantCulture, out double y) &&
                    double.TryParse(rawCoordinates[i + 2], System.Globalization.CultureInfo.InvariantCulture, out double z)
                )
                {
                    points.Add( new Point3D(x, y, z) );
                }
                else
                {
                    // Handle parsing errors if any coordinate is not a valid double
                    throw new System.FormatException($"Could not parse coordinate values at index {i}. Invalid format.");
                }
            }

            return points;
        }

        public static void Test()
        {
            string posList = "2680233.973000001 1291339.3619999997 518.2379999999976 2680254.0929999985 1291337.0119999982 518.2379999999976 2680245.2490000017 1291346.335000001 518.2379999999976 2680233.973000001 1291339.3619999997 518.2379999999976";

            try
            {
                System.Collections.Generic.List<Point3D> parsedPoints = ParsePosList(posList);

                System.Console.WriteLine("Parsed Points:");
                foreach (Point3D point in parsedPoints)
                {
                    System.Console.WriteLine(point.Wgs84);
                }
            }
            catch (System.ArgumentException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
            catch (System.FormatException ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
            }
        }


    }


}

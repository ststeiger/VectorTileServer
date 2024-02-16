using System.ComponentModel.DataAnnotations;

namespace TestSQLite
{
    public class BoundingBox
    {
        public decimal MinLatitude { get; set; }
        public decimal MinLongitude { get; set; }

        public decimal MaxLatitude { get; set; }
        public decimal MaxLongitude { get; set; }


        public BoundingBox(decimal minLatitude, decimal minLongitude, decimal maxLatitude, decimal maxLongitude)
        {
            MinLatitude = minLatitude;
            MinLongitude = minLongitude;
            MaxLatitude = maxLatitude;
            MaxLongitude = maxLongitude;
        }
        
        
        
        //   Top Left -------------- Top Right
        //      |                        |
        //      |                        |
        //      |                        |
        //   Bottom Left ---------- Bottom Right
        
        
        //   northwest -------------- northeast
        //      |                        |
        //      |                        |
        //      |                        |
        //   southwest -------------- southeast
        
         
        // Top Left:     northwest corner (MinLatitude, MinLongitude) 
        // Top Right:    northeast corner (MinLatitude, MaxLongitude) 
        // Bottom Left:  southwest corner (MaxLatitude, MinLongitude) 
        // Bottom Right: southeast corner (MaxLatitude, MaxLongitude) 
        public override string ToString()
        {
            string s = MinLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
                       + ","
                       + MinLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
                       + " "
                       + MaxLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
                       + ","
                       + MaxLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
                ;

            return s;
        }
        
        
        public static BoundingBox Parse(string input)
        {
            // Replace whitespaces and other separators with comma
            input = input.Replace(" ", "").Replace("\t", "").Replace(";", ",");

            string[] parts = input.Split(',');

            System.Collections.Generic.List<decimal> coordinates = new System.Collections.Generic.List<decimal>();
            foreach (string part in parts)
            {
                if (decimal.TryParse(part, System.Globalization.NumberStyles.AllowDecimalPoint,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal coord))
                {
                    coordinates.Add(coord);
                }
                else
                {
                    throw new System.ArgumentException($"Invalid coordinate: {part}");
                }
            }

            if (coordinates.Count != 4)
            {
                throw new System.IO.InvalidDataException("Invalid input. Expected format: long,lat,long,lat");
            }

            BoundingBox bbox = new BoundingBox(coordinates[1], coordinates[0], coordinates[3], coordinates[2]);
            return bbox;
        }
    }
}
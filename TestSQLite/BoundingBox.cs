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
    }

    public class BoundingBoxParser
    {
        public static void Test()
        {
            string input = "41.06894,11.36768,55.51167,19.03189";
            //  string input = "1,2,3,4";
        
            // Replace whitespaces and other separators with comma
            input = input.Replace(" ", "").Replace("\t", "").Replace(";", ",");

            string[] parts = input.Split(',');

            System.Collections.Generic.List<decimal> coordinates = new System.Collections.Generic.List<decimal>();
            foreach (string part in parts)
            {
                if (decimal.TryParse(part, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out decimal coord))
                {
                    coordinates.Add(coord);
                }
                else
                {
                    System.Console.WriteLine($"Invalid coordinate: {part}");
                }
            }

            if (coordinates.Count == 4)
            {
                BoundingBox bbox = new BoundingBox(coordinates[1], coordinates[0], coordinates[3], coordinates[2]);
                
                
                System.Console.WriteLine($"Bounding Box: Min Longitude: {bbox.MinLongitude}, Max Longitude: {bbox.MaxLongitude}, Min Latitude: {bbox.MinLatitude}, Max Latitude: {bbox.MaxLatitude}");


                System.Console.WriteLine(bbox.MinLatitude.ToString() + "," + bbox.MinLongitude.ToString() + "    " +
                                         bbox.MaxLatitude.ToString() + "," + bbox.MaxLongitude.ToString());
                // assert("2,1 4,3");
            }
            else
            {
                System.Console.WriteLine("Invalid input. Expected format: long,lat,long,lat");
            }
            
        }
    }

}
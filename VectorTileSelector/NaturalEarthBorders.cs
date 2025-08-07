
namespace VectorTileSelector
{

    // <PackageReference Include = "NetTopologySuite" Version="2.6.0" />
    // <PackageReference Include = "NetTopologySuite.IO.GeoJSON" Version="4.0.0" />


    // upstream is at 
    // https://naciscdn.org/naturalearth/packages/natural_earth_vector.sqlite.zip 
    // following is same URL as used in the OpenMapTiles, a mirror maintained by MapTiler 
    // https://dev.maptiler.download/geodata/omt/natural_earth_vector.sqlite.zip

    public struct BoundingBox
    {
        public double MinLongitude { get; }
        public double MinLatitude { get; }
        public double MaxLongitude { get; }
        public double MaxLatitude { get; }


        public BoundingBox(double minLon, double minLat, double maxLon, double maxLat)
        {
            this.MinLongitude = minLon;
            this.MinLatitude = minLat;
            this.MaxLongitude = maxLon;
            this.MaxLatitude = maxLat;
        } // End Constructor 


        public override string ToString()
        {
            // return $"MinLat: {MinLatitude}, MinLon: {MinLongitude}, MaxLat: {MaxLatitude}, MaxLon: {MaxLongitude}";

            // (xMin,yMin,xMax,yMax) 
            return $"({MinLongitude:F6},{MinLatitude:F6},{MaxLongitude:F6},{MaxLatitude:F6})";
        } // End Function ToString 


        /// <summary>
        /// Expands a bounding box by a given linear distance (in centimeters) on all sides.
        /// </summary>
        /// <param name="originalBbox">The original bounding box.</param>
        /// <param name="bufferCm">The buffer distance in centimeters.</param>
        /// <returns>A new BoundingBox expanded by the specified buffer.</returns>
        public static BoundingBox ExpandBoundingBox(BoundingBox originalBbox, double bufferCm)
        {
            // 1. Convert buffer from centimeters to meters
            double bufferMeters = bufferCm / 100.0; // 0.5 cm = 0.005 meters

            // 2. Calculate the average latitude of the bounding box
            // This latitude is used for a more accurate conversion of longitude meters to degrees.
            double centerLat = (originalBbox.MinLatitude + originalBbox.MaxLatitude) / 2.0;

            // 3. Approximate meters per degree for latitude and longitude (WGS84 ellipsoid average values)
            // A degree of latitude is roughly constant
            const double metersPerDegreeLat = 111139; // meters per degree latitude

            // A degree of longitude varies by latitude: meters_per_degree_lon = ~111320 * cos(latitude_in_radians)
            // We use equatorial value and adjust by cosine of the center latitude
            const double equatorialMetersPerDegreeLon = 111320; // meters per degree longitude at the equator
            double centerLatRadians = centerLat * System.Math.PI / 180.0; // Convert center latitude to radians

            // Calculate meters per degree longitude at the specific center latitude
            double metersPerDegreeLon = equatorialMetersPerDegreeLon * System.Math.Cos(centerLatRadians);

            // Avoid division by zero or extremely small numbers if very close to poles
            // For practical purposes, near poles, longitude expansion becomes less meaningful in terms of linear distance
            if (System.Math.Abs(metersPerDegreeLon) < 0.0001)
                metersPerDegreeLon = 0.0001; // Use a small value to prevent error, or decide to not expand longitude at poles

            // 4. Convert the buffer distance from meters to degrees
            double deltaLat = bufferMeters / metersPerDegreeLat;
            double deltaLon = bufferMeters / metersPerDegreeLon;

            // 5. Apply the calculated deltas to the original bounding box coordinates
            double newMinLat = originalBbox.MinLatitude - deltaLat;
            double newMaxLat = originalBbox.MaxLatitude + deltaLat;
            double newMinLon = originalBbox.MinLongitude - deltaLon;
            double newMaxLon = originalBbox.MaxLongitude + deltaLon;

            // 6. Clamp latitude values to ensure they stay within valid range (-90 to 90)
            newMinLat = System.Math.Max(newMinLat, -90.0);
            newMaxLat = System.Math.Min(newMaxLat, 90.0);

            // For longitude, for very wide bounding boxes that cross the anti-meridian (180/-180),
            // expanding it might require more complex logic (e.g., if newMinLon < -180 it wraps around).
            // For small buffers and typical geographic areas, direct subtraction/addition is usually sufficient.
            // If the expansion causes it to exceed +/-180, GeoJSON tools will typically handle the rendering correctly.

            return new BoundingBox(newMinLon, newMinLat, newMaxLon, newMaxLat);
        } // End Function ExpandBoundingBox 


        public static BoundingBox ProjectWGS84ToWebMercatorBounds(BoundingBox wgs84BoundingBox)
        {
            DotSpatial.Projections.ProjectionInfo projFrom = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(4326); // WGS84
            DotSpatial.Projections.ProjectionInfo projTo = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(3857); // Web Mercator projection

            Wgs84Point1[] mycoordinates = new Wgs84Point1[2];
            mycoordinates[0] = new Wgs84Point1(wgs84BoundingBox.MinLongitude, wgs84BoundingBox.MinLatitude);
            mycoordinates[1] = new Wgs84Point1(wgs84BoundingBox.MaxLongitude, wgs84BoundingBox.MaxLatitude);


            double[] latLonPoints = new double[mycoordinates.Length * 2];
            double[] z = new double[mycoordinates.Length];

            // dotspatial takes the x,y in a single array, and z in a separate array.  I'm sure there's a 
            // reason for this, but I don't know what it is.
            for (int i = 0; i < mycoordinates.Length; i++)
            {
                latLonPoints[i * 2] = (double)mycoordinates[i].Longitude;
                latLonPoints[i * 2 + 1] = (double)mycoordinates[i].Latitude;
                z[i] = 0;
            } // Next i 

            // prepare for ReprojectPoints (it's mutate array)
            DotSpatial.Projections.Reproject.ReprojectPoints(
                latLonPoints, z, projFrom, projTo
                , 0, latLonPoints.Length / 2
            );

            // assemblying new points array to create polygon
            Wgs84Point1[] polyPoints = new Wgs84Point1[latLonPoints.Length / 2];

            for (int i = 0; i < latLonPoints.Length / 2; ++i)
            {
                polyPoints[i] = new Wgs84Point1(latLonPoints[i * 2], latLonPoints[i * 2 + 1]);
            } // Next i 

            // Note: Labels in BoundingBox would be incorrect now. 
            BoundingBox webMercatorBounds = new BoundingBox(polyPoints[0].Longitude, polyPoints[0].Latitude, polyPoints[1].Longitude, polyPoints[1].Latitude);
            return webMercatorBounds;
        } // End Function ProjectWGS84ToWebMercatorBounds 



        /// <summary>
        /// Computes the bounding box for an array of Wgs84Point.
        /// </summary>
        /// <param name="points">The array of Wgs84Point to compute the bounding box for.</param>
        /// <returns>A BoundingBox struct containing the min/max latitudes and longitudes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the points array is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the points array is empty.</exception>
        public static BoundingBox FromPointArray(Wgs84Point1[] points)
        {
            if (points == null)
                throw new System.ArgumentNullException(nameof(points), "The array of Wgs84Point cannot be null.");

            if (points.Length == 0)
                throw new System.ArgumentException("The array of Wgs84Point cannot be empty.", nameof(points));

            // Initialize with the first point's values
            double minLat = points[0].Latitude;
            double maxLat = points[0].Latitude;
            double minLon = points[0].Longitude;
            double maxLon = points[0].Longitude;

            // Iterate through the rest of the points to find min/max values
            for (int i = 1; i < points.Length; i++)
            {
                minLat = System.Math.Min(minLat, points[i].Latitude);
                maxLat = System.Math.Max(maxLat, points[i].Latitude);
                minLon = System.Math.Min(minLon, points[i].Longitude);
                maxLon = System.Math.Max(maxLon, points[i].Longitude);
            } // Next i 

            BoundingBox exactBounds = new BoundingBox(minLon, minLat, maxLon, maxLat);

            // BoundingBox webMercatorBounds = ProjectWGS84ToWebMercatorBounds(exactBounds);
            BoundingBox safeBounds = ExpandBoundingBox(exactBounds, 100 * 1000 * 5); // expand 10 km
            return safeBounds;
        } // End Function FromPointArray 


    } // End Struct BoundingBox 


    public struct Lv95Point
    {

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }


        public Lv95Point(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        } // End Constructor 


        public Lv95Point(double x, double y)
            :this(x,y, 0)
        { } // End Constructor 


        public Wgs84Point1 ProjectLv95ToWgs84()
        {
            DotSpatial.Projections.ProjectionInfo projFrom = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(2056);
            DotSpatial.Projections.ProjectionInfo projTo = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;

            double[] latLonPoints = new double[2];
            double[] z = new double[1];

            latLonPoints[0] = (double)this.X;
            latLonPoints[1] = (double)this.Y;
            z[0] = (double)this.Z;

            // prepare for ReprojectPoints (it's mutate array)
            DotSpatial.Projections.Reproject.ReprojectPoints(
                latLonPoints, z, projFrom, projTo
                , 0, latLonPoints.Length / 2
            );

            Wgs84Point1 wgs84Point = new Wgs84Point1(latLonPoints[0], latLonPoints[1]);
            return wgs84Point;
        } // End Function ProjectLv95ToWgs84 


    } // End Struct Lv95Point 


    public struct Wgs84Point1
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }


        public Wgs84Point1(double longitude, double latitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        } // End Constructor 


        public override string ToString()
        {
            return $"Lat: {this.Latitude}, Lon: {this.Longitude}";
        } // End Function ToString 


    } // End Struct Wgs84Point1 


    public class PolygonData
    {
        // The points forming the outer boundary of the polygon
        public Wgs84Point1[] ExteriorRingPoints { get; set; }

        // A list of arrays of points, where each inner array represents a hole
        public System.Collections.Generic.List<Wgs84Point1[]> InteriorRingPoints { get; set; }


        public PolygonData()
        {
            this.InteriorRingPoints = new System.Collections.Generic.List<Wgs84Point1[]>();
        } // End Constructor 


        public override string ToString()
        {
            return $"Polygon (Exterior: {ExteriorRingPoints?.Length ?? 0} pts, Holes: {InteriorRingPoints?.Count ?? 0})";
        } // End Function ToString 


    } // End Class PolygonData 


    internal class NaturalEarthBorders
    {


        /// <summary>
        /// Ensures that the +towgs84 parameter in a Proj4 string has exactly 7 comma-separated values.
        /// Appends trailing zeros if fewer than 7 are present.
        /// </summary>
        /// <param name="proj4String">The original Proj4 string from DotSpatial.</param>
        /// <returns>The modified Proj4 string with a 7-parameter +towgs84, or the original string if no +towgs84 is found.</returns>
        public static string EnsureFullTowgs84(string proj4String)
        {
            // Regex to find the +towgs84 parameter and capture its values
            // It looks for "+towgs84=" followed by numbers separated by commas.
            // It's non-greedy to stop before the next '+' or end of string.
            string pattern = @"(\+towgs84=[\d.-]+(?:,[\d.-]+){0,6})"; // Captures 1 to 7 numbers

            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(proj4String, pattern);

            if (match.Success)
            {
                string towgs84Part = match.Groups[1].Value; // e.g., "+towgs84=674.374,15.056,405.346"
                string valuesString = towgs84Part.Substring(towgs84Part.IndexOf('=') + 1); // e.g., "674.374,15.056,405.346"
                string[] values = valuesString.Split(',');

                if (values.Length < 7)
                {
                    // Calculate how many zeros are needed
                    int zerosToAdd = 7 - values.Length;
                    // string trailingZeros = new string(',', zerosToAdd).Replace(",", ",0");
                    // string trailingZeros = string.Concat(System.Linq.Enumerable.Repeat(",0", zerosToAdd));

                    System.Text.StringBuilder sb = new System.Text.StringBuilder(",0".Length * zerosToAdd);
                    for (int i = 0; i < zerosToAdd; i++)
                    {
                        sb.Append(",0");
                    } // Next i 

                    string trailingZeros = sb.ToString();

                    // Construct the new +towgs84 part
                    string newTowgs84Part = towgs84Part + trailingZeros;

                    // Replace the old +towgs84 part with the new one in the original string
                    return proj4String.Replace(towgs84Part, newTowgs84Part);
                } // End if (values.Length < 7) 

                // If already 7 or more parameters (shouldn't happen for valid +towgs84 but good to handle)
                return proj4String;
            } // End if (match.Success) 

            // No +towgs84 found, return original string
            return proj4String;
        } // End Function EnsureFullTowgs84 


        public static void Test()
        {
            string hexString = @"0x010300000001000000cc000000c0a3a329f65f3540feca8b651da54b40180fb9d7d1673540d203d0a9dda24b40b897af9db87735403afc3610079b4b40e8b6550e028135406214606ce2984b40d0923826e99a35400e371fc5a3984b404863b8d755a5354026079ad92b974b4090128c7b5ab73540b6142d3921934b40d83225dbbebb35402a26de1d73914b40707daf002ac035403eb2f2cb60904b40c875d022e4d135409e1f9002468f4b40b0db0c9098da35406e384584158d4b40f85c0d2d8bdf354076c2b5c1268c4b4040fcefa797e83540c2cb5dcf808b4b4098b6a429d6ee354046b7939f4c8a4b4020991ba159f135407295d980058a4b4010b8e1d08bf3354046b7939f4c8a4b40d8b023783dfa35409244963cc68b4b4090727693e10036401ec5e557f78b4b40384d5d48f30336406ace8d65518b4b40e03a8641c504364016787a54e5894b40e03a8641c5043640868f178bcc874b40b06b314f19073640f6683aadae854b4060e27193c90c36401257a0134b844b40002488a4aa13364002414584a8834b40f0a4961891193640aa5d188bd4834b40e828c74b541e3640369415eed5844b4068c7d75c3d213640b2c1e35703864b40a060884185243640c25ce9f4fd864b4010f08207544036407ae1cfa911894b40f0aae51917453640c0755296e6884b4088e7c84bb06f36402276004065874b40c001c074848a3640dabfaf87b2894b40907070f6c2903640b6a8b5c1ad884b40b0e847e437923640ee9210d030884b40488484a4a69436400e38862b60874b40c01dae9d749736406a3099d9d0844b40103960e5bd9b36400212ef2e077f4b40204deda7279f3640d27252f8297c4b40b8f0e4d02ba23640e224e2ba9d7a4b4028c8a9c660a636402e2e8ac8f7794b4038592278c5b43640ea4c0140007a4b405082b6d72db73640a2ad3aadc6794b40e8b50a90d8b83640b2f344843d794b40902535ecabba3640b65d26ff5d784b40a085b8d721bd36405a88751a91774b4030e5499a8bc036404287d6e32e774b40c07d2815b0bd364032bf15ee74754b4050a405f3cdbf36404a6915ee76744b402088a72912cf3640769f299c66724b40401c71f63ed23640220c3bad57714b40f00b5f484fd43640f61c606cb56f4b40309d7f6a25d836402e79198b876a4b40784972932dd936407a4ff9052b684b40e0971f3e84d83640aaa740e7e7654b40603d759335d5364016c1818eab634b40d8ea5aab58d13640b6f1bcfb73624b40507a5ee561c93640c2a5f02ec4604b407822e4d0e3c53640e2b72d390a5f4b4020bd00b9ccc336404a1fdf1d5a5e4b40c02e89414dbf3640e62214eea45d4b40e04e42609abc3640da6ac235f85c4b40b8a571f66eba3640ce2eec91cf5c4b4000ea50718fb93640c612a5b0975c4b4040b3daf907b93640c23ac435fa5b4b4000ea50718fb93640fe3ce1ba5c5b4b4060a8a18c3fba36404e64741acb5a4b40a02dc5ae1dba36409e6b78b7485a4b4020af6dbc81b436405668e657e0574b40c03483076cb336408e6e4bbefb564b408873894165b33640b27a9c76ac554b406063f0a7fbb4364092fb8e02c0524b40481aa32946b53640b2e9f4685c514b40e868c9e822b4364016e54abe0b4f4b4080ea71f686ae3640faab717df2484b40c07a479ab3ac36402a96731a033f4b408065c7e82eae364046927ff1023a4b406063f0a7fbb43640667f707d99354b40b837648268c43640b63b01409a2d4b40f0f82578c9b23640aeb1ba5ee52b4b40d04aa6c6a88236408a0478b7a42c4b4010a5d75cf14636402aa0097a932d4b40d0ba9defbaff35404271ac6aad2e4b40a0d0638284b835404e424f5bc72f4b40e8e034ec934935402230125181314b40104f83a4faed344082dda3b0eb324b4048a405f30da43440a2d3bc7b11344b40a0f98741215a3440c6c9d54637354b40484a7ccdf6f63340c2174684c3364b40b85342101dc23340923a94b095374b40e069dc9626ae3340da45a013e5374b40e0f7a2297aa13340fa1ad8e32b394b40805db75c0b9c334050a3cf35763a4b40501910b2109c334050a3cf35763a4b40e08f0fb260a03340b0c5cf35023b4b40e08f0fb260a0334050a3cf35f63b4b406070b85cebbb3340d893d0354a434b40f0506107f6d433401804258b8f4c4b409089640716e233407871d035fe534b40007d0eb280e33340b0c5cf3582544b40501910b290e4334088427ae09c544b40f050610776e53340b0c5cf35c2544b40b0e00cb270e63340d0b2ce3562554b402096ba5cabe6334088f7ce35fa554b40b076630736e6334018b979e06c574b40b0e00cb270e63340b0c5cf3502584b40d0636207d6e73340f816268bef584b404083b95c4bea334018b979e0ec594b40206a0db220ee334030b6d035165c4b4090f30db2d0f43340d0b2ce35e2604b4070060fb230f6334060bf248bf7624b40f050610776f533407871d035fe654b404083b95ccbf3334068557be07c684b40007d0eb200ed33407871d035fe6e4b409089640716ec334040877ae034704b4070060fb2b0eb3340985ecf35de714b4070060fb230ef3340985ecf355e754b40d0636207d6f03340680ad0359a774b406070b85cebf4334030b6d035d6794b40805db75c0bfa334060bf248b777b4b40b0e00cb2f0fe3340f880cf35ea7b4b40805db75c8b053440b0c5cf35c2794b40206a0db2a0083440985ecf355e794b4070060fb230133440f880cf352a7a4b4040570cb2c0243440607479e0d4794b4070060fb2b034344028d5ce35ae7a4b4090896407965d344088427ae01c794b4070060fb23068344008e8cf35ce794b40007d0eb2807b3440985ecf35de7c4b4090f30db250a234407871d0353e864b40f0506107f6ce34401804258b8f964b40007d0eb280d23440f880cf352a974b4010f9ca88b0ec34406e044e212ea44b40a0fe82544bfd34409270af79f5a24b4090f30db250fd344088427ae09ca24b4070060fb2b0f33440680ad0359aa04b406070b85cebf03440607479e0d49f4b40b0e00cb270f13440c02cd035669f4b40007d0eb280f13440f8cb7ae04c9f4b40e08f0fb2e0ef344028d5ce356e9e4b4040570cb2c0e93440680ad035da9d4b4040570cb240e73440d048258be79c4b4090f30db2d0e7344088f7ce35fa9b4b4070060fb230e5344028d5ce35ae9b4b40007d0eb200e4344050a3cf35369b4b40b0e00cb270e33440985ecf355e9a4b4090f30db250e23440680ad0355a994b40007d0eb280e0344008e8cf358e984b40d063620756dc344050a3cf3536974b40e08f0fb260da3440d893d0354a964b40b0766307b6d5344050a3cf3576934b40e08f0fb2e0d23440403ccf3552924b40805db75c0bc63440d048258ba78e4b409089640716b0344068557be07c864b40e08f0fb2e08e3440f880cf35aa7d4b40f0506107768c344028d5ce352e7d4b40b0766307368a3440b0c5cf35027d4b40805db75c8b883440985ecf359e7c4b406070b85ceb87344088f7ce357a7b4b4070060fb2b088344030b6d035d67a4b4090896407968a344088427ae0dc794b406070b85ceb8c344028d5ce35ee784b40e08f0fb2e08e344018b979e06c784b4070060fb2b093344028d5ce352e784b40007d0eb280c3344008e8cf354e794b40206a0db220ce344018b979e06c784b4040570cb240dc3440c02cd035a6744b409089640716f8344018b979e02c734b40501910b2100a354050a3cf35b6734b40805db75c0b0d3540f035248b87744b4090f30db2500e3540f8cb7ae00c754b40b0e00cb2f01035407871d0357e744b40f050610776133540d0b2ce35a2734b40b07663073614354018b979e02c734b40b076630736173540c02cd03566734b4090f30db2d0183540c02cd035e6734b4090f30db2d019354088f7ce357a744b4070060fb2301b354028d5ce35ee744b40b0766307b6303540b0107be0a4764b406070b85ceb3a354028d5ce352e784b4090f30db2503e3540d893d0350a7b4b40007d0eb2803c3540d893d0358a7d4b402096ba5c2b383540204fd035b2814b40b0e00cb2f0313540607479e054914b40206a0db22031354088427ae05c924b40805db75c0b31354030b6d035d6924b40f0506107f62e354030b6d03596954b406070b85c6b2e3540c02cd035e6954b406070b85c6b2e3540888d258bbf984b40f0506107f62e3540403ccf35d2994b40d063620756303540f880cf35aa9a4b40206a0db2a0333540204fd035729b4b40d0636207d6353540403ccf35129b4b40f050610776373540e019cf35469a4b40501910b210393540d893d035ca994b40206a0db2a03c354028d5ce35ae994b40b0766307b63f3540d893d035ca994b4090f30db25042354068557be03c9a4b40007d0eb280443540680ad0351a9b4b40f050610776453540204fd035329c4b40f0506107f6453540f035248bc79d4b40b0766307b645354050a3cf35369f4b40007d0eb280443540607479e0d49f4b4048ad18048f593540faced346d6a44b40c0a3a329f65f3540feca8b651da54b40";
            byte[] geometryBytes = ByteArrayHelper.StringToByteArray(hexString);


            DotSpatial.Projections.ProjectionInfo lv03 = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(21781);
            string proj4jStringLV03 = EnsureFullTowgs84(lv03.ToProj4String());


            // 47.551795288113375, 9.225877271521504

            // https://epsg.io/transform#s_srs=2056&t_srs=4326&x=2734522.5720128&y=1268318.0951206
            // Wgs84Point1 wgs = new Lv95Point(2734522.5720128273, 1268318.0951206307).ProjectLv95ToWgs84();
            Wgs84Point1 wgs = new Lv95Point(0, 0).ProjectLv95ToWgs84();
            System.Console.WriteLine(wgs);


            NetTopologySuite.Geometries.Geometry kaliningradGeometry = null;
            try
            {
                // WKBReader works with a Stream, so wrap the byte array in a MemoryStream
                NetTopologySuite.IO.WKBReader wkbReader = new NetTopologySuite.IO.WKBReader();
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(geometryBytes))
                {
                    kaliningradGeometry = wkbReader.Read(ms);
                } // End using ms 

            } // End Try 
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error reading WKB: {ex.Message}");
            }

            if (kaliningradGeometry != null)
            {
                // 3. Convert the Geometry object to WKT
                NetTopologySuite.IO.WKTWriter wktWriter = new NetTopologySuite.IO.WKTWriter();

                string wkt = wktWriter.Write(kaliningradGeometry);
                // System.Console.WriteLine("WKT:\n" + wkt.Substring(0, System.Math.Min(wkt.Length, 500)) + "..."); // Print first 500 chars
                // System.Console.WriteLine("WKT:\n" + wkt); // Print first 500 chars


                NetTopologySuite.IO.GeoJsonWriter geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();

                // Convert Geometry to GeoJSON string
                string kaliningradGeoJson = geoJsonWriter.Write(kaliningradGeometry);
                System.Console.WriteLine("GeoJson:\n" + kaliningradGeoJson); // Print first 500 chars


                if (kaliningradGeometry is NetTopologySuite.Geometries.Polygon polygon)
                {
                    // Access the exterior ring coordinates
                    NetTopologySuite.Geometries.Coordinate[] coordinates = polygon.ExteriorRing.Coordinates;

                    // Convert to Wgs84Point array
                    Wgs84Point1[] wgs84Points = new Wgs84Point1[coordinates.Length];

                    for (int i = 0; i < coordinates.Length; ++i)
                    {
                        wgs84Points[i] = new Wgs84Point1(coordinates[i].X, coordinates[i].Y);
                    } // Next i 


                    // http://bboxfinder.com
                    BoundingBox bbox = BoundingBox.FromPointArray(wgs84Points);
                    System.Console.WriteLine(bbox); // (19.531597,54.297949,22.926303,55.334948)

                } // End if Polygon 

                if (kaliningradGeometry is NetTopologySuite.Geometries.MultiPolygon multiPoly)
                {
                    foreach (NetTopologySuite.Geometries.Geometry subGeometry in multiPoly.Geometries)
                    {
                        if (subGeometry is NetTopologySuite.Geometries.Polygon poly)
                        {
                            // polygonList.Add(ConvertSinglePolygonToPolygonData(poly));
                        }
                        else
                        {
                            // This case should ideally not happen for well-formed MultiPolygons,
                            // but it's good practice to check if a MultiPolygon contains non-Polygon geometries.
                            System.Console.WriteLine($"Warning: MultiPolygon contains a non-Polygon sub-geometry: {subGeometry.GeometryType}");
                        }
                    } // Next subGeometry 

                } // End if MultiPolygon 

                System.Console.WriteLine($"Number of Polygons: {(kaliningradGeometry is NetTopologySuite.Geometries.MultiPolygon ? ((NetTopologySuite.Geometries.MultiPolygon)kaliningradGeometry).NumGeometries : (kaliningradGeometry is NetTopologySuite.Geometries.Polygon ? 1 : 0))}");



                System.Console.WriteLine($"Geometry Type: {kaliningradGeometry.GeometryType}");
                System.Console.WriteLine($"Number of Polygons: {(kaliningradGeometry is NetTopologySuite.Geometries.MultiPolygon ? ((NetTopologySuite.Geometries.MultiPolygon)kaliningradGeometry).NumGeometries : (kaliningradGeometry is NetTopologySuite.Geometries.Polygon ? 1 : 0))}");
            } // End if (kaliningradGeometry != null) 
            else
            {
                System.Console.WriteLine("Failed to parse geometry from bytes.");
            }
            
        } // End Sub Test 


    } // End Class NaturalEarthBorders 


} // End Namespace 

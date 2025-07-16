
namespace VectorTileSelector
{


    public class SimplePolygonParser
    {
        // Regex to match two numbers with optional whitespace around, supporting scientific notation
        private static readonly System.Text.RegularExpressions.Regex coordinateLineRegex =
            new System.Text.RegularExpressions.Regex(
                @"^\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s+([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)\s*$"
                , System.Text.RegularExpressions.RegexOptions.Compiled
            );

        public static System.Collections.Generic.List<(decimal lat, decimal lon)> 
            ParseCoordinates(string[] lines)
        {
            System.Collections.Generic.List<(decimal lat, decimal lon)> coordinates = 
                new System.Collections.Generic.List<(decimal lat, decimal lon)>();

            System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;

            foreach (string line in lines)
            {
                System.Text.RegularExpressions.Match match = coordinateLineRegex.Match(line);
                if (match.Success)
                {
                    // Parse decimals from the two captured groups
                    decimal lat = decimal.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.Float, culture);
                    decimal lon = decimal.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.Float, culture);

                    coordinates.Add((lat, lon));
                } // End if (match.Success) 
                // else ignore line

            } // Next line 

            return coordinates;
        } // End Function ParseCoordinates 


        static System.Collections.Generic.List<string> FilterFiles(
            string folderPath,
            System.Func<string, bool> filter
        )
        {
            System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string>();

            foreach (string file in System.IO.Directory.EnumerateFiles(folderPath))
            {
                if (filter(file))
                {
                    // yield return file;
                    ls.Add(file);
                }
            } // Next file 

            ls.Sort(System.StringComparer.InvariantCultureIgnoreCase);


            return ls;
        } // End Function FilterFiles 


        public static void SimpleTest()
        {
            string input = @"
polygon 
1
   47.450240   4.1211990E1
   47.555540E0   41.148660
   47.639650   41.173940
  
   46.985370   41.517750
   47.103080   41.507380
   47.240510   41.288040
   47.450240   41.211990
END
2
   -180.000000   72.291600
   -180.000000   72.290530
   -180.000000   62.261340
   -172.074188   64.033556
   -168.725284   65.414875
   -168.684085   65.990095
   -169.907891   72.234092
   -180.000000   72.291600
END
3
   22.227270   54.335930
   22.796200   54.359260
   22.740210   54.447440
   22.709300   54.458910
   22.704760   54.508970
   22.688610   54.532170
END
END
";

            string bbox = GetBoundingBox(input);
            System.Console.WriteLine("Bounding box: {0}", bbox);
        }


        public static string GetFileNameBeforeFirstDot(string filePath)
        {
            string fileName = System.IO.Path.GetFileName(filePath);
            int firstDotIndex = fileName.IndexOf('.');

            if (firstDotIndex >= 0)
            {
                return fileName.Substring(0, firstDotIndex);
            }

            return fileName; // No dot found
        }


        public static void Test()
        {
            string basePath = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileSelector\Results\poly";

            System.Collections.Generic.List<string> pbfFiles = FilterFiles(
                basePath,
                delegate (string file)
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    return fileName.EndsWith(".poly.txt", System.StringComparison.InvariantCultureIgnoreCase);
                }
            );

            foreach (string polygonFile in pbfFiles)
            {
                string regionName = GetFileNameBeforeFirstDot(polygonFile);

                string polygonText = System.IO.File.ReadAllText(polygonFile, System.Text.Encoding.UTF8);
                string bbox = GetBoundingBox(polygonText);
                System.Console.WriteLine("Bounding box {0}: {1}"
                    , System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                        regionName)
                    , bbox
                );

                // string outputFile = System.IO.Path.Combine(basePath, regionName + ".bbox.txt");
                // System.IO.File.WriteAllText(outputFile, bbox, System.Text.Encoding.UTF8);
            } // Next polygonFile 

        } // End Sub Test 


        public static string GetBoundingBox(string input)
        {
            string[] lines = input.Split(new char[] { '\r', '\n' }, 
                System.StringSplitOptions.RemoveEmptyEntries
            );

            System.Collections.Generic.List<(decimal lat, decimal lon)> coords = 
                ParseCoordinates(lines);

            decimal minLat = decimal.MaxValue;
            decimal maxLat = decimal.MinValue;
            decimal minLon = decimal.MaxValue;
            decimal maxLon = decimal.MinValue;

            foreach ((decimal lat, decimal lon) in coords)
            {
                // System.Console.WriteLine($"Lat: {lat}, Lon: {lon}");

                if (lat < minLat) 
                    minLat = lat;

                if (lat > maxLat) 
                    maxLat = lat;

                if (lon < minLon) 
                    minLon = lon;

                if (lon > maxLon) 
                    maxLon = lon;
            } // Next lat, lon 

            // http://bboxfinder.com/#39.627380,18.896480,42.663130,21.062850
            // return string.Format(System.Globalization.CultureInfo.InvariantCulture, $"http://bboxfinder.com/#{minLon},{minLat},{maxLon},{maxLat}");

            // (xMin, yMin, xMax, yMax)
            // (18.89648, 39.62738, 21.06285, 42.66313)
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, $"({minLat}, {minLon}, {maxLat}, {maxLon})");

            // 18.89648, 39.62738, 21.06285, 42.66313
            // return string.Format(System.Globalization.CultureInfo.InvariantCulture, $"{minLat},{minLon},{maxLat},{maxLon}");
        } // End Function GetBoundingBox 


    } // End Class SimplePolygonParser 


} // End Namespace 

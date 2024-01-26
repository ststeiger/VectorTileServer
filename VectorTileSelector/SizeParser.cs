
namespace VectorTileSelector
{


    class SizeParser
    {


        public static void Test()
        {
            string[] inputs = { "(121 TB)", "(12.5 GB)", "(647 MB)", "(5 KB)", "(365 B)" };

            foreach (string input in inputs)
            {
                long bytes = ParseSizeToBytes(input);
                string size = ConvertBytesToSize(bytes);
                System.Console.WriteLine($"{input}: {bytes} bytes aka {size}");
            } // Next input 

        } // End Sub Test 


        // string ver= RemoveSuffix(this.Href, "-latest.osm.pbf");
        public static string RemoveSuffix(string input, string suffix)
        {
            if (input.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))
            {
                return input.Substring(0, input.Length - suffix.Length);
            }
            
            return input;
        }

        public static string ExtractAfterFirstSlash(string input)
        {
            // Find the index of the first occurrence of "/"
            int index = input.IndexOf('/');

            // If "/" is found, return the substring after it
            if (index != -1)
            {
                return input.Substring(index + 1);
            }
            else
            {
                // If "/" is not found, return the original input
                return input;
            }
        }


        public static string ConvertBytesToSize(long bytes)
        {
            if (bytes >= System.Math.Pow(1024, 4))
            {
                return $"{bytes / System.Math.Pow(1024, 4):F2} TB";
            }
            else if (bytes >= System.Math.Pow(1024, 3))
            {
                return $"{bytes / System.Math.Pow(1024, 3):F2} GB";
            }
            else if (bytes >= System.Math.Pow(1024, 2))
            {
                return $"{bytes / System.Math.Pow(1024, 2):F2} MB";
            }
            else if (bytes >= System.Math.Pow(1024, 1))
            {
                return $"{bytes / System.Math.Pow(1024, 1):F2} KB";
            }
            
            return $"{bytes} B";
        } // End Function ConvertBytesToSize 


        public static long ParseSizeToBytes(string input)
        {
            try
            {
                // Split input string into value and unit
                char[] charsToTrim = { '(', ')', ' ', '\t', '\n', '\u00A0' };
                char[] charsWhichSplit = new char[] { ' ', '\u00A0' };

                string[] parts = input
                    .Trim(charsToTrim)
                    .Split(charsWhichSplit, System.StringSplitOptions.RemoveEmptyEntries)
                ;
                

                double value = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                string unit = parts[1];

                // Convert value to bytes based on unit
                switch (unit)
                {
                    case "TB":
                        return (long)(value * System.Math.Pow(1024, 4));
                    case "GB":
                        return (long)(value * System.Math.Pow(1024, 3));
                    case "MB":
                        return (long)(value * System.Math.Pow(1024, 2));
                    case "KB":
                        return (long)(value * System.Math.Pow(1024, 1));
                    case "B":
                        return (long)value;
                    default:
                        throw new System.ArgumentException($"Invalid unit: {unit}");
                } // End Switch 

            } // End Try 
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
            } // End Catch 

            return -1;
        } // End Function ParseSizeToBytes 


    } // End Class SizeParser 


} // End Namespace 

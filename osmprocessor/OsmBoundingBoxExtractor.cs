
namespace osmprocessor
{


    public static class OsmBoundingBoxExtractor
    {


        public static void Test()
        {
            string filePath = @"D:\stefan.steiger\Downloads\swiss_extract.mbtiles\monaco-latest.osm.pbf";
            System.Console.WriteLine("Getting bounding-box for `{0}`.", filePath);
            string? bbox = GetBoundingBox(filePath);
            System.Console.WriteLine("Bounding box: `{0}`.", bbox);
        } // End Sub Test 


        /// <summary>
        /// Uses osmium to read the bbox of a .osm.pbf.file, and if not present, uses osmconvert to compute the bbox of a .osm.pbf file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string? GetBoundingBox(string filePath)
        {
            string? bbox = GetOsmiumBoundingBox(filePath);

            if (!string.IsNullOrEmpty(bbox)) 
                return bbox;
            
            bbox = GetOsmConvertBoundingBox(filePath);
            return bbox;
        } // End Function GetBoundingBox 


        private static void AddToPathEnvironmentVariable(string additionalPath)
        {
            string path = System.Environment.GetEnvironmentVariable("PATH") ?? "";

            if (!string.IsNullOrWhiteSpace(path) && !path.EndsWith(";"))
                path += ";";

            path += additionalPath + ";";
            System.Environment.SetEnvironmentVariable("PATH", path);
        } // End Sub AddToPathEnvironmentVariable 

        
        /// <summary>
        /// Uses osmium to read the bbox of a .osm.pbf.file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>tring filePath)
        public static string? GetOsmiumBoundingBox(string filePath)
        {
            string? retVal = null;

            if (!System.IO.File.Exists(filePath))
            {
                System.Console.Error.WriteLine($"File not found: {filePath}");
                return retVal;
            }

            // Append custom path
            if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                const string additionalPath = @"D:\Programme\LessPortableApps\osm\osmium\";
                AddToPathEnvironmentVariable(additionalPath);
            }
            else
            {
                string? osmiumEnvPath = System.Environment.GetEnvironmentVariable("OSMIUM_PATH");

                if (!string.IsNullOrWhiteSpace(osmiumEnvPath) && System.IO.Directory.Exists(osmiumEnvPath))
                {
                    AddToPathEnvironmentVariable(osmiumEnvPath);
                }
                else
                {
                    System.Console.Error.WriteLine("Environment variable OSMIUM_PATH is not set or points to an invalid directory.");
                }
            }

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "osmium",
                Arguments = $"fileinfo --no-progress -g header.boxes \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (System.Diagnostics.Process process = new System.Diagnostics.Process { StartInfo = startInfo })
            {

                try
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        System.Console.Error.WriteLine("osmium failed:");
                        System.Console.Error.WriteLine(error);
                        return retVal;
                    } // End if (process.ExitCode != 0) 

                    // Expected format: (122.5607,20.08228,154.4709,45.815403)
                    System.Text.RegularExpressions.Match match =
                        System.Text.RegularExpressions.Regex.Match(
                            output, @"\(\s*([^)]+?)\s*\)"
                    );

                    if (!match.Success)
                    {
                        System.Console.Error.WriteLine("Bounding box not found in output.");
                        return retVal;
                    } // End if (!match.Success) 

                    retVal = match.Groups[1].Value.Trim(); // Removes surrounding parentheses
                    // System.Console.WriteLine(retVal);
                }
                catch (System.Exception ex)
                {
                    System.Console.Error.WriteLine($"Error: {ex.Message}");
                }

            } // End Using process 
            return retVal;
        } // End Function GetOsmiumBoundingBox 


        /// <summary>
        /// Uses osmconvert to compute the bbox of a .osm.pbf file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string? GetOsmConvertBoundingBox(string filePath)
        {
            string? retVal = null;

            if (!System.IO.File.Exists(filePath))
            {
                System.Console.Error.WriteLine($"File not found: {filePath}");
                return retVal;
            }

            // Append custom path
            if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                const string additionalPath = @"D:\Programme\LessPortableApps\osm\";
                AddToPathEnvironmentVariable(additionalPath);
            }
            else
            {
                string? osmConvertEnvPath = System.Environment.GetEnvironmentVariable("OSMCONVERT_PATH");

                if (!string.IsNullOrWhiteSpace(osmConvertEnvPath) && System.IO.Directory.Exists(osmConvertEnvPath))
                {
                    AddToPathEnvironmentVariable(osmConvertEnvPath);
                }
                else
                {
                    System.Console.Error.WriteLine("Environment variable OSMCONVERT_PATH is not set or points to an invalid directory.");
                }
            }


            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "osmconvert",
                Arguments = $"\"{filePath}\" --out-statistics",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Inherit and modify environment variables
            // Note: unnecessary, as per default, it copies them.
            //       Also, note that additional path variables only affect the program once it is running 
            //       AND NOT when it is being started ...
            // foreach (System.Collections.DictionaryEntry env in System.Environment.GetEnvironmentVariables())
            // {
            //     startInfo.EnvironmentVariables[env.Key!.ToString()!] = env.Value!.ToString()!;
            // }

            // string path = startInfo.EnvironmentVariables["PATH"];
            // System.Console.WriteLine(path);


            using (System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = startInfo
            })
            {

                try
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        System.Console.Error.WriteLine("osmconvert failed:");
                        System.Console.Error.WriteLine(error);
                        return null;
                    } // End if (process.ExitCode != 0) 

                    double lonMin = ExtractValue(output, "lon min");
                    double lonMax = ExtractValue(output, "lon max");
                    double latMin = ExtractValue(output, "lat min");
                    double latMax = ExtractValue(output, "lat max");

                    retVal = System.FormattableString.Invariant($"{lonMin},{latMin},{lonMax},{latMax}");
                }
                catch (System.Exception ex)
                {
                    System.Console.Error.WriteLine($"Error: {ex.Message}");
                    retVal = null;
                }

                // System.Console.WriteLine(retVal);
                return retVal;
            } // End Using process 

        } // End Function GetOsmConvertBoundingBox


        private static double ExtractValue(string text, string key)
        {
            System.Text.RegularExpressions.Match match =
                System.Text.RegularExpressions.Regex.Match(
                    text,
                    @$"{System.Text.RegularExpressions.Regex.Escape(key)}:\s*([+-]?\d+(\.\d+)?)"
            );

            if (!match.Success || !double.TryParse(
                    match.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double result
                )
            )
            {
                throw new System.InvalidOperationException($"Could not extract '{key}' from output.");
            }

            return result;
        } // End Function ExtractValue 


    } // End Class 


} // End Namespace 

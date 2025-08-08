
namespace osmprocessor
{


    public static class PlanetilerRunner
    {

        // https://download.openstreetmap.fr/
        // https://download.openstreetmap.fr/extracts/
        // https://download.geofabrik.de/

        public static async System.Threading.Tasks.Task Test()
        {
            string osmPbfPath = @"D:\stefan.steiger\Programme\LessPortableApps\osm\planetiler\downloaded\montenegro-latest.osm.pbf";
            string outputMbtilesPath = @"D:\stefan.steiger\Programme\LessPortableApps\osm\planetiler\downloaded\montenegro.mbtiles";

            string bounds = OsmBoundingBoxExtractor.GetBoundingBox(osmPbfPath)!;
            System.Console.WriteLine(bounds);

            await GenerateMbtilesAsync(osmPbfPath, outputMbtilesPath, bounds);
        }


        private static void AddToPathEnvironmentVariable(string additionalPath)
        {
            string path = System.Environment.GetEnvironmentVariable("PATH") ?? "";

            if (!string.IsNullOrWhiteSpace(path) && !path.EndsWith(";"))
                path += ";";

            path += additionalPath + ";";
            System.Environment.SetEnvironmentVariable("PATH", path);
        } // End Sub AddToPathEnvironmentVariable 


        public static async System.Threading.Tasks.Task<int> GenerateMbtilesAsync(
            string osmPbfPath,
            string outputMbtilesPath,
            string bounds,
            int heapSizeInGB
        )
        {
            int exitCode = -1;
            string jarPath = "planetiler-openmaptiles.jar";

            if (!System.IO.File.Exists(osmPbfPath))
                throw new System.IO.FileNotFoundException("Input .osm.pbf file not found", osmPbfPath);


            // Append custom path
            if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                const string additionalPath = @"D:\Programme\LessPortableApps\osm\planetiler\";
                AddToPathEnvironmentVariable(additionalPath);
            }
            else
            {
                string? planeTilerEnvPath = System.Environment.GetEnvironmentVariable("PLANETILER_PATH");

                if (!string.IsNullOrWhiteSpace(planeTilerEnvPath) && System.IO.Directory.Exists(planeTilerEnvPath))
                {
                    AddToPathEnvironmentVariable(planeTilerEnvPath);
                }
                else
                {
                    System.Console.Error.WriteLine("Environment variable OSMIUM_PATH is not set or points to an invalid directory.");
                }
            }

            string backupCurDir = System.IO.Directory.GetCurrentDirectory();
            jarPath = JarFinder.FindJarInPath("planetiler-openmaptiles.jar")!;

            if (!System.IO.File.Exists(jarPath))
                throw new System.IO.FileNotFoundException("planetiler JAR file not found", jarPath);

            string jarDir = System.IO.Path.GetDirectoryName(jarPath)!;

            try
            {
                System.Environment.SetEnvironmentVariable("MIN_ZOOM", "0");
                System.Environment.SetEnvironmentVariable("MAX_ZOOM", "14");
                // planetiler won't find the data, if it's not executed in the current working directory. 
                System.IO.Directory.SetCurrentDirectory(jarDir);


                string javaHeapSize = $"-Xmx{heapSizeInGB}g";

                string arguments = string.Join(" ",
                    javaHeapSize,
                    "-jar",
                    Quote(jarPath),
                    $"--osm-path={Quote(osmPbfPath)}",
                    $"--output={Quote(outputMbtilesPath)}",
                    $"--bounds={bounds}",
                    "--download=true"
                );

                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using System.Diagnostics.Process process = new System.Diagnostics.Process()
                {
                    StartInfo = processStartInfo
                };

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        System.Console.WriteLine("[stdout] " + args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        System.Console.Error.WriteLine("[stderr] " + args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();
                exitCode = process.ExitCode;
            }
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine(ex.Message);
                System.Console.Error.WriteLine(ex.StackTrace);
                throw;
            }
            finally
            {
                System.IO.Directory.SetCurrentDirectory(backupCurDir);
            }

            return exitCode;
        } // End Task GenerateMbtilesAsync 


        public static async System.Threading.Tasks.Task<int> GenerateMbtilesAsync(
            string osmPbfPath,
            string outputMbtilesPath,
            string bounds
        )
        {
            return await GenerateMbtilesAsync(osmPbfPath, outputMbtilesPath, bounds, 1);
        } // End Task GenerateMbtilesAsync 


        private static string Quote(string path)
        {
            return $"\"{path}\"";
        } // End Function Quote 


    } // End Class PlanetilerRunner 


} // End Namespace 


namespace VectorTileSelector
{

    
    using System.Linq;

    public class OsmProcessor
    {
        public static void Test()
        {
            // Array of *.osm.pbf files
            string[] pbfFiles = new[]
            {
            "switzerland-latest.osm.pbf",
            "liechtenstein-latest.osm.pbf",
            "vorarlberg-latest.osm.pbf",
            "haute_savoie-latest.osm.pbf",
            "savoie.osm.pbf",
            "alsace-latest.osm.pbf",
            "tuebingen-regbez-latest.osm.pbf",
            "schwaben-latest.osm.pbf",
            "freiburg-regbez-latest.osm.pbf"
        };

            string[] o5mFiles = pbfFiles.Select(f => System.IO.Path.GetFileNameWithoutExtension(f) + ".o5m").ToArray();

            // Step 1: Convert each .osm.pbf to .o5m
            foreach ((string input, string output) in pbfFiles.Zip(o5mFiles))
            {
                RunCommand("osmconvert", $"{input} --out-o5m -o={output}");
            }

            // Step 2: Merge all .o5m files into one big .o5m
            string mergedO5m = "swiss_big.o5m";
            RunCommand("osmconvert", $"{string.Join(" ", o5mFiles)} -o={mergedO5m}");

            // Step 3: Convert merged .o5m to .osm.pbf
            string mergedPbf = "swiss_big.osm.pbf";
            RunCommand("osmconvert", $"{mergedO5m} --out-pbf -o={mergedPbf}");

            // Step 4: Extract bounding box
            string boundingBoxRaw = RunCommand("osmium", $"fileinfo --no-progress -g header.boxes {mergedPbf}", captureOutput: true)
                ?.Trim();

            if (boundingBoxRaw != null && boundingBoxRaw.StartsWith("(") && boundingBoxRaw.EndsWith(")"))
            {
                string bbox = boundingBoxRaw[1..^1]; // Remove parentheses
                System.Console.WriteLine($"Bounding box: {bbox}");
            }
            else
            {
                System.Console.WriteLine("Failed to extract bounding box.");
            }
        }

        private static string? RunCommand(string command, string arguments, bool captureOutput = false)
        {
            using System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = captureOutput,
                RedirectStandardError = !captureOutput,
                UseShellExecute = false,
                CreateNoWindow = true
            };



            // Copy current environment variables
            foreach (System.Collections.DictionaryEntry kvp in System.Environment.GetEnvironmentVariables())
            {
                string key = (string)kvp.Key;
                string value = (string)kvp.Value;
                process.StartInfo.Environment[key] = value;
            }



            // Always set these environment variables
            process.StartInfo.EnvironmentVariables["MIN_ZOOM"] = "0";
            process.StartInfo.EnvironmentVariables["MAX_ZOOM"] = "14";

            process.Start();

            string? errorOutput = null;


            if (captureOutput)
            {
                string output = process.StandardOutput.ReadToEnd();
                errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return output;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                if (string.IsNullOrEmpty(errorOutput))
                    errorOutput = process.ExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture);

               System.Console.Error.WriteLine($"ERROR ({command} {arguments}): {errorOutput}");
            }

            return null;
        }
    }

}
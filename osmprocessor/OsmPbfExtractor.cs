
namespace osmprocessor
{

    
    public static class OsmPbfExtractor
    {


        /// <summary>
        /// Uses osmium extract, to create a bbox-extract from a .osm.pbf file
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="bbox"></param>
        /// <param name="outputFile"></param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static void ExtractBoundingBox(
            string inputFile,
            string bbox,
            string outputFile 
        )
        {
            // Validate input file
            if (!System.IO.File.Exists(inputFile))
                throw new System.IO.FileNotFoundException($"Input file does not exist: {inputFile}");

            string quotedInput = $"\"{inputFile}\"";
            string quotedOutput = $"\"{outputFile}\"";

            string args = $"extract --bbox {bbox} --set-bounds {quotedInput} -o {quotedOutput}";

            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "osmium";
                process.StartInfo.Arguments = args;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                try
                {
                    process.Start();

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                        throw new System.Exception($"Osmium extract failed with exit code {process.ExitCode}:{System.Environment.NewLine}{stderr}");

                    System.Console.WriteLine(stdout);
                } // End Try 
                catch (System.Exception ex)
                {
                    System.Console.Error.WriteLine($"Error during extract: {ex.Message}");
                    throw;
                } // End Catch 

            } // End Using process 

        } // End Sub ExtractBoundingBox 


        public static void ExtractBoundingBox(
            string inputFile,
            string outputFile,
            double left,
            double bottom,
            double right,
            double top
        )
        {

            // Validate bounding box
            if (left >= right || bottom >= top)
                throw new System.ArgumentException("Invalid bounding box coordinates.");

            // Format coordinates using invariant culture
            string bbox = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0},{1},{2},{3}",
                left,
                bottom,
                right,
                top);

            ExtractBoundingBox(inputFile, bbox, outputFile);
        } // End Sub ExtractBoundingBox 

        
        public static void Test()
        {
            try
            {
                string inputFile = @"C:\osm\merged-europe.osm.pbf";
                string outputFile = @"C:\osm\extracted-central-europe.osm.pbf";

                // Example bbox: Central Europe approx.
                double left = 5.0;
                double bottom = 45.0;
                double right = 15.0;
                double top = 50.0;

                ExtractBoundingBox(inputFile, outputFile, left, bottom, right, top);

                System.Console.WriteLine("Extraction completed successfully.");
            } // End Try 
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine("Fatal error: " + ex.Message);
            } // End Catch 

        } // End Sub Test 


    } // End Class OsmPbfExtractor 


} // End Namespace 

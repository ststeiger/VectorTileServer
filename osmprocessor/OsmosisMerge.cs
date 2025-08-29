
namespace osmprocessor
{


    internal class OsmosisMerge
    {


        static void DisplayTotalSize(System.Collections.Generic.List<string> files)
        {
            long totalBytes = 0;

            foreach (string file in files)
            {
                totalBytes += new System.IO.FileInfo(file).Length;
            }

            double totalMB = totalBytes / (1024.0 * 1024.0);

            System.Globalization.NumberFormatInfo nfi = (System.Globalization.NumberFormatInfo)
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();

            nfi.NumberGroupSeparator = "'";
            nfi.NumberDecimalDigits = 3;

            System.Console.WriteLine("Total Size: {0} MB", totalMB.ToString("N", nfi));
        } // End Sub DisplayTotalSize 


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


        public static async System.Threading.Tasks.Task<int> Test(string folderPath)
        {
            // https://github.com/openstreetmap/osmosis
            // https://github.com/onthegomap/planetiler/releases
            // https://github.com/openmaptiles/planetiler-openmaptiles/releases

            string osmosisBinPath = @"D:\Programme\LessPortableApps\osm\osmosis-0.49.2\bin";
            string outputFile = System.IO.Path.Combine(folderPath, "merged.osm.pbf");

            if (!System.IO.Directory.Exists(folderPath))
            {
                await System.Console.Out.WriteLineAsync($"Folder does not exist: {folderPath}");
                return 1;
            }

            if (System.IO.File.Exists(outputFile))
            {
                System.IO.File.Delete(outputFile);
                System.Console.WriteLine($"Deleted existing merged file: {outputFile}");
            }

            System.Collections.Generic.List<string> pbfFiles = FilterFiles(
                folderPath,
                delegate(string file)
                {
                    string fileName = System.IO.Path.GetFileName(file);

                    bool isPbf = fileName.EndsWith(".osm.pbf", System.StringComparison.InvariantCultureIgnoreCase);
                    if (!isPbf)
                        return false;

                    if ("merged.osm.pbf".Equals(fileName, System.StringComparison.InvariantCultureIgnoreCase))
                        return false;

                    return true;
                }
            );
            DisplayTotalSize(pbfFiles);


            if (pbfFiles.Count == 0)
            {
                await System.Console.Out.WriteLineAsync("No .osm.pbf files found in folder.");
                return 1;
            }


            await System.Console.Out.WriteLineAsync($"Found {pbfFiles.Count} .osm.pbf files. Preparing Osmosis command...");

            

            // Build Osmosis command arguments
            // Example for 3 files:
            // osmosis ^
            //  --read-pbf file1.osm.pbf ^
            //  --read-pbf file2.osm.pbf ^
            //  --merge ^
            //  --read-pbf file3.osm.pbf ^
            //  --merge ^
            //  --write-pbf merged.osm.pbf

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("osmosis ");

            // Add the first two --read-pbf and one --merge (if only one file, skip merge)
            if (pbfFiles.Count == 1)
            {
                // Just read and write directly, no merge needed
                sb.Clear();
                sb.AppendFormat("osmosis --read-pbf \"{0}\" --write-pbf \"{1}\"", pbfFiles[0], outputFile);
            }
            else
            {
                sb.AppendFormat("--read-pbf \"{0}\" ", pbfFiles[0]);
                sb.AppendFormat("--read-pbf \"{0}\" ", pbfFiles[1]);
                sb.Append("--merge ");

                // For remaining files, add read-pbf + merge
                for (int i = 2; i < pbfFiles.Count; i++)
                {
                    sb.AppendFormat("--read-pbf \"{0}\" ", pbfFiles[i]);
                    sb.Append("--merge ");
                }

                sb.AppendFormat("--write-pbf \"{0}\"", outputFile);
            }

            string arguments = sb.ToString();

            await System.Console.Out.WriteLineAsync("Running command:");
            await System.Console.Out.WriteLineAsync(arguments);

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C " + arguments,
                WorkingDirectory= folderPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Add Osmosis bin folder to PATH for this process only
            

            string? existingPath = psi.EnvironmentVariables["PATH"];
            if (!string.IsNullOrEmpty(existingPath))
                psi.EnvironmentVariables["PATH"] = osmosisBinPath + ";" + existingPath;
            else
                psi.EnvironmentVariables["PATH"] = osmosisBinPath;


            using (System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = psi
            })
            {

                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        System.Console.Out.WriteLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        System.Console.Out.WriteLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                await System.Console.Out.WriteLineAsync($"Process exited with code {process.ExitCode}");

                if (process.ExitCode == 0)
                {
                    await System.Console.Out.WriteLineAsync($"Merged file created at: {outputFile}");
                    return 0;
                }
                else
                {
                    await System.Console.Out.WriteLineAsync("Osmosis process failed.");
                    return process.ExitCode;
                }
            } // End Using proces 

        } // End Sub Test 


    } // End Class OsmosisMerge 


} // End Namespace 


using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace osmprocessor
{


    public static class OsmPbfMerger
    {


        /// <summary>
        /// Uses osmium to merge multiple .osm.pbf files
        /// </summary>
        /// <param name="outputFilePath"></param>
        /// <param name="inputFiles"></param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static void MergeOsmPbfFiles(
            string outputFilePath,
            params string[] inputFiles
        )
        {
            // Validate inputs
            if (inputFiles == null || inputFiles.Length == 0)
                throw new System.ArgumentException("No input files provided.", nameof(inputFiles));
            
            System.Collections.Generic.List<string> validInputFiles = 
                new System.Collections.Generic.List<string>();

            foreach (string file in inputFiles)
            {
                if (!System.IO.File.Exists(file))
                    throw new System.IO.FileNotFoundException($"Input file not found: {file}");

                validInputFiles.Add($"\"{file}\""); // quote each file path
            } // Next file 

            string quotedOutputFile = $"\"{outputFilePath}\"";

            string args = $"merge {string.Join(" ", validInputFiles)} -o {quotedOutputFile}";

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
                        throw new System.Exception($"Osmium merge failed with exit code {process.ExitCode}:{System.Environment.NewLine}{stderr}");

                    System.Console.WriteLine(stdout);
                } // End Try 
                catch (System.Exception ex)
                {
                    System.Console.Error.WriteLine($"Error during merge: {ex.Message}");
                    throw;
                } // End Catch 

            } // End Using process 

        } // End Sub MergeOsmPbfFiles 


        public static void Test2()
        {
            System.Collections.Generic.List<string> lsMaghreb = new System.Collections.Generic.List<string>()
            {
                "Mauritania"
                ,"Morocco"
                ,"Algeria"
                ,"Tunisia"
                // Libya and Egypt are often considered part of North Africa but not specifically the Maghreb. 
                ,"Libya"
                // Including Egypt is defensible — while it's more Mashriq than Maghreb,
                // it's often included in North Africa.
                ,"Egypt"
                // optional: "Western Sahara"
            };

            System.Collections.Generic.List<string> lsNorthAfrica = new System.Collections.Generic.List<string>()
            {
                 "Sudan"
                // ,"south-sudan" // considered East Africa,
                // Somalia and Djibouti are more accurately in the Horn of Africa, not North Africa
                // Optional: ,"Malta" ,"Cyprus" // not NA, is Europe 
            };
            lsNorthAfrica.AddRange(lsMaghreb);

            System.Collections.Generic.List<string> lsHornOfAfrica = new System.Collections.Generic.List<string>()
            { 
                "Somalia", "Djibouti", "Eritrea", "Ethiopia" 
            };



            System.Collections.Generic.List<string> lsCentralAsia = new System.Collections.Generic.List<string>()
            {
                 "Kazakhstan"
                ,"Kyrgyzstan"
                ,"Tajikistan"
                ,"Turkmenistan"
                ,"Uzbekistan"
            };


            // Modern countries typically considered part of Persia’s historical extent:
            System.Collections.Generic.List<string> lsPersianSphere = new System.Collections.Generic.List<string>()
            {
                 "Iran" // (the heartland and core of Persia)
                ,"Iraq" // (especially parts that were under Persian control at various times)
                ,"Afghanistan" // (eastern parts, culturally and historically influenced by Persian empires)
                ,"Turkmenistan" // (southern regions)
                ,"Azerbaijan" // (especially the region of Iranian Azerbaijan in the northwest)
                ,"Armenia" // (parts historically under Persian rule)
                ,"Georgia" // (parts, historically intermittently controlled)
                ,"Pakistan" // (western regions, mainly Baluchistan and parts of Sindh under Persian cultural influence)
                ,"Syria" // (at times under Achaemenid and later Persian empires)
                ,"Turkey" // (eastern Anatolia, historically Persian - controlled or influenced)
                // ,"Tajikistan" // is part of Central Asia
                ,"Tajikistan" // was part of the Persian cultural and linguistic sphere for centuries,
                              // especially under the Samanid Empire (9th–10th century),
                              // which is considered a key period in the development
                              // of Persian language and culture
            };


            // greatest extent of the Ilkhanate:
            System.Collections.Generic.List<string> lsIlkhanate = new System.Collections.Generic.List<string>()
            {
                 "Turkey" // (mostly the eastern part)
                ,"Azerbaijan"
                ,"Iran" // (core territory)
                
                ,"Iraq"
                ,"Afghanistan"
                ,"Pakistan" // ("Parts of: northwestern regions, e.g., Baluchistan, but mostly contested)

                ,"Armenia"
                ,"Georgia"

                ,"Syria"
                ,"Lebanon" // (likely nominally under Ilkhanate influence)
                ,"Israel / Palestine" // (nominally under Ilkhanate suzerainty, often contested)
                ,"Kuwait" // (coastal areas, nominally)

                ,"Turkmenistan" // (southern parts)
                ,"Uzbekistan"  // (southern parts)
                ,"Tajikistan" // (southern parts)
            };


            System.Collections.Generic.List<string> lsSouthCaucasus = new System.Collections.Generic.List<string>()
            {
                 "Armenia"
                ,"Georgia"
                ,"Azerbaijan"
            };


            System.Collections.Generic.List<string> lsGreaterMiddleEast = new System.Collections.Generic.List<string>()
            {
                 "Afghanistan"
                ,"Pakistan"
                ,"Turkey"
                ,"Iran"
            };
            lsGreaterMiddleEast.AddRange(lsSouthCaucasus);
            


            System.Collections.Generic.List<string> lsGCC = new System.Collections.Generic.List<string>()
            {
                 "Bahrain"
                ,"Kuwait"
                ,"Oman"
                ,"Qatar"
                ,"Saudi Arabia"
                ,"United Arab Emirates"
            };

            System.Collections.Generic.List<string> lsArabianPeninsula = new System.Collections.Generic.List<string>(lsGCC);
            lsArabianPeninsula.Add("Yemen");

            System.Collections.Generic.List<string> lsLevant = new System.Collections.Generic.List<string>()
            {
                 "Israel"
                ,"Jordan"
                ,"Lebanon"
                ,"Palestine"
                ,"Syria"
            };


            System.Collections.Generic.List<string> lsArabia = new System.Collections.Generic.List<string>(lsArabianPeninsula);
            lsArabia.AddRange(lsLevant);

            
            // Standard-MENA: Middle East and North Africa (MENA)
            System.Collections.Generic.List<string> lsMENA = new System.Collections.Generic.List<string>(lsArabia);
            lsMENA.AddRange(lsMaghreb);
            // Including "Iran" and "Iraq" in MENA is debatable.
            // Many definitions include Iraq, but exclude Iran.
            lsMENA.Add("Iraq");
            // Turkey, Afghanistan and Pakistan are typically not included in Standard-MENA.


            // Non-Standard-Mena: Turkey, Iran, Afghanistan and Pakistan are typically not included in MENA.
            System.Collections.Generic.List<string> lsExtendedMENA = new System.Collections.Generic.List<string>(lsMENA);
            lsExtendedMENA.AddRange(lsGreaterMiddleEast);
            

            // https://www.imf.org/en/Publications/WEO/weo-database/2023/April/groups-and-aggregates
            // IMF's Middle East and Central Asia (ME&CA)
            System.Collections.Generic.List<string> lsMECA = new System.Collections.Generic.List<string>(lsSouthCaucasus);
            lsMECA.AddRange(lsExtendedMENA);
            lsMECA.AddRange(lsCentralAsia);
        }




        /*
        Opt: Mauritania
        Morocco
        Algeria
        Tunisia
        Opt: Malta
        Opt: Cyprus
        Libya
        Egypt

        Opt: Sudan 
        Opt: Somalia
        Opt: Djibouti

        Palestine(Occupied Palestinian Territory / State of Palestine)
        Israel
        Lebanon
        Jordan
        Syria
        Iraq

        Iran

        // Opt: Turkey
        // Opt: Azerbaijan
        // Opt: Afghanistan 
        // Opt: Pakistan
        // Opt: Tajikistan (according to IMF is Central Asia)

        Saudi Arabia
        United Arab Emirates
        Qatar
        Oman
        Bahrain
        Kuwait

        Yemen
        */



        public static void Test()
        {
            try
            {
                string[] inputFiles = new string[]
                {
                    @"C:\osm\switzerland-latest.osm.pbf",
                    @"C:\osm\austria-latest.osm.pbf"
                };


                string outputFile = @"C:\osm\merged-europe.osm.pbf";

                MergeOsmPbfFiles(outputFile, inputFiles);

                System.Console.WriteLine("Merge completed successfully.");
            } // End Try 
            catch (System.Exception ex)
            {
                System.Console.Error.WriteLine("Fatal error: " + ex.Message);
            } // End Catch 

        } // End Sub Test 


    } // End Class OsmPbfMerger 


} // End Namespace 


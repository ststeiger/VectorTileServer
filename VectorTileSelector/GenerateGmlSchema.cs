
namespace VectorTileSelector
{


    // https://schemas.opengis.net/citygml/
    class GmlSchemaGenerator
    {

        private static bool IsNumber(string part)
        {
            return double.TryParse(part,
                System.Globalization.NumberStyles.Float |
                System.Globalization.NumberStyles.AllowThousands,
                System.Globalization.CultureInfo.InvariantCulture, out _);
        }

        public static void Test()
        {
            string username = System.Environment.UserName;
            string sourceDir = $@"D:\{username}\Downloads\citygml-2_0_0\citygml";
            string outputDir = $@"D:\{username}\Downloads\citygml-2_0_0\build";

            if (false)
            {
                string[] xsdFiles = System.IO.Directory.GetFiles(sourceDir, "*.xsd", System.IO.SearchOption.AllDirectories);

                // ml>xscgen building/2.0/building.xsd --namespace CityGMLSharp.Citygml.Building.2_0 --output "D:\stefan.steiger\Downloads\citygml-2_0_0\build"
                foreach (string xsdFile in xsdFiles)
                {
                    string relativePath = System.IO.Path.GetRelativePath(sourceDir, xsdFile);
                    string[] parts = relativePath.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
                    string moduleName = System.IO.Path.GetFileNameWithoutExtension(xsdFile);

                    bool doGen = true;

                    string safeNamespace = "CityGMLSharp.V2_0";
                    foreach (string part in parts)
                    {
                        if (IsNumber(part))
                            continue;

                        if ("examples".Equals(part, System.StringComparison.InvariantCultureIgnoreCase))
                            doGen = false;

                        safeNamespace += "." + ToValidCSharpIdentifier(part);
                    }

                    System.Console.WriteLine($"Generating for {relativePath}...");

                    if(doGen)
                        RunXscgen(xsdFile, safeNamespace, outputDir);
                }
            }
            else
                FileRenamer.RenameFilesInFolder(outputDir);

            System.Console.WriteLine("✅ All done.");
        }


        // dotnet tool install --global dotnet-xscgen
        static void RunXscgen(string xsdFilePath, string targetNamespace, string outputDir)
        {
            System.Diagnostics.ProcessStartInfo psi =
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xscgen",
                    Arguments = $"\"{xsdFilePath}\" --namespace {targetNamespace} --output \"{outputDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

            using System.Diagnostics.Process process = System.Diagnostics.Process.Start(psi);
            if (process == null) return;

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
                System.Console.WriteLine($"✔ Success: {System.IO.Path.GetFileName(xsdFilePath)}");
            else
                System.Console.WriteLine($"❌ Error ({System.IO.Path.GetFileName(xsdFilePath)}):\n{stderr}");
        }

        static string ToValidCSharpIdentifier(string input)
        {
            if (input.EndsWith(".xsd"))
            {
                input = System.IO.Path.GetFileNameWithoutExtension(input);
            }

            input = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input);

            string cleaned = input.Replace("-", "_").Replace(" ", "_");
            if (char.IsDigit(cleaned[0]))
                cleaned = "V" + cleaned;

            cleaned = cleaned
                .Replace("gml", "Gml")
                .Replace("base", "Base")
                .Replace("furniture", "Furniture")
                .Replace("object", "Object")
                .Replace("group", "Group")
                .Replace("surface", "Surface")
                .Replace("tron", "Tron")
            ;


            return cleaned;
        }
    }
}

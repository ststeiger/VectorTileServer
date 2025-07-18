
namespace VectorTileSelector
{

    public static class FileRenamer
    {
        public static void RenameFilesInFolder(string folderPath)
        {
            string[] files = System.IO.Directory.GetFiles(folderPath, "*.cs");

            foreach (string filePath in files)
            {
                string fileName = System.IO.Path.GetFileName(filePath);

                // Find positions of dots in filename
                int lastDotIndex = fileName.LastIndexOf('.');
                if (lastDotIndex <= 0) continue; // no dots or hidden file

                int secondLastDotIndex = fileName.LastIndexOf('.', lastDotIndex - 1);

                // If there's no second last dot, no need to rename
                if (secondLastDotIndex < 0) continue;

                // Extract the part after the second last dot
                string newFileName = fileName.Substring(secondLastDotIndex + 1);

                string newFilePath = System.IO.Path.Combine(folderPath, newFileName);

                // Skip if file with newFileName already exists
                if (System.IO.File.Exists(newFilePath))
                {
                    System.Console.WriteLine($"Skipped rename of {fileName} because {newFileName} already exists.");
                    continue;
                }

                System.Console.WriteLine($"Renaming {fileName} to {newFileName}");
                System.IO.File.Move(filePath, newFilePath);
            }
        }
    }


}


namespace osmprocessor
{
    

    public static class JarFinder
    {


        public static string? FindJarInPath(string jarFileName)
        {
            if (string.IsNullOrWhiteSpace(jarFileName))
                throw new System.ArgumentException("Jar file name must not be empty.", nameof(jarFileName));

            string? pathEnv = System.Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrWhiteSpace(pathEnv))
                return null;

            foreach (string dir in ParsePathEnv(pathEnv))
            {
                try
                {
                    string fullPath = System.IO.Path.Combine(dir, jarFileName);
                    if (System.IO.File.Exists(fullPath))
                        return fullPath;
                }
                catch
                {
                    // Skip invalid or inaccessible directories
                }
            } // Next dir 

            string curDir = System.IO.Directory.GetCurrentDirectory();
            curDir = System.IO.Path.Combine(curDir, jarFileName);
            if (System.IO.File.Exists(curDir))
                return curDir;

            return null;
        } // End Function FindJarInPath 


        /// <summary>
        /// Parses the PATH variable, handling quoted entries with embedded semicolons or colons.
        /// </summary>
        private static System.Collections.Generic.IEnumerable<string> ParsePathEnv(string pathEnv)
        {
            System.Collections.Generic.List<string> paths = new System.Collections.Generic.List<string>();
            System.Text.StringBuilder current = new System.Text.StringBuilder();
            bool inQuotes = false;

            foreach (char c in pathEnv)
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes && c == System.IO.Path.PathSeparator)
                {
                    string entry = current.ToString().Trim();
                    if (entry.Length > 0)
                        paths.Add(entry);
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            } // Next c 

            string last = current.ToString().Trim();
            if (last.Length > 0)
                paths.Add(last);

            return paths;
        } // End Function ParsePathEnv 


    } // End Class JarFinder 


} // End Namespace 


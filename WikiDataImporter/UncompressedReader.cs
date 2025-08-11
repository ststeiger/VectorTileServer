
namespace WikiDataImporter
{


    internal class UncompressedReader 
    {


        public static void Test()
        {
 
            // Open the compressed file
            using (System.IO.FileStream fs = 
                new System.IO.FileStream("latest-truthy.nt.bz2", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // Wrap the file stream in a BZip2 decompressor stream
                using (ICSharpCode.SharpZipLib.BZip2.BZip2InputStream bz2Stream =
                    new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(fs))
                {

                    using (System.IO.StreamReader reader = new System.IO.StreamReader(bz2Stream))
                    {

                        // Use a dotNetRDF parser to read the N-Triples from the stream
                        VDS.RDF.Parsing.NTriplesParser parser = new VDS.RDF.Parsing.NTriplesParser();
                        using (VDS.RDF.Graph g = new VDS.RDF.Graph())
                        {
                            parser.Load(g, reader);
                        } // End Using g 

                        // The triples are now loaded into the graph object
                        // without ever writing the uncompressed file to disk.
                        System.Console.WriteLine($"Loaded {g.Triples.Count} triples.");
                    } // End Using bz2Stream 

                } // End Using reader 

            } // End Using fs 

        } // End Sub Test 


    } // End Class 


} // End Namespace 

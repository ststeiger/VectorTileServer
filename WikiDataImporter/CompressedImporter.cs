
namespace WikiDataImporter
{


    class CompressedImporter 
    {


        public static async System.Threading.Tasks.Task Test()
        {
            string bz2Path = @"path\to\latest-truthy.nt.bz2";

            using System.IO.FileStream fileStream = new System.IO.FileStream(
                bz2Path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

            using SharpCompress.Compressors.BZip2.BZip2Stream bz2Stream = new SharpCompress.Compressors.BZip2.BZip2Stream(
                fileStream,
                SharpCompress.Compressors.CompressionMode.Decompress, 
                false
            );

            using System.IO.StreamReader streamReader = new System.IO.StreamReader(bz2Stream);

            // Initialize a Graph to hold the triples
            using (VDS.RDF.Graph graph = new VDS.RDF.Graph())
            {

                // dotNetRDF parser
                VDS.RDF.Parsing.NTriplesParser parser = new VDS.RDF.Parsing.NTriplesParser(VDS.RDF.Parsing.NTriplesSyntax.Rdf11);

                // Load the triples from the stream into the graph
                parser.Load(graph, streamReader);

                // Now iterate through the graph to access each triple
                foreach (VDS.RDF.Triple? triple in graph.Triples)
                {
                    System.Console.WriteLine($"{triple.Subject} {triple.Predicate} {triple.Object}");
                } // Next triple 

            } // End using graph 

        } // End Task Test 


    } // End Class CompressedImporter 


} // End Namespace 

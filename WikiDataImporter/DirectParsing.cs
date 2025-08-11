
namespace WikiDataImporter
{


    internal class DirectImporter
    {


        public static void Test()
        {

            // Create a new graph to store the triples
            using (VDS.RDF.IGraph g = new VDS.RDF.Graph())
            {

                // Create an NTriplesParser instance
                VDS.RDF.Parsing.NTriplesParser parser = new VDS.RDF.Parsing.NTriplesParser();

                // Load the file into the graph
                try
                {
                    parser.Load(g, "latest-truthy.nt");

                    // You can now iterate through the triples in the graph
                    foreach (VDS.RDF.Triple t in g.Triples)
                    {
                        System.Console.WriteLine(t.ToString());
                    } // Next t 

                } // End Try 
                catch (VDS.RDF.Parsing.RdfParseException parseEx)
                {
                    // Handle parsing errors
                    System.Console.WriteLine("Parser Error: " + parseEx.Message);
                } // End Catch 

            } // End Using graph 

        } // End Sub Test 


    } // End Class DirectImporter 


} // End Namespace 

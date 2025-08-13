
namespace Mapbox.VectorTile
{


    public class ExampleUsage 
    {


        public static void Test(
            byte[] tileBytes,
            System.Func<
                System.Collections.Generic.List<
                    System.Collections.Generic.List<Mapbox.VectorTile.Geometry.Point2d<float>>
                >
                , string
            > serializeCallback
        )
        {

            // Create a VectorTile object
            Mapbox.VectorTile.VectorTile vectorTile = new Mapbox.VectorTile.VectorTile(tileBytes);

            // Iterate over all layers in the tile
            foreach (string layerName in vectorTile.LayerNames())
            {
                Mapbox.VectorTile.VectorTileLayer layer = vectorTile.GetLayer(layerName);

                System.Console.WriteLine($"Layer: {layerName}, Features: {layer.FeatureCount()}");

                for (int i = 0; i < layer.FeatureCount(); i++)
                {
                    Mapbox.VectorTile.VectorTileFeature feature = layer.GetFeature(i);

                    // Access properties

                    System.Collections.Generic.Dictionary<string, object> properties = feature.GetProperties();
                    foreach (string key in properties.Keys)
                    {
                        System.Console.WriteLine($"{key}: {properties[key]}");
                    }

                    // Access geometry (as Mapbox.VectorTile.Geometry.GeomType)
                    Mapbox.VectorTile.Geometry.GeomType geomType = feature.GeometryType;
                    System.Console.WriteLine("geomType: {0}", geomType);


                    System.Collections.Generic.List<
                        System.Collections.Generic.List<
                            Mapbox.VectorTile.Geometry.Point2d<float>
                            >
                        > geometry = feature.Geometry<float>();

                    // System.Console.WriteLine(geometrygeometry

                    // Serialize to JSON
                    if (serializeCallback != null)
                    { 
                        string json = serializeCallback(geometry);
                        System.Console.WriteLine(json);
                    } // End if (serializeCallback != null) 

                } // Next i 

            } // Next layerName 

        } // End Sub Test 


        public static void Test()
        {
            byte[] tileBytes = System.IO.File.ReadAllBytes("example.pbf");
            Test(tileBytes, null);
        } // End Sub Test 


        public static void Test(byte[] tileBytes)
        {
            Test(tileBytes, null);
        } // End Sub Test 


    } // End Class ExampleUsage 


} // End Namespace 

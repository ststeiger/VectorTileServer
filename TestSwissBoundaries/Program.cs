
namespace TestSwissBoundaries
{


    internal class Program
    {


        public static async System.Threading.Tasks.Task<int> Main(string[] args)
        {
            string basePath = System.AppContext.BaseDirectory;
            basePath = System.IO.Path.Combine(basePath, "..", "..", "..", "swissboundaries");
            basePath = System.IO.Path.GetFullPath(basePath);
            

            string input = System.IO.Path.Combine(basePath, @"swissBOUNDARIES3D_1_5_TLM_BEZIRKSGEBIET.shp");
            input = System.IO.Path.Combine(basePath, @"swissBOUNDARIES3D_1_5_TLM_KANTONSGEBIET.shp");
            input = System.IO.Path.Combine(basePath, @"swissBOUNDARIES3D_1_5_TLM_HOHEITSGRENZE.shp");
            input = System.IO.Path.Combine(basePath, @"swissBOUNDARIES3D_1_5_TLM_LANDESGEBIET.shp");
            input = System.IO.Path.Combine(basePath, @"swissBOUNDARIES3D_1_5_TLM_HOHEITSGEBIET.shp");


            // Load from shapefile
            NetTopologySuite.IO.ShapefileDataReader reader = new NetTopologySuite.IO
                .ShapefileDataReader(
                input,
                NetTopologySuite.Geometries.GeometryFactory.Default
            );

            // Prepare GeoJSON writer
            NetTopologySuite.IO.GeoJsonWriter geoJsonWriter = 
                new NetTopologySuite.IO.GeoJsonWriter();

            NetTopologySuite.Features.FeatureCollection featureCollection = 
                new NetTopologySuite.Features.FeatureCollection();

            while (reader.Read())
            {
                NetTopologySuite.Geometries.Geometry geometry = reader.Geometry;

                NetTopologySuite.Features.AttributesTable attributes = 
                    new NetTopologySuite.Features.AttributesTable();

                for (int i = 0; i < reader.DbaseHeader.NumFields; i++)
                {
                    string fieldName = reader.GetName(i);
                    object value = reader.GetValue(i);
                    attributes.Add(fieldName, value);
                } // Next i 

                featureCollection.Add(new NetTopologySuite.Features.Feature(geometry, attributes));
            } // Whend 

            // Save as GeoJSON
            string output = System.IO.Path.GetFileNameWithoutExtension(input) + ".geojson";
            string geoJson = geoJsonWriter.Write(featureCollection);
            await System.IO.File.WriteAllTextAsync(output, geoJson, System.Text.Encoding.UTF8);

            // https://www.swisstopo.admin.ch/en/landscape-model-swissboundaries3d
            // https://mapshaper.org/
            await System.Console.Out.WriteLineAsync("Check the output on mapshaper.org");

            return 0;
        } // End Sub Main 


    } // End Class Program 


} // End Namespace TestSwissBoundaries 

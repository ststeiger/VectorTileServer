
namespace TestSwissBoundaries
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string input = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\TestSwissBoundaries\swissboundaries\swissBOUNDARIES3D_1_5_TLM_BEZIRKSGEBIET.shp";
            input = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\TestSwissBoundaries\swissboundaries\swissBOUNDARIES3D_1_5_TLM_KANTONSGEBIET.shp";
            input = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\TestSwissBoundaries\swissboundaries\swissBOUNDARIES3D_1_5_TLM_HOHEITSGRENZE.shp";
            input = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\TestSwissBoundaries\swissboundaries\swissBOUNDARIES3D_1_5_TLM_LANDESGEBIET.shp";
            input = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\TestSwissBoundaries\swissboundaries\swissBOUNDARIES3D_1_5_TLM_HOHEITSGEBIET.shp";
            

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
            System.IO.File.WriteAllText(output, geoJson);

            // https://www.swisstopo.admin.ch/en/landscape-model-swissboundaries3d
            // https://mapshaper.org/
            System.Console.WriteLine("Check the output on mapshaper.org");
        }
    }



}

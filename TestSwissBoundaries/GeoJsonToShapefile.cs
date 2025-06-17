
namespace TestSwissBoundaries
{
    internal class GeoJsonToShapefile
    {
        public static void ConvertGeoJsonToShapefile(string geoJsonPath, string shapefilePath)
        {
            try
            {
                // Read GeoJSON
                string geoJson = System.IO.File.ReadAllText(geoJsonPath);
                NetTopologySuite.IO.GeoJsonReader reader = 
                    new NetTopologySuite.IO.GeoJsonReader();
                NetTopologySuite.Features.FeatureCollection featureCollection = 
                    reader.Read<NetTopologySuite.Features.FeatureCollection>(geoJson);

                if (featureCollection == null || featureCollection.Count == 0)
                {
                    throw new System.Exception("No features found in GeoJSON file.");
                }

                // Create Dbase header for attributes
                NetTopologySuite.IO.DbaseFileHeader dbfHeader = new NetTopologySuite.IO.DbaseFileHeader();
                // dbfHeader.SetCodePage(1252); // Standard encoding for shapefiles
                dbfHeader.Encoding = System.Text.Encoding.GetEncoding(1252);

                // Dynamically infer fields from the first feature
                NetTopologySuite.Features.IFeature firstFeature = featureCollection[0];
                foreach (string? attrName in firstFeature.Attributes.GetNames())
                {
                    object value = firstFeature.Attributes[attrName];
                    if (value == null) continue;

                    

                    // Determine field type based on value
                    if (value is string) 
                    {
                        string strVal = (string)value;
                        // dbfHeader.AddColumn(attrName, 'C', System.Math.Min(attrName.Length + 1, 255), 0);
                        dbfHeader.AddColumn(attrName, 'C', System.Math.Min(strVal.Length + 1, 255), 0);
                    }
                    else if (value is int || value is long)
                        dbfHeader.AddColumn(attrName, 'N', 10, 0);
                    else if (value is double || value is float)
                        dbfHeader.AddColumn(attrName, 'N', 20, 6);
                    else if (value is bool)
                        dbfHeader.AddColumn(attrName, 'L', 1, 0); // Logical
                    else if (value is System.DateTime)
                        dbfHeader.AddColumn(attrName, 'D', 8, 0); // Date
                    else
                        dbfHeader.AddColumn(attrName, 'C', 255, 0); // Fallback to string
                }

                // Create Shapefile writer
                NetTopologySuite.Geometries.GeometryFactory geometryFactory = new NetTopologySuite.Geometries.GeometryFactory();
                NetTopologySuite.IO.ShapefileDataWriter shpWriter = new NetTopologySuite.IO.ShapefileDataWriter(shapefilePath, geometryFactory)
                {
                    Header = dbfHeader
                };

                // Convert features to shapefile records
                System.Collections.Generic.List<NetTopologySuite.Features.IFeature> features = 
                    new System.Collections.Generic.List<NetTopologySuite.Features.IFeature>();
                foreach (NetTopologySuite.Features.IFeature? feature in featureCollection)
                {
                    if (feature.Geometry != null)
                    {
                        features.Add(feature);
                    }
                }

                // Write to Shapefile
                shpWriter.Write(features);
                System.Console.WriteLine($"Shapefile written successfully to {shapefilePath}");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"Error converting GeoJSON to Shapefile: {ex.Message}");
            }
        }
    }
}
#if false

namespace VectorTileSelector
{


    public class CityGMLExample
    {

        public static void Test()
        {
            string filePath = @"D:\stefan.steiger\Downloads\swissbuildings3d_3_0_2023_1265-11_2056_5728.citygml\swissBUILDINGS3D_3-0_1265-11.gml";
            LoadCityGML(filePath);
        }


        public static void LoadCityGML(string filePath)
        {
            System.Xml.Serialization.XmlRootAttribute rootAttribute = 
                new System.Xml.Serialization.XmlRootAttribute
            {
                ElementName = "CityModel",
                Namespace = "http://www.opengis.net/citygml/2.0"
            };


            System.Type[] knownTypes = new System.Type[]
            {
                typeof(CityGMLSharp.Gml.V3_2.EnvelopeType),
                typeof(CityGMLSharp.Gml.V3_2.EnvelopeWithTimePeriodType),
                typeof(CityGMLSharp.Ows.V1_1.BoundingBoxType),
                typeof(CityGMLSharp.Ows.V1_1.WGS84BoundingBoxType),

                typeof(CityGMLSharp.Citygml.Building.V3_0.BuildingType),
                typeof(CityGMLSharp.Citygml.Generics.V3_0.StringAttributeType)
                // Add other expected types here
            };


            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(
                typeof(CityGMLSharp.Citygml.V3_0.CityModelType),
                rootAttribute // ,knownTypes
            );

            using (System.IO.FileStream stream = System.IO.File.OpenRead(filePath))
            {
                if (serializer.Deserialize(stream) is CityGMLSharp.Citygml.V3_0.CityModelType model)
                {
                    System.Console.WriteLine($"Found {model.cityObjectMember?.Length ?? 0} city objects.");
                }
                else
                {
                    System.Console.WriteLine("Failed to parse CityGML.");
                }
            }
        }


    }


}
#endif

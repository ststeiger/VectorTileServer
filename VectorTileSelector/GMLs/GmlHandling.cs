
namespace VectorTileSelector
{

    using Gml.Xml2CSharp;

    // Problematic files 
    // \GMLs\Archived\swissBUILDINGS3D_3-0_1032-13.gml
    // \GMLs\Archived\swissBUILDINGS3D_3-0_1032-44.gml
    internal class GmlHandling
    {


        public static async System.Threading.Tasks.Task TestAsync()
        {
            string path = System.AppContext.BaseDirectory;
            path = System.IO.Path.Combine(path, "..", "..", "..", "GMLs", "Archived");
            path = System.IO.Path.GetFullPath(path);

            foreach (string filePath in System.IO.Directory.GetFiles(path, "*.gml"))
            {
                try
                {
                    await ReadFileAsync(filePath);
                } // End Try 
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine(ex.StackTrace);
                    System.Console.WriteLine(filePath);
                } // End Catch 

            } // Next filePath 

        } // End Task TestAsync 


        static async System.Threading.Tasks.Task ReadFileAsync(string filePath)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(
                typeof(CityModel)
            //,rootAttribute // ,knownTypes
            );

            using (System.IO.FileStream stream = System.IO.File.OpenRead(filePath))
            {
                if (serializer.Deserialize(stream) is CityModel model)
                {
                    System.Console.WriteLine($"Found {model.CityObjectMember?.Count ?? 0} city objects.");

                    System.Console.WriteLine(model.BoundedBy.Envelope.UpperCorner);
                    System.Console.WriteLine(model.BoundedBy.Envelope.LowerCorner);


                    foreach (CityObjectMember cityObject in model.CityObjectMember)
                    {
                        if (cityObject.Building == null)
                            continue;


                        if (cityObject.Building.BoundedBy2 == null)
                            System.Console.WriteLine(cityObject);

                        bool hasFoundGroundSurface = false;


                        foreach (BoundedBy2 bound in cityObject.Building.BoundedBy2)
                        {
                            if (bound.GroundSurface == null)
                                continue;

                            System.Console.WriteLine(bound.GroundSurface.Lod2MultiSurface.MultiSurface);

                            foreach (SurfaceMember surface in bound.GroundSurface.Lod2MultiSurface.MultiSurface.SurfaceMember)
                            {
                                System.Console.WriteLine(surface.Polygon.Exterior.LinearRing.PosList);
                                hasFoundGroundSurface = true;
                            } // Next surface 

                        } // Next bound 


                        bool hasFoundRoofSurface = false;


                        foreach (BoundedBy2 bound in cityObject.Building.BoundedBy2)
                        {
                            if (bound.RoofSurface == null)
                                continue;

                            System.Console.WriteLine(bound.RoofSurface.Lod2MultiSurface.MultiSurface);

                            foreach (SurfaceMember surface in bound.RoofSurface.Lod2MultiSurface.MultiSurface.SurfaceMember)
                            {
                                System.Console.WriteLine(surface.Polygon.Exterior.LinearRing.PosList);
                                hasFoundRoofSurface = true;
                            } // Next surface 

                        } // Next bound 


                        if (!hasFoundGroundSurface && !hasFoundRoofSurface)
                        {
                            System.Console.WriteLine(cityObject.Building);
                        }

                    } // Next cityObject 

                    // model.CityObjectMember[0].Building.BoundedBy2[0].GroundSurface.Lod2MultiSurface.

                }
                else
                {
                    System.Console.WriteLine("Failed to parse CityGML.");
                }
            } // End Using stream 

        } // End Task ReadFileAsync 


    } // End Class GmlHandling 


} // End Namespace 

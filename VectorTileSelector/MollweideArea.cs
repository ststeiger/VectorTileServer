
namespace VectorTileSelector
{


    public class MollweideArea
    {


        internal class DoubleCoordinates
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }

            public override string ToString()
            {
                return this.Latitude.ToString("N6") + ", " + this.Longitude.ToString("N6");
            } // End Function ToString 

        } // End Class DoubleCoordinates 


        public static void Test()
        {
            string inputFile = System.IO.Path.Combine(Db.KmlDirectory, "africa-burundi.kml");

            Xml2CSharp.KmlRegionBoundaryXml region_boundaries = Xml2CSharp.KmlRegionBoundaryXml.DeserializeFile(inputFile);
            System.Collections.Generic.List<Xml2CSharp.Coordinates> ls = region_boundaries.Document.Placemark.MultiGeometry.Polygon.OuterBoundaryIs.LinearRing.CoordinateList;
            double area = CalculatePolygonArea(ls);

            System.Console.WriteLine(area);

            // Burundi spheric: 28'034'768'830
            // Burundi Mollweide = 27'970'515'322.824875
            // Burundi AlbersA: 27'844'821'510.476734
            // Burundi CEAW:    27'844'703'736 Projected.World.CylindricalEqualAreaworld
            // Burundi actual:  27'834'000'000 = 27,834 km² 

        } // End Sub Test 


        public static double CalculatePolygonArea(System.Collections.Generic.List<Xml2CSharp.Coordinates> ls)
        {
            System.Collections.Generic.List<DoubleCoordinates> coordinates = new System.Collections.Generic.List<DoubleCoordinates>();

            for (int i = 0; i < ls.Count; ++i)
            {
                coordinates.Add(
                    new DoubleCoordinates()
                    {
                        Latitude = (double)ls[i].Latitude,
                        Longitude = (double)ls[i].Longitude
                    }
                    );

            } // Next i 

            return CalculatePolygonArea(coordinates);
        } // End Function CalculatePolygonArea 


        private static double CalculatePolygonArea(System.Collections.Generic.List<DoubleCoordinates> points)
        {
            // Project the polygon to a suitable projection for accurate area calculation
            // DotSpatial.Projections.ProjectionInfo wgs84 = DotSpatial.Projections.KnownCoordinateSystems.Geographic.World.WGS1984;
            // DotSpatial.Projections.ProjectionInfo utm = DotSpatial.Projections.KnownCoordinateSystems.Projected.UtmWgs1984.WGS1984UTMZone32N;

            DotSpatial.Projections.ProjectionInfo fromWgs84 = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(4326); // WGS84


            DotSpatial.Projections.ProjectionInfo equalAreaProjection = DotSpatial.Projections.KnownCoordinateSystems.Projected.World.CylindricalEqualAreaworld;


            // Mollweide equal-area projection 
            // DotSpatial.Projections.ProjectionInfo toMollweide = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(53009); // not working 

            // https://epsg.io/102013
            // DotSpatial.Projections.ProjectionInfo toAlbers = DotSpatial.Projections.ProjectionInfo.FromEpsgCode(102013); // not working 


            // https://epsg.io/102003

            // https://epsg.io/102001 // Albers canada
            // https://epsg.io/102003 // Albers USA 
            // https://epsg.io/102027 // Albers north asia
            // https://epsg.io/102022 // Albers Africa


            // https://epsg.io/53009
            DotSpatial.Projections.ProjectionInfo toMollweide = DotSpatial.Projections.ProjectionInfo.FromEsriString(@"
PROJCS[""Sphere_Mollweide"",
    GEOGCS[""Unknown datum based upon the Authalic Sphere"",
        DATUM[""Not_specified_based_on_Authalic_Sphere"",
            SPHEROID[""Sphere"", 6371000, 0],
            AUTHORITY[""EPSG"", ""6035""]],
        PRIMEM[""Greenwich"", 0],
        UNIT[""Degree"", 0.0174532925199433]],
    PROJECTION[""Mollweide""],
    PARAMETER[""central_meridian"", 0],
    PARAMETER[""false_easting"", 0],
    PARAMETER[""false_northing"", 0],
    UNIT[""metre"", 1,
        AUTHORITY[""EPSG"", ""9001""]],
    AXIS[""Easting"", EAST],
    AXIS[""Northing"", NORTH],
    AUTHORITY[""ESRI"", ""53009""]]
");


            //            // Albers Africa
            //            DotSpatial.Projections.ProjectionInfo toAlbersAfrica = DotSpatial.Projections.ProjectionInfo.FromEsriString(@"
            //PROJCS[""Africa_Albers_Equal_Area_Conic"",
            //    GEOGCS[""WGS 84"",
            //        DATUM[""WGS_1984"",
            //            SPHEROID[""WGS 84"", 6378137, 298.257223563,
            //                AUTHORITY[""EPSG"", ""7030""]],
            //            AUTHORITY[""EPSG"", ""6326""]],
            //        PRIMEM[""Greenwich"", 0,
            //            AUTHORITY[""EPSG"", ""8901""]],
            //        UNIT[""degree"", 0.0174532925199433,
            //            AUTHORITY[""EPSG"", ""9122""]],
            //        AUTHORITY[""EPSG"", ""4326""]],
            //    PROJECTION[""Albers_Conic_Equal_Area""],
            //    PARAMETER[""latitude_of_center"", 0],
            //    PARAMETER[""longitude_of_center"", 25],
            //    PARAMETER[""standard_parallel_1"", 20],
            //    PARAMETER[""standard_parallel_2"", -23],
            //    PARAMETER[""false_easting"", 0],
            //    PARAMETER[""false_northing"", 0],
            //    UNIT[""metre"", 1,
            //        AUTHORITY[""EPSG"", ""9001""]],
            //    AXIS[""Easting"", EAST],
            //    AXIS[""Northing"", NORTH],
            //    AUTHORITY[""ESRI"", ""102022""]]
            //");


            // toAlbers = DotSpatial.Projections.KnownCoordinateSystems.Projected.Europe.EuropeAlbersEqualAreaConic;
            // toAlbers = DotSpatial.Projections.KnownCoordinateSystems.Projected.Asia.AsiaNorthAlbersEqualAreaConic;
            // toAlbers = DotSpatial.Projections.KnownCoordinateSystems.Projected.Asia.AsiaSouthAlbersEqualAreaConic;


            // Begin Reproject 

            double[] latLonPoints = new double[points.Count * 2];
            double[] z = new double[points.Count];

            // dotspatial takes the x,y in a single array, and z in a separate array.  I'm sure there's a 
            // reason for this, but I don't know what it is.
            for (int i = 0; i < points.Count; i++)
            {
                latLonPoints[i * 2] = (double)points[i].Longitude;
                latLonPoints[i * 2 + 1] = (double)points[i].Latitude;
                z[i] = 0;
            } // Next i 

            DotSpatial.Projections.Reproject.ReprojectPoints(
                latLonPoints, z, fromWgs84, equalAreaProjection
                , 0, latLonPoints.Length / 2
            );

            DoubleCoordinates[] polyPoints = new DoubleCoordinates[latLonPoints.Length / 2];

            for (int i = 0; i < latLonPoints.Length / 2; ++i)
            {
                polyPoints[i] = new DoubleCoordinates() { Latitude = latLonPoints[i * 2 + 1], Longitude = latLonPoints[i * 2] };
            } // Next i 


            // End Reproject 



            // Create a list of vertices
            System.Collections.Generic.List<NetTopologySuite.Geometries.Coordinate> vertices =
                new System.Collections.Generic.List<NetTopologySuite.Geometries.Coordinate>();

            foreach (DoubleCoordinates point in polyPoints)
            {
                vertices.Add(new NetTopologySuite.Geometries.Coordinate(point.Longitude, point.Latitude));
            } // Next point 


            // Create a polygon geometry
            // GeometryFactory factory = new GeometryFactory();
            // Polygon polygon = factory.CreatePolygon(vertices.ToArray());

            // Create a polygon geometry
            NetTopologySuite.Geometries.Polygon polygon = new NetTopologySuite.Geometries.Polygon(
                new NetTopologySuite.Geometries.LinearRing(vertices.ToArray())
            );

            // Calculate the area of the polygon in square meters
            return System.Math.Abs(polygon.Area);
        } // End Function CalculatePolygonArea 


    } // End Class MollweideArea 


} // End Namespace 


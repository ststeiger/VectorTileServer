
// Licensed under the Apache License, Version 2.0
// http://www.apache.org/licenses/LICENSE-2.0
namespace VectorTileSelector.Xml2CSharp
{

    // [System.Diagnostics.DebuggerDisplay("{Latitude.ToString(\"N6\")}, {Longitude.ToString(\"N6\")}")]
    public class Coordinates
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public override string ToString()
        {
            return this.Latitude.ToString("N6", System.Globalization.CultureInfo.InvariantCulture)
                + ", "
                + this.Longitude.ToString("N6", System.Globalization.CultureInfo.InvariantCulture);
        } // End Function ToString 

    } // End Class Coordinates 


    // One tool to extracts from the OSM planet database using a custom polygon is Osmosis,
    // which is a command-line Java application. 
    // https://github.com/openstreetmap/osmosis

    // osmosis \
    // --read-xml file="planet.osm" \
    // --bounding-box left=MINLON bottom=MINLAT right=MAXLON top=MAXLAT \
    // --write-xml file="custom_polygon.osm"

    public class RectBounds
    {
        public decimal Min_Latitude { get; set; }
        public decimal Max_Latitude { get; set; }

        public decimal Min_Longitude { get; set; }
        public decimal Max_Longitude { get; set; }


        public Coordinates TopLeft
        {
            get
            {
                return new Coordinates()
                {
                    Latitude = this.Max_Latitude,
                    Longitude = this.Min_Longitude
                };
            }

        } // End Property TopLeft 


        public Coordinates BottomRight
        {
            get
            {
                return new Coordinates()
                {
                    Latitude = this.Min_Latitude,
                    Longitude = this.Max_Longitude
                };
            }

        } // End Property BottomRight 


        public override string ToString()
        {
            return this.TopLeft.ToString() + " - " + this.BottomRight.ToString();
        } // End Function ToString 


    } // End Class RectBounds 



    [System.Xml.Serialization.XmlRoot(ElementName = "LinearRing", Namespace = "http://earth.google.com/kml/2.0")]
    public class LinearRing
    {
        [System.Xml.Serialization.XmlElement(ElementName = "coordinates", Namespace = "http://earth.google.com/kml/2.0")]
        public string Coordinates { get; set; }


        [System.Xml.Serialization.XmlIgnore()]
        public double SphericalPolygonArea
        {
            get
            {
                // Convert coordinates to Cartesian
                System.Collections.Generic.List<System.Tuple<double, double>> cartesianCoordinates =
                    new System.Collections.Generic.List<System.Tuple<double, double>>();

                foreach (Xml2CSharp.Coordinates coord in this.CoordinateList)
                {
                    // The value 111319.9 is an approximation of the number of meters per degree of latitude at the equator.
                    // This approximation assumes a spherical Earth model.

                    double x = (double)coord.Longitude * 111319.9d * System.Math.Cos((double)coord.Latitude * System.Math.PI / 180.0d);
                    double y = (double)coord.Latitude * 111319.9d;
                    cartesianCoordinates.Add(new System.Tuple<double, double>(x, y));
                } // Next coord 

                // Shoelace formula
                double area = 0.0;
                int j = cartesianCoordinates.Count - 1;
                for (int i = 0; i < cartesianCoordinates.Count; i++)
                {
                    area += (cartesianCoordinates[j].Item1 + cartesianCoordinates[i].Item1)
                        *
                        (cartesianCoordinates[j].Item2 - cartesianCoordinates[i].Item2)
                    ;
                    j = i;
                } // Next i 

                return System.Math.Abs(area / 2.0);
            } // End Getter 

        } // End Property SphericalPolygonArea 


        public RectBounds RectangularBounds
        {
            get
            {
                System.Collections.Generic.List<Coordinates> coordinates = this.CoordinateList;

                decimal minLat = coordinates[0].Latitude;
                decimal maxLat = coordinates[0].Latitude;
                decimal minLon = coordinates[0].Longitude;
                decimal maxLon = coordinates[0].Longitude;

                for (int i = 0; i < coordinates.Count; ++i)
                {
                    if (coordinates[i].Latitude < minLat)
                        minLat = coordinates[i].Latitude;
                    if (coordinates[i].Latitude > maxLat)
                        maxLat = coordinates[i].Latitude;
                    if (coordinates[i].Longitude < minLon)
                        minLon = coordinates[i].Longitude;
                    if (coordinates[i].Longitude > maxLon)
                        maxLon = coordinates[i].Longitude;
                } // Next i 

                RectBounds rb = new RectBounds()
                {
                    Min_Latitude = minLat,
                    Max_Latitude = maxLat,
                    Min_Longitude = minLon,
                    Max_Longitude = maxLon
                };

                return rb;
            } // End Getter 

        } // End Property RectangularBounds 



        [System.Xml.Serialization.XmlIgnore()]
        public System.Collections.Generic.List<Coordinates> CoordinateList
        {
            get
            {
                System.Collections.Generic.List<Coordinates> retValue = new System.Collections.Generic.List<Coordinates>();
                string[] allCoordinates = this.Coordinates.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < allCoordinates.Length; ++i)
                {
                    string[] coordsPair = allCoordinates[i].Split(new char[] { ',', ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);

                    decimal lng = decimal.Parse(coordsPair[0], System.Globalization.CultureInfo.InvariantCulture);
                    decimal lat = decimal.Parse(coordsPair[1], System.Globalization.CultureInfo.InvariantCulture);

                    Coordinates coords = new Coordinates() { Latitude = lat, Longitude = lng };
                    retValue.Add(coords);
                } // Next i 

                return retValue;
            } // End Getter 

        } // End Property CoordinateList 


    } // End Class LinearRing 


    [System.Xml.Serialization.XmlRoot(ElementName = "outerBoundaryIs", Namespace = "http://earth.google.com/kml/2.0")]
    public class OuterBoundaryIs
    {
        [System.Xml.Serialization.XmlElement(ElementName = "LinearRing", Namespace = "http://earth.google.com/kml/2.0")]
        public LinearRing LinearRing { get; set; }
    }

    [System.Xml.Serialization.XmlRoot(ElementName = "Polygon", Namespace = "http://earth.google.com/kml/2.0")]
    public class Polygon
    {
        [System.Xml.Serialization.XmlElement(ElementName = "outerBoundaryIs", Namespace = "http://earth.google.com/kml/2.0")]
        public OuterBoundaryIs OuterBoundaryIs { get; set; }
    }

    [System.Xml.Serialization.XmlRoot(ElementName = "MultiGeometry", Namespace = "http://earth.google.com/kml/2.0")]
    public class MultiGeometry
    {
        [System.Xml.Serialization.XmlElement(ElementName = "Polygon", Namespace = "http://earth.google.com/kml/2.0")]
        public Polygon Polygon { get; set; }
    }


    [System.Xml.Serialization.XmlRoot(ElementName = "PolyStyle", Namespace = "http://earth.google.com/kml/2.0")]
    public class PolyStyle
    {
        [System.Xml.Serialization.XmlElement(ElementName = "color", Namespace = "http://earth.google.com/kml/2.0")]
        public string Color { get; set; }
        [System.Xml.Serialization.XmlElement(ElementName = "outline", Namespace = "http://earth.google.com/kml/2.0")]
        public string Outline { get; set; }
    }


    [System.Xml.Serialization.XmlRoot(ElementName = "Style", Namespace = "http://earth.google.com/kml/2.0")]
    public class Style
    {
        [System.Xml.Serialization.XmlElement(ElementName = "PolyStyle", Namespace = "http://earth.google.com/kml/2.0")]
        public PolyStyle PolyStyle { get; set; }
    }


    [System.Xml.Serialization.XmlRoot(ElementName = "Placemark", Namespace = "http://earth.google.com/kml/2.0")]
    public class Placemark
    {
        [System.Xml.Serialization.XmlElement(ElementName = "MultiGeometry", Namespace = "http://earth.google.com/kml/2.0")]
        public MultiGeometry MultiGeometry { get; set; }
        [System.Xml.Serialization.XmlElement(ElementName = "Style", Namespace = "http://earth.google.com/kml/2.0")]
        public Style Style { get; set; }
    }


    [System.Xml.Serialization.XmlRoot(ElementName = "Document", Namespace = "http://earth.google.com/kml/2.0")]
    public class Document
    {
        [System.Xml.Serialization.XmlElement(ElementName = "Placemark", Namespace = "http://earth.google.com/kml/2.0")]
        public Placemark Placemark { get; set; }
    }


    [System.Xml.Serialization.XmlRoot(ElementName = "kml", Namespace = "http://earth.google.com/kml/2.0")]
    public class KmlRegionBoundaryXml
    {
        [System.Xml.Serialization.XmlElement(ElementName = "Document", Namespace = "http://earth.google.com/kml/2.0")]
        public Document Document { get; set; }

        [System.Xml.Serialization.XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }


        public static KmlRegionBoundaryXml DeserializeFile(string path)
        {
            KmlRegionBoundaryXml kml = null;
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(KmlRegionBoundaryXml));

            using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
            {
                kml = (KmlRegionBoundaryXml)serializer.Deserialize(reader);
                // Access the deserialized Kml object
            }

            return kml;
        } // End Sub DeserializeFile 


    } // End Class KmlRegionBoundaryXml 


} // End Namespace 

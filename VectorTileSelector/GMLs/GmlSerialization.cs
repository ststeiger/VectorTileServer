
namespace Gml.Xml2CSharp
{

    using System.Xml.Serialization;
    using System.Collections.Generic;


    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.opengis.net/gml")]
    public class Envelope
    {
        [XmlElement(ElementName = "lowerCorner", Namespace = "http://www.opengis.net/gml")]
        public string LowerCorner { get; set; }
        [XmlElement(ElementName = "upperCorner", Namespace = "http://www.opengis.net/gml")]
        public string UpperCorner { get; set; }
        [XmlAttribute(AttributeName = "srsName")]
        public string SrsName { get; set; }
        [XmlAttribute(AttributeName = "srsDimension")]
        public string SrsDimension { get; set; }
    }

    [XmlRoot(ElementName = "boundedBy", Namespace = "http://www.opengis.net/gml")]
    public class BoundedBy
    {
        [XmlElement(ElementName = "Envelope", Namespace = "http://www.opengis.net/gml")]
        public Envelope Envelope { get; set; }
    }

    [XmlRoot(ElementName = "stringAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
    public class StringAttribute
    {
        [XmlElement(ElementName = "value", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "intAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
    public class IntAttribute
    {
        [XmlElement(ElementName = "value", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "doubleAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
    public class DoubleAttribute
    {
        [XmlElement(ElementName = "value", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "measuredHeight", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class MeasuredHeight
    {
        [XmlAttribute(AttributeName = "uom")]
        public string Uom { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "LinearRing", Namespace = "http://www.opengis.net/gml")]
    public class LinearRing
    {
        [XmlElement(ElementName = "posList", Namespace = "http://www.opengis.net/gml")]
        public string PosList { get; set; }
    }

    [XmlRoot(ElementName = "exterior", Namespace = "http://www.opengis.net/gml")]
    public class Exterior
    {
        [XmlElement(ElementName = "LinearRing", Namespace = "http://www.opengis.net/gml")]
        public LinearRing LinearRing { get; set; }
        [XmlElement(ElementName = "CompositeSurface", Namespace = "http://www.opengis.net/gml")]
        public CompositeSurface CompositeSurface { get; set; }
    }

    [XmlRoot(ElementName = "Polygon", Namespace = "http://www.opengis.net/gml")]
    public class Polygon
    {
        [XmlElement(ElementName = "exterior", Namespace = "http://www.opengis.net/gml")]
        public Exterior Exterior { get; set; }
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "surfaceMember", Namespace = "http://www.opengis.net/gml")]
    public class SurfaceMember
    {
        [XmlElement(ElementName = "Polygon", Namespace = "http://www.opengis.net/gml")]
        public Polygon Polygon { get; set; }
    }

    [XmlRoot(ElementName = "CompositeSurface", Namespace = "http://www.opengis.net/gml")]
    public class CompositeSurface
    {
        [XmlElement(ElementName = "surfaceMember", Namespace = "http://www.opengis.net/gml")]
        public List<SurfaceMember> SurfaceMember { get; set; }
    }

    [XmlRoot(ElementName = "Solid", Namespace = "http://www.opengis.net/gml")]
    public class Solid
    {
        [XmlElement(ElementName = "exterior", Namespace = "http://www.opengis.net/gml")]
        public Exterior Exterior { get; set; }
        [XmlAttribute(AttributeName = "srsName")]
        public string SrsName { get; set; }
        [XmlAttribute(AttributeName = "srsDimension")]
        public string SrsDimension { get; set; }
    }

    [XmlRoot(ElementName = "lod2Solid", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class Lod2Solid
    {
        [XmlElement(ElementName = "Solid", Namespace = "http://www.opengis.net/gml")]
        public Solid Solid { get; set; }
    }

    [XmlRoot(ElementName = "MultiSurface", Namespace = "http://www.opengis.net/gml")]
    public class MultiSurface
    {
        [XmlElement(ElementName = "surfaceMember", Namespace = "http://www.opengis.net/gml")]
        public List<SurfaceMember> SurfaceMember { get; set; }
        [XmlAttribute(AttributeName = "srsName")]
        public string SrsName { get; set; }
        [XmlAttribute(AttributeName = "srsDimension")]
        public string SrsDimension { get; set; }
    }

    [XmlRoot(ElementName = "lod2MultiSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class Lod2MultiSurface
    {
        [XmlElement(ElementName = "MultiSurface", Namespace = "http://www.opengis.net/gml")]
        public MultiSurface MultiSurface { get; set; }
    }

    [XmlRoot(ElementName = "WallSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class WallSurface
    {
        [XmlElement(ElementName = "stringAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<StringAttribute> StringAttribute { get; set; }
        [XmlElement(ElementName = "intAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<IntAttribute> IntAttribute { get; set; }
        [XmlElement(ElementName = "lod2MultiSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public Lod2MultiSurface Lod2MultiSurface { get; set; }
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "boundedBy", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class BoundedBy2
    {
        [XmlElement(ElementName = "WallSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public WallSurface WallSurface { get; set; }
        [XmlElement(ElementName = "RoofSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public RoofSurface RoofSurface { get; set; }
        [XmlElement(ElementName = "GroundSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public GroundSurface GroundSurface { get; set; }
    }

    [XmlRoot(ElementName = "RoofSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class RoofSurface
    {
        [XmlElement(ElementName = "stringAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<StringAttribute> StringAttribute { get; set; }
        [XmlElement(ElementName = "intAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<IntAttribute> IntAttribute { get; set; }
        [XmlElement(ElementName = "doubleAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<DoubleAttribute> DoubleAttribute { get; set; }
        [XmlElement(ElementName = "lod2MultiSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public Lod2MultiSurface Lod2MultiSurface { get; set; }
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "GroundSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class GroundSurface
    {
        [XmlElement(ElementName = "stringAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<StringAttribute> StringAttribute { get; set; }
        [XmlElement(ElementName = "intAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<IntAttribute> IntAttribute { get; set; }
        [XmlElement(ElementName = "lod2MultiSurface", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public Lod2MultiSurface Lod2MultiSurface { get; set; }
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "Building", Namespace = "http://www.opengis.net/citygml/building/2.0")]
    public class Building
    {
        [XmlElement(ElementName = "stringAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<StringAttribute> StringAttribute { get; set; }
        [XmlElement(ElementName = "intAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<IntAttribute> IntAttribute { get; set; }
        [XmlElement(ElementName = "doubleAttribute", Namespace = "http://www.opengis.net/citygml/generics/2.0")]
        public List<DoubleAttribute> DoubleAttribute { get; set; }
        [XmlElement(ElementName = "measuredHeight", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public MeasuredHeight MeasuredHeight { get; set; }
        [XmlElement(ElementName = "lod2Solid", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public Lod2Solid Lod2Solid { get; set; }
        [XmlElement(ElementName = "boundedBy", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public List<BoundedBy2> BoundedBy2 { get; set; }
        [XmlAttribute(AttributeName = "id", Namespace = "http://www.opengis.net/gml")]
        public string Id { get; set; }
    }

    [XmlRoot(ElementName = "cityObjectMember", Namespace = "http://www.opengis.net/citygml/2.0")]
    public class CityObjectMember
    {
        [XmlElement(ElementName = "Building", Namespace = "http://www.opengis.net/citygml/building/2.0")]
        public Building Building { get; set; }
    }

    [XmlRoot(ElementName = "X3DMaterial", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
    public class X3DMaterial
    {
        [XmlElement(ElementName = "isFront", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public string IsFront { get; set; }
        [XmlElement(ElementName = "diffuseColor", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public string DiffuseColor { get; set; }
        [XmlElement(ElementName = "target", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public List<string> Target { get; set; }
    }

    [XmlRoot(ElementName = "surfaceDataMember", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
    public class SurfaceDataMember
    {
        [XmlElement(ElementName = "X3DMaterial", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public X3DMaterial X3DMaterial { get; set; }
    }

    [XmlRoot(ElementName = "Appearance", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
    public class Appearance
    {
        [XmlElement(ElementName = "theme", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public string Theme { get; set; }
        [XmlElement(ElementName = "surfaceDataMember", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public List<SurfaceDataMember> SurfaceDataMember { get; set; }
    }

    [XmlRoot(ElementName = "appearanceMember", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
    public class AppearanceMember
    {
        [XmlElement(ElementName = "Appearance", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public Appearance Appearance { get; set; }
    }

    [XmlRoot(ElementName = "CityModel", Namespace = "http://www.opengis.net/citygml/2.0")]
    public class CityModel
    {
        [XmlElement(ElementName = "name", Namespace = "http://www.opengis.net/gml")]
        public string Name { get; set; }
        [XmlElement(ElementName = "boundedBy", Namespace = "http://www.opengis.net/gml")]
        public BoundedBy BoundedBy { get; set; }
        [XmlElement(ElementName = "cityObjectMember", Namespace = "http://www.opengis.net/citygml/2.0")]
        public List<CityObjectMember> CityObjectMember { get; set; }
        [XmlElement(ElementName = "appearanceMember", Namespace = "http://www.opengis.net/citygml/appearance/2.0")]
        public AppearanceMember AppearanceMember { get; set; }
        [XmlAttribute(AttributeName = "brid", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Brid { get; set; }
        [XmlAttribute(AttributeName = "tran", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Tran { get; set; }
        [XmlAttribute(AttributeName = "frn", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Frn { get; set; }
        [XmlAttribute(AttributeName = "wtr", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Wtr { get; set; }
        [XmlAttribute(AttributeName = "sch", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Sch { get; set; }
        [XmlAttribute(AttributeName = "veg", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Veg { get; set; }
        [XmlAttribute(AttributeName = "xlink", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xlink { get; set; }
        [XmlAttribute(AttributeName = "tun", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Tun { get; set; }
        [XmlAttribute(AttributeName = "tex", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Tex { get; set; }
        [XmlAttribute(AttributeName = "gml", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Gml { get; set; }
        [XmlAttribute(AttributeName = "gen", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Gen { get; set; }
        [XmlAttribute(AttributeName = "dem", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Dem { get; set; }
        [XmlAttribute(AttributeName = "app", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string App { get; set; }
        [XmlAttribute(AttributeName = "luse", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Luse { get; set; }
        [XmlAttribute(AttributeName = "xAL", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string XAL { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "smil20lang", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Smil20lang { get; set; }
        [XmlAttribute(AttributeName = "pbase", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Pbase { get; set; }
        [XmlAttribute(AttributeName = "smil20", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Smil20 { get; set; }
        [XmlAttribute(AttributeName = "bldg", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Bldg { get; set; }
        [XmlAttribute(AttributeName = "core", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Core { get; set; }
        [XmlAttribute(AttributeName = "grp", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Grp { get; set; }
    }

}

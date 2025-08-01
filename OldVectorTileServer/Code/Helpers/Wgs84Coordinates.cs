
namespace VectorTileServer
{
    
    
    public class Wgs84Coordinates
    {
        public double Latitude;
        public double Longitude;
        public int ZoomLevel;
        
        
        public Wgs84Coordinates()
        { }
        
        
        public Wgs84Coordinates(double lat, double lng, int zoom)
        {
            this.Latitude = lat;
            this.Longitude = lng;
            this.ZoomLevel = zoom;
        }
        
        
        public static double TileToLongitude(double x, int z)
        {
            return (x / System.Math.Pow(2, z) * 360 - 180);
        }
        
        
        public static double TileToLatitude(double y, int z)
        {
            double n = System.Math.PI - 2 * System.Math.PI * y / System.Math.Pow(2, z);
            return (180 / System.Math.PI * System.Math.Atan(0.5 * (System.Math.Exp(n) - System.Math.Exp(-n))));
        }



        public static Wgs84Coordinates FromTile(TileCoords xyz)
        {
            return FromTile(xyz.X, xyz.Y, xyz.Z);
        }


        public static Wgs84Coordinates FromTile(int x, int y, int z)
        {
            Wgs84Coordinates coord = new Wgs84Coordinates();
            coord.ZoomLevel = z;

            coord.Longitude = (x / System.Math.Pow(2, z) * 360 - 180);
            
            double n = System.Math.PI - 2 * System.Math.PI * y / System.Math.Pow(2, z);
            coord.Latitude = 180 / System.Math.PI * System.Math.Atan( 0.5 * 
                    ( System.Math.Exp(n) - System.Math.Exp(-n) )
                )
            ;
            
            return coord;
        }
        
        
        public override string ToString()
        {
            return "( Latitude: " + this.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) 
                                  + "    Longitude: " + this.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)
                                  + " )";
        }
        
        
    }
    
    
}

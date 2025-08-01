
namespace VectorTileServer
{
    
    
    public class TileCoords
    {
        public int X;
        public int Y;
        public int Z;
        
        
        public override string ToString()
        {
            return "( X: " + this.X.ToString(System.Globalization.CultureInfo.InvariantCulture) 
                           + "   Y: " + this.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)
                           + "   Z: " + this.Z.ToString(System.Globalization.CultureInfo.InvariantCulture)
                           + " )";
        }
        
        
        
        public static TileCoords FromWgs84(double lat, double lon, int zoom)
        {
            TileCoords CalcTileXY = new TileCoords();
            
            CalcTileXY.X = System.Convert.ToInt32(System.Math.Floor((lon + 180) / 360.0 * System.Math.Pow(2, zoom)));
            CalcTileXY.Y = System.Convert.ToInt32(
                System.Math.Floor(
                    (1 - System.Math.Log(
                         System.Math.Tan(lat * System.Math.PI / 180) 
                         + 1 / System.Math.Cos(lat * System.Math.PI / 180)
                        ) 
                        / System.Math.PI
                     ) / 2 
                    * System.Math.Pow(2, zoom)
                    )
            );
            
            CalcTileXY.Z = zoom;
            return CalcTileXY;
        }
        
        
    }
    
    
}


namespace TestSQLite
{


#if true 
    using SQLiteConnection = Community.CsharpSqlite.SQLiteClient.SqliteConnection;
    using SQLiteCommand = Community.CsharpSqlite.SQLiteClient.SqliteCommand;
    using SQLiteDataReader = Community.CsharpSqlite.SQLiteClient.SqliteDataReader;
#else
    using SQLiteConnection = System.Data.SQLite.SQLiteConnection;
    using SQLiteCommand = System.Data.SQLite.SQLiteCommand;
    using SQLiteDataReader = System.Data.SQLite.SQLiteDataReader;
#endif



    class Program
    {
        
        
        public static System.IO.Stream GetRawTile(int x, int y, int zoom)
        {
            System.IO.Stream stream = null;
            
            try
            {
                string path = "/root/github/ststeiger/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";                
                
                if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    path = @"file:///D:/username/Documents/Visual%20Studio%202017/Projects/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";
                    // path = @"file:///D:/username/Documents/Visual Studio 2017/Projects/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";
                    // path = @"D:/username/Documents/Visual Studio 2017/Projects/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";
                    // path = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot\2017-07-03_france_monaco.mbtiles";

                //if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase))
                // path = @"D:\username\Downloads\2017-07-03_planet_z0_z14.mbtiles";


                using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", path)))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand() { Connection = conn, CommandText = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", x, y, zoom) })
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        System.Console.WriteLine("perhaps");
                        
                        if (reader.Read())
                        {
                            System.Console.WriteLine("reads");
                            stream = reader.GetStream(reader.GetOrdinal("tile_data"));
                        }
                    }
                }
            }
            catch(System.Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                throw new System.MemberAccessException("Could not load tile from Mbtiles");
            }
            
            return stream;
        } // End Function GetRawTile 




        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        public static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // = 2^zoom - tmsY - 1
        } // End Function FromTmsY 



        static void Main(string[] args)
        {
            // https://localhost:44378/tiles/8528/5975/14
            int x = 8528, y = 5975, z = 14;
            y = FromTmsY(y, z);

            System.IO.Stream strm = GetRawTile(x, y, z);
            System.Console.WriteLine(strm.Length);
            System.Console.WriteLine(" --- Press any key to continue --- ");
        }


    }


}

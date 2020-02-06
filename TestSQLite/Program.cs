
namespace TestSQLite
{

    using SQLiteConnection = Community.CsharpSqlite.SQLiteClient.SqliteConnection;
    using SQLiteCommand = Community.CsharpSqlite.SQLiteClient.SqliteCommand;
    using SQLiteDataReader = Community.CsharpSqlite.SQLiteClient.SqliteDataReader;
    
    
    class Program
    {
        
        
        public static System.IO.Stream GetRawTile(int x, int y, int zoom)
        {
            System.IO.Stream stream = null;
            
            try
            {
                string path = "/root/github/ststeiger/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";                
                
                if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    path = @"d:\username\wwwroot\2017-07-03_france_monaco.mbtiles";
                
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

        
        
        
        static void Main(string[] args)
        {
            // https://localhost:44378/tiles/8528/5975/14
            // curl 'https://localhost:44378/tiles/8528/5975/14
            // System.IO.Stream strm = GetRawTile(8528, 5975, 14);
            System.IO.Stream strm = GetRawTile(8528, 5975, 14);
            System.Console.WriteLine(strm.Length);
            System.Console.WriteLine(" --- Press any key to continue --- ");
        }
    }
}
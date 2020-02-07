
// #define MANAGED_SQLITE 


namespace TestSQLite
{


#if MANAGED_SQLITE     
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


        public static void SqlScripts()
        {
            string tiles = @"
CREATE VIEW tiles AS   
SELECT 
     map.zoom_level as zoom_level
    ,map.tile_column as tile_column
    ,map.tile_row as tile_row
    ,images.tile_data as tile_data   
FROM map 
JOIN images ON map.tile_id = images.tile_id
";


            string map = @"CREATE TABLE map (zoom_level INTEGER,tile_column INTEGER,tile_row INTEGER,tile_id TEXT)";
            string images = @"CREATE TABLE images (tile_id TEXT, tile_data BLOB)";

            string tilesTable = @"CREATE TABLE tiles (zoom_level INTEGER,tile_column INTEGER,tile_row INTEGER, tile_data BLOB)";


        } // End Sub SqlScripts 


        public static void RecreateTable_old()
        {
            string uri = "D:/username/Documents/Visual Studio 2017/Projects/MyVectorTile/TestSQLite/bin/Debug/MyDatabase.sqlite";
            uri = @"D:\username\Documents\Visual Studio 2017\Projects\MyVectorTile\TestSQLite\bin\Debug\MyDatabase.sqlite";

#if !MANAGED_SQLITE
            SQLiteConnection.CreateFile(uri);
#endif



            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", uri)))
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string createTilesTableSql = @"CREATE TABLE tiles (zoom_level INTEGER,tile_column INTEGER,tile_row INTEGER, tile_data BLOB)";

                SQLiteCommand command = new SQLiteCommand(createTilesTableSql, connection);
                command.ExecuteNonQuery();

                // sql = "insert into tiles (zoom_level, tile_column, tile_row, tile_data) values (z, x, y, data)";
                // command = new SQLiteCommand(sql, connection);
                // command.ExecuteNonQuery();

                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            } // End Using connection 

        } // End Sub RecreateTable_old 


        public static void RecreateTable()
        {
            // SQLiteConnection.CreateFile("MyDatabase.sqlite");

            // https://stackoverflow.com/questions/15292880/create-sqlite-database-and-table/15292958
            // https://stackoverflow.com/questions/19729514/community-csharpsqllite-create-database

            string uri = "file:///D:/username/Documents/Visual%20Studio%202017/Projects/MyVectorTile/TestSQLite/bin/Debug/MyDatabase.sqlite";


#if !MANAGED_SQLITE
            // uri = "D:/username/Documents/Visual Studio 2017/Projects/MyVectorTile/TestSQLite/bin/Debug/MyDatabase.sqlite";
            uri = @"D:\username\Documents\Visual Studio 2017\Projects\MyVectorTile\TestSQLite\bin\Debug\MyDatabase.sqlite";
            SQLiteConnection.CreateFile(uri);
#endif


#if MANAGED_SQLITE
            
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;New=True;", uri)))
            //using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};New=True;", uri)))
#else
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", uri)))
#endif
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string createTilesTableSql = @"CREATE TABLE tiles (zoom_level INTEGER,tile_column INTEGER,tile_row INTEGER, tile_data BLOB)";

                SQLiteCommand command = new SQLiteCommand(createTilesTableSql, connection);
                command.ExecuteNonQuery();

                // https://stackoverflow.com/questions/625029/how-do-i-store-and-retrieve-a-blob-from-sqlite
                byte[] photo = new byte[] { 1, 2, 3, 4, 5 };
                command.CommandText = "INSERT INTO tiles (tile_data) VALUES (@photo)";
                //command.Parameters.Add("@photo", System.Data.DbType.Binary, photo.Length).Value = photo;
                command.AddParameter("@photo", photo, System.Data.DbType.Binary, photo.Length);

                command.ExecuteNonQuery();


                // sql = "insert into tiles (zoom_level, tile_column, tile_row, tile_data) values (z, x, y, data)";
                // command = new SQLiteCommand(sql, connection);
                // command.ExecuteNonQuery();

                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
            } // End Using connection 

        } // End Sub RecreateTable 


        // "SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}

        public static System.IO.Stream GetRawTile(int x, int y, int zoom)
        {
            System.IO.Stream stream = null;

            try
            {
                string path = "/root/github/ststeiger/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";

                if (System.Environment.OSVersion.Platform != System.PlatformID.Unix)
                    path = @"file:///D:/username/Documents/Visual%20Studio%202017/Projects/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";
                //  path = @"D:/username/Documents/Visual Studio 2017/Projects/VectorTileServer/VectorTileServer/wwwroot/2017-07-03_france_monaco.mbtiles";
                // path = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot\2017-07-03_france_monaco.mbtiles";

                if ("COR".Equals(System.Environment.UserDomainName, System.StringComparison.InvariantCultureIgnoreCase)) path = @"D:\username\Downloads\2017-07-03_planet_z0_z14.mbtiles";

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
            catch (System.Exception ex)
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

            RecreateTable();

            System.IO.Stream strm = GetRawTile(x, y, z);
            System.Console.WriteLine(strm.Length);
            System.Console.WriteLine(" --- Press any key to continue --- ");
        } // End Sub Main 


    } // End Class Program 


} // End Namespace TestSQLite 

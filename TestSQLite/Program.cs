
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
        public static int InvertTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // = 2^zoom - tmsY - 1
        } // End Function FromTmsY 

        public static long InvertTmsY(long tmsY, long zoom)
        {
            return (1L << (int)zoom) - tmsY - 1L; // = 2^zoom - tmsY - 1
        } // End Function FromTmsY 

        public static void TestGetTile()
        {
            // https://localhost:44378/tiles/8528/5975/14
            int x = 8528, y = 5975, z = 14;
            y = InvertTmsY(y, z);

            long a = ToSingleNumber(0, 0, 14);
            long[] b = SingleNumberToXYZ(a);

            RecreateTable();

            System.IO.Stream strm = GetRawTile((int)x, (int)y, (int)z);
            System.Console.WriteLine(strm.Length);
        }


        static void Main(string[] args)
        {

            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        }


        static void SpeedUpPlanetFile()
        {
            const bool multiTableMode = true;

            // string tilesQuery = "SELECT zoom_level, tile_row, tile_column, tile_data FROM tiles ORDER BY zoom_level, tile_row, tile_column LIMIT 10 OFFSET 0";
            string tilesQuery = "SELECT zoom_level, tile_row, tile_column, tile_data FROM tiles ORDER BY zoom_level, tile_row, tile_column ";

            string mbTilesSource = @"D:\username\Documents\Visual Studio 2017\Projects\VectorTileServer\VectorTileServer\wwwroot\2017-07-03_france_monaco.mbtiles";
            mbTilesSource = @"D:\username\Downloads\2017-07-03_planet_z0_z14.mbtiles";

            string dataTarget = @"D:\username\Desktop\monaco.db3";
            dataTarget = @"E:\planet{0}.db3";

            dataTarget = string.Format(dataTarget, multiTableMode ? "_multiTableMode" : "");


            if (System.IO.File.Exists(dataTarget))
                System.IO.File.Delete(dataTarget);

            SQLiteConnection.CreateFile(dataTarget);

            // https://stackoverflow.com/questions/633274/what-causes-a-journal-file-to-be-created-in-sqlite/633390
            using (SQLiteConnection targetConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3; Journal Mode=Off;", dataTarget)))
            {
                if (targetConnection.State != System.Data.ConnectionState.Open)
                    targetConnection.Open();

                using (SQLiteCommand writeCommand = new SQLiteCommand("", targetConnection))
                {

                    if (multiTableMode)
                    {
                        for (int j = 0; j < 15; ++j)
                        {
                            if (j < 17)
                                writeCommand.CommandText = string.Format(@"CREATE TABLE tiles_{0} (id INTEGER, tile_data BLOB);", j.ToString().PadLeft(2, '0'));
                            else
                                writeCommand.CommandText = string.Format(@"CREATE TABLE tiles_{0} (id BIGINTEGER, tile_data BLOB);", j.ToString().PadLeft(2, '0'));

                            writeCommand.ExecuteNonQuery();
                        } // Next j 
                    }
                    else
                    {
                        writeCommand.CommandText = @"CREATE TABLE tiles(id BIGINTEGER, tile_data BLOB)";
                        writeCommand.ExecuteNonQuery();
                    }



                    writeCommand.CommandText = @"INSERT INTO tiles(id, tile_data) VALUES (@id, @tile)"; ;
                    writeCommand.Parameters.Add("@id", System.Data.DbType.Int64).Value = 0;
                    writeCommand.Parameters.Add("@tile", System.Data.DbType.Binary).Value = System.DBNull.Value;

                    using (SQLiteConnection sourceConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3; Read Only=True;", mbTilesSource)))
                    {
                        if (sourceConnection.State != System.Data.ConnectionState.Open)
                            sourceConnection.Open();

                        using (SQLiteCommand readCommand = new SQLiteCommand(tilesQuery, sourceConnection))
                        {
                            SQLiteDataReader reader = readCommand.ExecuteReader(System.Data.CommandBehavior.SequentialAccess);

                            while (reader.Read())
                            {
                                int zoom_level = reader.GetInt32(reader.GetOrdinal("zoom_level"));
                                int x = reader.GetInt32(reader.GetOrdinal("tile_row"));
                                int y = reader.GetInt32(reader.GetOrdinal("tile_column"));
                                y = InvertTmsY(y, zoom_level); // To TMS


                                long a = multiTableMode ? ToSingleNumberWithoutZoomLevel(x, y, zoom_level) : ToSingleNumber(x, y, zoom_level);


                                byte[] data = null;

                                using (System.IO.MemoryStream stream = (System.IO.MemoryStream)reader.GetStream(reader.GetOrdinal("tile_data")))
                                {
                                    data = stream.ToArray();
                                } // End Using stream 

                                if (multiTableMode)
                                    writeCommand.CommandText = string.Format(@"INSERT INTO tiles_{0}(id, tile_data) VALUES (@id, @tile)", zoom_level.ToString().PadLeft(2, '0'));

                                writeCommand.Parameters["@id"].Value = a;
                                writeCommand.Parameters["@tile"].Value = data;
                                writeCommand.Parameters["@tile"].Size = data.Length;
                                writeCommand.ExecuteNonQuery();

                                // System.Console.Write(a);
                                // System.Console.Write(": ");
                                // System.Console.WriteLine(data.Length);
                            } // Whend 

                        } // End Using readCommand 

                        if (sourceConnection.State != System.Data.ConnectionState.Closed)
                            sourceConnection.Close();
                    } // End Using sourceConnection 

                    for (int j = writeCommand.Parameters.Count - 1; j > 0; --j)
                    {
                        writeCommand.Parameters.RemoveAt(j);
                    } // Next j 

                    if (multiTableMode)
                    {
                        for (int j = 0; j < 15; ++j)
                        {
                            writeCommand.CommandText = string.Format(@"CREATE INDEX IX_tiles_{0}_id ON tiles_{0}(id);", j.ToString().PadLeft(2, '0'));
                            writeCommand.ExecuteNonQuery();
                        } // Next j 

                    }
                    else
                    {
                        writeCommand.CommandText = @"CREATE INDEX IX_tiles_id ON tiles(id);";
                        writeCommand.ExecuteNonQuery();
                    }


                } // End Using writeCommand 


                if (targetConnection.State != System.Data.ConnectionState.Closed)
                    targetConnection.Close();
            } // End Using targetConnection

        } // End Sub SpeedUpPlanetFile 



        public static long ToSingleNumber_old(long x, long y, long z)
        {
            long magic = (y * (1L << 20) + x) * 100L + z;

            return magic;
        }

        public static long ToSingleNumber(long x, long y, long z)
        {
            // [z, y, x]
            long magic = (((z << 20) | y) << 20) | x;

            return magic;
        }


        public static long ToSingleNumberWithoutZoomLevel(long x, long y, long z)
        {
            // [y, x] 
            long magic = (y << (int)z) | x; // y*2^z+x
            return magic;
        }


        public static long[] SingleNumberToXYZ(long n)
        {
            // const long maxValue = (1L << 20) - 1;
            const long maxValue = 0xFFFFF;

            return new long[] {
                (n & maxValue), // x
                ((n>>20) & maxValue) , // y
                ((n>>40) & maxValue)  // z
            };
        }


        public static long GetZoomBegin(long z)
        {
            long zoomBegin = (z << 40);
            return zoomBegin;
        }


        public static long GetZoomEnd(long z)
        {
            long zoomEnd = ((z + 1L) << 40) - 1L;
            return zoomEnd;
        }


    } // End Class Program 


} // End Namespace TestSQLite 

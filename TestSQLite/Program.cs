
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
        } // End Function InvertTmsY 


        public static long InvertTmsY(long tmsY, long zoom)
        {
            return (1L << (int)zoom) - tmsY - 1L; // = 2^zoom - tmsY - 1
        } // End Function InvertTmsY 


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
            System.Console.WriteLine(BoundingBox.Parse("41.06894,11.36768,55.51167,19.03189"));
            
            /*
            
            string[] files = GetZoomLevelFiles(@"D:\temp", 20);
            string[] queries = GetZoomLevelQueries(20);

            System.Console.WriteLine(files);
            System.Console.WriteLine(queries);

            SpeedUpPlanetFile();
            */
            
            System.Console.WriteLine(" --- Press any key to continue --- ");
            System.Console.ReadKey();
        }


        static void SpeedUpPlanetFile()
        {
            const bool multiTableMode = true;
            const bool multiFileMode = false;
            string mbTilesSource = @"D:\username\Downloads\2017-07-03_planet_z0_z14.mbtiles";
            string targetDirectory = @"E:\planet";

            // https://stackoverflow.com/questions/12831504/optimizing-fast-access-to-a-readonly-sqlite-database
            // https://blog.devart.com/increasing-sqlite-performance.html
            using (SQLiteConnection sourceConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3; Read Only=True;", mbTilesSource)))
            {
                if (sourceConnection.State != System.Data.ConnectionState.Open)
                    sourceConnection.Open();

                long minZoomLevel = -1;
                long maxZoomLevel = -1;

                using (SQLiteCommand cmdCount = new SQLiteCommand("SELECT MIN(zoom_level) FROM tiles", sourceConnection))
                {
                    minZoomLevel = (long)cmdCount.ExecuteScalar();
                }

                using (SQLiteCommand cmdCount = new SQLiteCommand("SELECT MAX(zoom_level) FROM tiles", sourceConnection))
                {
                    maxZoomLevel = (long)cmdCount.ExecuteScalar();
                    maxZoomLevel++;
                }
                

                bool isFirstZoomLevel = true;

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                for (long currentZoomLevel = minZoomLevel; currentZoomLevel < maxZoomLevel; ++currentZoomLevel)
                {
                    System.Console.Write(System.DateTime.Now.ToString("dd'.'MM'.'yyyy' 'HH':'mm':'ss.fff': '"));
                    System.Console.Write("Started processing zoom-level ");
                    System.Console.WriteLine(currentZoomLevel);

                    sw.Start();
                    
                    string idDataType = (currentZoomLevel < 17) ? "INTEGER" : "BIGINTEGER";
                    string tableZoomLevelString = multiTableMode ? ("_" + currentZoomLevel.ToString().PadLeft(2, '0')) : "";
                    string fileZoomLevelString = multiFileMode ? ("_" + currentZoomLevel.ToString().PadLeft(2, '0')) : "";
                    string insertCommand = string.Format("INSERT INTO tiles{0}(id, tile_data) VALUES (@id, @tile);", tableZoomLevelString);
                    string targetDatabase = string.Format("{0}{1}{2}.db3", targetDirectory, multiTableMode ? "_mt" : "", fileZoomLevelString);


                    if (isFirstZoomLevel || multiFileMode)
                    {
                        if (System.IO.File.Exists(targetDatabase))
                            System.IO.File.Delete(targetDatabase);

                        SQLiteConnection.CreateFile(targetDatabase);
                    } // End if (isFirstZoomLevel || multiFileMode) 

                    // https://stackoverflow.com/questions/633274/what-causes-a-journal-file-to-be-created-in-sqlite/633390
                    // https://stackoverflow.com/questions/1711631/improve-insert-per-second-performance-of-sqlite
                    using (SQLiteConnection targetConnection = new SQLiteConnection(string.Format("Data Source={0};Version=3; Journal Mode=Off; Synchronous=OFF", targetDatabase)))
                    {
                        if (targetConnection.State != System.Data.ConnectionState.Open)
                            targetConnection.Open();

                        string createTableCommand = "CREATE TABLE tiles(id BIGINTEGER, tile_data BLOB);";

                        if (multiTableMode || multiFileMode)
                        {
                            createTableCommand = string.Format("CREATE TABLE tiles{0}(id {1}, tile_data BLOB);", tableZoomLevelString, idDataType);
                        }

                        if ((!multiTableMode && !multiFileMode && isFirstZoomLevel) || multiFileMode || multiTableMode)
                        {
                            using (SQLiteCommand cmdCreateTable = new SQLiteCommand(createTableCommand, targetConnection))
                            {
                                cmdCreateTable.ExecuteNonQuery();
                            }
                        } // End if ((!multiTableMode && !multiFileMode && isFirstZoomLevel) || multiFileMode || multiTableMode) 


                        string tilesQuery = "SELECT zoom_level, tile_row, tile_column, tile_data FROM tiles WHERE zoom_level = @zoom_level ORDER BY tile_row, tile_column ";


                        using (SQLiteCommand readCommand = new SQLiteCommand(tilesQuery, sourceConnection))
                        {
                            readCommand.Parameters.Add("@zoom_level", System.Data.DbType.Int32).Value = currentZoomLevel;

                            SQLiteDataReader reader = readCommand.ExecuteReader(System.Data.CommandBehavior.SequentialAccess);
                            using (SQLiteCommand writeCommand = new SQLiteCommand(insertCommand, targetConnection))
                            {
                                writeCommand.Parameters.Add("@id", System.Data.DbType.Int64).Value = 0;
                                writeCommand.Parameters.Add("@tile", System.Data.DbType.Binary).Value = System.DBNull.Value;

                                while (reader.Read())
                                {
                                    int zoom_level = reader.GetInt32(reader.GetOrdinal("zoom_level"));
                                    int x = reader.GetInt32(reader.GetOrdinal("tile_row"));
                                    int y = reader.GetInt32(reader.GetOrdinal("tile_column"));
                                    y = InvertTmsY(y, zoom_level); // To TMS

                                    long newId = (multiTableMode || multiFileMode) ? ToSingleNumberWithoutZoomLevel(x, y, zoom_level) : ToSingleNumber(x, y, zoom_level);
                                    
                                    byte[] data = null;
                                    using (System.IO.MemoryStream stream = (System.IO.MemoryStream)reader.GetStream(reader.GetOrdinal("tile_data")))
                                    {
                                        data = stream.ToArray();
                                    } // End Using stream 

                                    writeCommand.Parameters["@id"].Value = newId;
                                    writeCommand.Parameters["@tile"].Value = data;
                                    writeCommand.Parameters["@tile"].Size = data.Length;
                                    writeCommand.ExecuteNonQuery();
                                } // Whend 

                            } // End Using writeCommand

                        } // End Using readCommand 


                        // https://stackoverflow.com/questions/1711631/improve-insert-per-second-performance-of-sqlite
                        if ((!multiTableMode && !multiFileMode && isFirstZoomLevel) || multiFileMode || multiTableMode)
                        {
                            string createIndexCommand = string.Format("CREATE INDEX IX_tiles{0}_id ON tiles{0}(id);", tableZoomLevelString);
                            using (SQLiteCommand cmdCreateIndex = new SQLiteCommand(createIndexCommand, targetConnection))
                            {
                                cmdCreateIndex.ExecuteNonQuery();
                            } // End Using cmdCreateIndex
                        } // End if ((!multiTableMode && !multiFileMode && isFirstZoomLevel) || multiFileMode || multiTableMode) 

                        if (targetConnection.State != System.Data.ConnectionState.Closed)
                            targetConnection.Close();

                    } // End Using targetConnection 

                    sw.Stop();

                    System.Console.Write(System.DateTime.Now.ToString("dd'.'MM'.'yyyy' 'HH':'mm':'ss.fff': '"));
                    System.Console.Write("Finished zoom-level ");
                    System.Console.WriteLine(currentZoomLevel);
                    System.Console.Write("Elapsed time (ms): ");
                    System.Console.WriteLine(sw.ElapsedMilliseconds);
                    sw.Reset();

                    isFirstZoomLevel = false;
                } // Next currentZoomLevel 

                if (sourceConnection.State != System.Data.ConnectionState.Closed)
                    sourceConnection.Close();
            } // End Using sourceConnection 

        } // End Sub SpeedUpPlanetFile 


        public static string[] GetZoomLevelFiles(string baseDir, long maxZoomLevel)
        {
            string[] files = new string[maxZoomLevel];

            for (int i = 0; i < maxZoomLevel; ++i)
            {
                files[i] = @"planet_at_zoom_" + i.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, '0') + ".db3";
                files[i] = System.IO.Path.Combine(baseDir, files[i]);

                files[i] = string.Format("Data Source={0};Version=3; Read Only=True;", files[i]);
            } // Next i 

            return files;
        } // End Function GetZoomLevelFiles 


        public static string[] GetZoomLevelQueries(long maxZoomLevel)
        {
            string[] queries = new string[maxZoomLevel];

            for (int i = 0; i < maxZoomLevel; ++i)
            {
                queries[i] = string.Format("SELECT tile_data FROM tiles_{0} WHERE id = @id", i.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, '0'));
            } // Next i 

            // queries[0] = "SELECT tile_data FROM tiles WHERE id = @id";

            return queries;
        } // End Function GetZoomLevelQueries 




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

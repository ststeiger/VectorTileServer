
namespace VectorTileSelector
{


    internal class TileDataReader
    {


        // https://alastaira.wordpress.com/2011/07/06/converting-tms-tile-coordinates-to-googlebingosm-tile-coordinates/
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int FromTmsY(int tmsY, int zoom)
        {
            return (1 << zoom) - tmsY - 1; // 2^zoom - tmsY - 1
        } // End Function FromTmsY 


        private static byte[] DecompressGzip(byte[] compressedData)
        {
            using System.IO.MemoryStream compressedStream = new System.IO.MemoryStream(compressedData);
            using System.IO.Compression.GZipStream gzipStream = new System.IO.Compression.GZipStream(compressedStream, System.IO.Compression.CompressionMode.Decompress);
            using System.IO.MemoryStream resultStream = new System.IO.MemoryStream();

            gzipStream.CopyTo(resultStream);

            byte[] retValue = resultStream.ToArray();
            return retValue;
        } // End Function DecompressGzip 


        private static string Serialize(System.Collections.Generic.List<
             System.Collections.Generic.List<Mapbox.VectorTile.Geometry.Point2d<float>>
         > geomDouble)
        {
            // Serialize to JSON
            string json = System.Text.Json.JsonSerializer.Serialize(geomDouble, new System.Text.Json.JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true
            });

            return json;
        } // End Function Serialize 


        public static async System.Threading.Tasks.Task ReadTileData(int x, int y, int z)
        {
            y = FromTmsY(y, z);

            Microsoft.Data.Sqlite.SqliteConnectionStringBuilder builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder()
            {
                DataSource = @"D:\stefan.steiger\Documents\Visual Studio 2022\github\VectorTileServer\VectorTileServer\wwwroot\maps\liechtenstein.mbtiles", // path to your SQLite file
                Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly, // optional: ReadWrite, ReadOnly, etc.
                Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Default // optional
            };


            await using System.Data.Common.DbConnection conn = new Microsoft.Data.Sqlite.SqliteConnection(builder.ConnectionString);
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();


            await using System.Data.Common.DbCommand cmd = conn.CreateCommand();
            // PRAGMA mmap_size = 0; 
            // PRAGMA cache_size = -1; 
            cmd.CommandText = @$"SELECT tile_data FROM tiles WHERE tile_column = {x} AND tile_row = {y} AND zoom_level = {z};";


            await using System.Data.Common.DbDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection);
            if (!await reader.ReadAsync())
            {
                System.Console.WriteLine("Tile not found");
                return;
            }

            int ordinal = reader.GetOrdinal("tile_data");
            long length = reader.GetBytes(ordinal, 0, null, 0, 0);

            // Allocate a byte array
            byte[] gzippedTileData = new byte[length];

            // Read the data into the byte array
            reader.GetBytes(ordinal, 0, gzippedTileData, 0, (int)length);

            byte[] tileData = DecompressGzip(gzippedTileData);

            bool serializePolygons = false;
            Mapbox.VectorTile.ExampleUsage.Test(tileData, serializePolygons ? Serialize : null);
        } // End Task ReadTileData 


        public static async System.Threading.Tasks.Task Test()
        {
            await TileDataReader.ReadTileData(8630, 5754, 14);
        } // End Task ReadTileData 


    } // End Class TileDataReader 


} // End Namespace 

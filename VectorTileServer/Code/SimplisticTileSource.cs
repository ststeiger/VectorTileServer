
#define SYSTEM_SQLITE

#if SYSTEM_SQLITE
using System.Data.SQLite;
#else
    using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
    using SQLiteCommand= Mono.Data.Sqlite.SqliteCommand;
    using SQLiteDataReader= Mono.Data.Sqlite.SqliteDataReader;
#endif 


namespace VectorTileServer
{

    public class GeoExtent
    {
        public double West;
        public double South;
        public double East;
        public double North;

    }


    public class SimplisticTileSource
    {

        protected string m_path;

        public int MinZoom;
        public int MaxZoom;


        public string Name;
        public string Description;
        public string MBTilesVersion;
        

        public GeoExtent Bounds;
        public System.Windows.Point Center;


        public System.DateTime PlanetTime;
        public int PixelScale;


        public string Format;
        public string Attribution;
        public string JSON; // vector_layers


        public SimplisticTileSource()
        { }

        // table: omtm


        public static System.DateTime PlanetTimeToDateTime(ulong planetTime)
        {
            System.DateTime dt = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            System.DateTime dtPlanetTime = dt.AddMilliseconds(planetTime);
            return dtPlanetTime;
        }


        public SimplisticTileSource(string path)
        {
            this.m_path = path;

            if(System.IO.File.Exists(this.m_path))
                LoadMetadata();
        }




        private const string s_tileSQL = @"
CREATE VIEW tiles AS 
SELECT 
        map.zoom_level as zoom_level 
    ,map.tile_column as tile_column 
    ,map.tile_row as tile_row 
    ,images.tile_data as tile_data 
FROM map 
JOIN images ON map.tile_id = images.tile_id 
; ";


        public System.IO.Stream GetRawTile(int x, int y, int zoom)
        {


            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", this.m_path)))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand() { Connection = conn, CommandText = string.Format("SELECT * FROM tiles WHERE tile_column = {0} and tile_row = {1} and zoom_level = {2}", x, y, zoom) })
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            var stream = reader.GetStream(reader.GetOrdinal("tile_data"));
                            return stream;
                        }
                    }
                }
            }
            catch
            {
                throw new System.MemberAccessException("Could not load tile from Mbtiles");
            }

            return null;
        } // End Function GetRawTile 


        private void LoadMetadata()
        {
            try
            {

                // https://github.com/klokantech/tileserver-php/blob/master/tileserver.php
                // https://github.com/apdevelop/map-tile-service-asp-net-core/tree/master/Src/MapTileService

                // SELECT * FROM metadata
                // SELECT min(zoom_level) AS min, max(zoom_level) AS max FROM tiles
                // SELECT hex(substr(tile_data,1,2)) AS magic FROM tiles LIMIT 1
                // SELECT min(tile_column) AS w, max(tile_column) AS e, min(tile_row) AS s,
                // max(tile_row) AS n FROM tiles WHERE zoom_level='.$metadata['maxzoom']);
                // w = -180 + 360 * ($resultdata[0]['w'] / pow(2, $metadata['maxzoom']));
                // e = -180 + 360 * ((1 + $resultdata[0]['e']) / pow(2, $metadata['maxzoom']));
                // n = $this->row2lat($resultdata[0]['n'], $metadata['maxzoom']);
                // s = $this->row2lat($resultdata[0]['s'] - 1, $metadata['maxzoom']);
                // metadata['bounds'] = implode(',', array($w, $s, $e, $n));

                using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", this.m_path)))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand() { Connection = conn, CommandText = "SELECT * FROM metadata;" })
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            string name = reader["name"].ToString();
                            switch (name.ToLower())
                            {
                                case "bounds":
                                    string val = reader["value"].ToString();
                                    string[] vals = val.Split(new char[] { ',' });
                                    this.Bounds = new GeoExtent() // GeoJSON-extents (aka sequence [long, lat] )
                                    {
                                        // bottom left (southWest)
                                        West = System.Convert.ToDouble(vals[0], System.Globalization.CultureInfo.InvariantCulture), // longitude
                                        South = System.Convert.ToDouble(vals[1], System.Globalization.CultureInfo.InvariantCulture), // latitude
                                        // right top (northEast)
                                        East = System.Convert.ToDouble(vals[2], System.Globalization.CultureInfo.InvariantCulture), // longitude 
                                        North = System.Convert.ToDouble(vals[3], System.Globalization.CultureInfo.InvariantCulture) // latitide 
                                    };
                                    break;
                                case "center":
                                    // https://github.com/klokantech/tileserver-gl/blob/master/src/utils.js
                                    // module.exports.fixTileJSONCenter
                                    val = reader["value"].ToString();
                                    vals = val.Split(new char[] { ',' });
                                    this.Center = new System.Windows.Point()
                                    {
                                        X = System.Convert.ToDouble(vals[0], System.Globalization.CultureInfo.InvariantCulture),
                                        Y = System.Convert.ToDouble(vals[1], System.Globalization.CultureInfo.InvariantCulture)
                                    };
                                    break;
                                case "minzoom":
                                    this.MinZoom = System.Convert.ToInt32(reader["value"], System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "maxzoom":
                                    this.MaxZoom = System.Convert.ToInt32(reader["value"], System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "pixel_scale":
                                    this.PixelScale = System.Convert.ToInt32(reader["value"], System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "name":
                                    this.Name = reader["value"].ToString();
                                    break;
                                case "description":
                                    this.Description = reader["value"].ToString();
                                    break;
                                case "version":
                                    this.MBTilesVersion = reader["value"].ToString();
                                    break;

                                case "planettime":
                                    ulong ulpt = System.Convert.ToUInt64(reader["value"], System.Globalization.CultureInfo.InvariantCulture);
                                    this.PlanetTime = PlanetTimeToDateTime(ulpt);
                                    break;

                                case "json":
                                    this.JSON = System.Convert.ToString(reader["value"], System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "format":
                                    this.Format = System.Convert.ToString(reader["value"], System.Globalization.CultureInfo.InvariantCulture);
                                    break;


                            } // End switch (name.ToLower()) 

                        } // Whend 

                    } // End Using cmd 

                } // End Using conn 

            } // End Try 
            catch (System.Exception e)
            {
                throw new System.MemberAccessException("Could not load Mbtiles source file");
            } // End Catch 

        } // End Sub LoadMetadata 


    } // End Class SimplisticTileSource 


} // End  namespace VectorTileServer 

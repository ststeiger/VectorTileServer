﻿
#define SYSTEM_SQLITE

#if SYSTEM_SQLITE
using System.Data.SQLite;
#else
    using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
    using SQLiteCommand= Mono.Data.Sqlite.SqliteCommand;
    using SQLiteDataReader= Mono.Data.Sqlite.SqliteDataReader;
#endif 


using System.Windows;


namespace VectorTileRenderer.Sources
{
    // MbTiles loading code in GIST by geobabbler
    // https://gist.github.com/geobabbler/9213392

    public class MbTilesSource 
        : IVectorTileSource
    {
        
        public GlobalMercator.GeoExtent Bounds { get; private set; }
        public GlobalMercator.CoordinatePair Center { get; private set; }
        public int MinZoom { get; private set; }
        public int MaxZoom { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string MBTilesVersion { get; private set; }
        public string Path { get; private set; }

        System.Collections.Generic.Dictionary<string, VectorTile> tileCache = 
            new System.Collections.Generic.Dictionary<string, VectorTile>();

        private GlobalMercator gmt = new GlobalMercator();

        public MbTilesSource(string path)
        {
            this.Path = path;
            loadMetadata();
        }

        private void loadMetadata()
        {
            try
            {
                
                
                
                
                using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", this.Path)))
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
                                    this.Bounds = new GlobalMercator.GeoExtent()
                                    {
                                        West = System.Convert.ToDouble(vals[0]), 
                                        South = System.Convert.ToDouble(vals[1]), 
                                        East = System.Convert.ToDouble(vals[2]), 
                                        North = System.Convert.ToDouble(vals[3])
                                    };
                                    break;
                                case "center":
                                    val = reader["value"].ToString();
                                    vals = val.Split(new char[] { ',' });
                                    this.Center = new GlobalMercator.CoordinatePair()
                                    {
                                        X = System.Convert.ToDouble(vals[0]), 
                                        Y = System.Convert.ToDouble(vals[1])
                                    };
                                    break;
                                case "minzoom":
                                    this.MinZoom = System.Convert.ToInt32(reader["value"]);
                                    break;
                                case "maxzoom":
                                    this.MaxZoom = System.Convert.ToInt32(reader["value"]);
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

                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                throw new System.MemberAccessException("Could not load Mbtiles source file");
            }
        }

        public System.IO.Stream GetRawTile(int x, int y, int zoom)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Path)))
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
        }


        public void ExtractTile(int x, int y, int zoom, string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            using (System.IO.FileStream fileStream = System.IO.File.Create(path))
            {
                using (System.IO.Stream tileStream = GetRawTile(x, y, zoom))
                {
                    tileStream.Seek(0, System.IO.SeekOrigin.Begin);
                    tileStream.CopyTo(fileStream);
                } // End Using tileStream 

            } // End using fileStream 
        }


        public async System.Threading.Tasks.Task<VectorTile> GetVectorTile(int x, int y, int zoom)
        {
            var extent = new Rect(0, 0, 1, 1);
            bool overZoomed = false;

            if(zoom > MaxZoom)
            {
                var bounds = gmt.TileLatLonBounds(x, y, zoom);

                var northEast = new GlobalMercator.CoordinatePair();
                northEast.X = bounds.East;
                northEast.Y = bounds.North;

                var northWest = new GlobalMercator.CoordinatePair();
                northWest.X = bounds.West;
                northWest.Y = bounds.North;

                var southEast = new GlobalMercator.CoordinatePair();
                southEast.X = bounds.East;
                southEast.Y = bounds.South;

                var southWest = new GlobalMercator.CoordinatePair();
                southWest.X = bounds.West;
                southWest.Y = bounds.South;

                var center = new GlobalMercator.CoordinatePair();
                center.X = (northEast.X + southWest.X) / 2;
                center.Y = (northEast.Y + southWest.Y) / 2;

                var biggerTile = gmt.LatLonToTile(center.Y, center.X, MaxZoom);

                var biggerBounds = gmt.TileLatLonBounds(biggerTile.X, biggerTile.Y, MaxZoom);

                var newL = Utils.ConvertRange(northWest.X, biggerBounds.West, biggerBounds.East, 0, 1);
                var newT = Utils.ConvertRange(northWest.Y, biggerBounds.North, biggerBounds.South, 0, 1);

                var newR = Utils.ConvertRange(southEast.X, biggerBounds.West, biggerBounds.East, 0, 1);
                var newB = Utils.ConvertRange(southEast.Y, biggerBounds.North, biggerBounds.South, 0, 1);

                extent = new Rect(new Point(newL, newT), new Point(newR, newB));
                //thisZoom = MaxZoom;

                x = biggerTile.X;
                y = biggerTile.Y;
                zoom = MaxZoom;

                overZoomed = true;
            }
            
            try
            {
                var actualTile = await getCachedVectorTile(x, y, zoom);

                if (actualTile != null)
                {
                    actualTile.IsOverZoomed = overZoomed;
                    actualTile = actualTile.ApplyExtent(extent);
                }

                return actualTile;

            } catch(System.Exception e)
            {
                return null;
            }
        }

        async System.Threading.Tasks.Task<VectorTile> getCachedVectorTile(int x, int y, int zoom)
        {
            var key = x.ToString() + "," + y.ToString() + "," + zoom.ToString();

            lock(key)
            {
                if (tileCache.ContainsKey(key))
                {
                    return tileCache[key];
                }

                using (var rawTileStream = GetRawTile(x, y, zoom))
                {
                    var pbfTileProvider = new PbfTileSource(rawTileStream);
                    var tile = pbfTileProvider.GetVectorTile(x, y, zoom).Result;
                    tileCache[key] = tile;

                    return tile;
                }
            }
            
        }

        async System.Threading.Tasks.Task<System.IO.Stream> ITileSource.GetTile(int x, int y, int zoom)
        {
            return GetRawTile(x, y, zoom);
        }
    }
}

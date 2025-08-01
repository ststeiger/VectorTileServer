


using System.Threading.Tasks;
using System.Windows;

namespace VectorTileRenderer.Sources
{
    public class PbfTileSource : IVectorTileSource
    {
        public string Path { get; set; } = "";
        public System.IO.Stream Stream { get; set; } = null;

        public PbfTileSource(string path)
        {
            this.Path = path;
        }

        public PbfTileSource(System.IO.Stream stream)
        {
            this.Stream = stream;
        }

        public async Task<System.IO.Stream> GetTile(int x, int y, int zoom)
        {
            string qualifiedPath = Path
                .Replace("{x}", x.ToString())
                .Replace("{y}", y.ToString())
                .Replace("{z}", zoom.ToString());

            return System.IO.File.Open(qualifiedPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
        }
        
        public async Task<VectorTile> GetVectorTile(int x, int y, int zoom)
        {
            if(Path != "")
            {
                using (System.IO.Stream stream = await GetTile(x, y, zoom))
                {
                    return await unzipStream(stream);
                }
            }
            else if (Stream != null)
            {
                return  await unzipStream(Stream);
            }

            return null;
        }

        private async Task<VectorTile> unzipStream(System.IO.Stream stream)
        {
            if (isGZipped(stream))
            {
                using (System.IO.Compression.GZipStream zipStream =
                    new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
                {
                    using (System.IO.MemoryStream resultStream = new System.IO.MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        resultStream.Seek(0, System.IO.SeekOrigin.Begin);
                        return await loadStream(resultStream);
                    } // End Using resultStream 

                } // End Using zipStream 
            }
            else
            {
                return await loadStream(stream);
            }
        }
        
        private async Task<VectorTile> loadStream(System.IO.Stream stream)
        {
            Mapbox.VectorTile.VectorTile mbLayers = 
                new Mapbox.VectorTile.VectorTile(readTillEnd(stream));

            return await baseTileToVector(mbLayers);
        }

        static string convertGeometryType(Mapbox.VectorTile.Geometry.GeomType type)
        {
            if (type == Mapbox.VectorTile.Geometry.GeomType.LINESTRING)
            {
                return "LineString";
            } else if (type == Mapbox.VectorTile.Geometry.GeomType.POINT)
            {
                return "Point";
            }
            else if (type == Mapbox.VectorTile.Geometry.GeomType.POLYGON)
            {
                return "Polygon";
            } else
            {
                return "Unknown";
            }
        }

        private static async Task<VectorTile> baseTileToVector(object baseTile)
        {
            Mapbox.VectorTile.VectorTile tile = baseTile as Mapbox.VectorTile.VectorTile;
            VectorTile result = new VectorTile();

            foreach (string lyrName in tile.LayerNames())
            {
                Mapbox.VectorTile.VectorTileLayer lyr = tile.GetLayer(lyrName);

                VectorTileLayer vectorLayer = new VectorTileLayer();
                vectorLayer.Name = lyrName;

                for (int i = 0; i < lyr.FeatureCount(); i++)
                {
                    Mapbox.VectorTile.VectorTileFeature feat = lyr.GetFeature(i);

                    VectorTileFeature vectorFeature = new VectorTileFeature();
                    vectorFeature.Extent = 1;
                    vectorFeature.GeometryType = convertGeometryType(feat.GeometryType);
                    vectorFeature.Attributes = feat.GetProperties();

                    System.Collections.Generic.List<System.Collections.Generic.List<Point>> vectorGeometry = 
                        new System.Collections.Generic.List<System.Collections.Generic.List<Point>>();

                    foreach (System.Collections.Generic.List<Mapbox.VectorTile.Geometry.Point2d<int>> points in feat.Geometry<int>())
                    {
                        System.Collections.Generic.List<Point> vectorPoints = 
                            new System.Collections.Generic.List<Point>();

                        foreach (Mapbox.VectorTile.Geometry.Point2d<int> coordinate in points)
                        {
                            double dX = (double)coordinate.X / (double)lyr.Extent;
                            double dY = (double)coordinate.Y / (double)lyr.Extent;

                            vectorPoints.Add(new Point(dX, dY));

                            // double newX = Utils.ConvertRange(dX, extent.Left, extent.Right, 0, vectorFeature.Extent);
                            // double newY = Utils.ConvertRange(dY, extent.Top, extent.Bottom, 0, vectorFeature.Extent);

                            //vectorPoints.Add(new Point(newX, newY));
                        }

                        vectorGeometry.Add(vectorPoints);
                    }

                    vectorFeature.Geometry = vectorGeometry;
                    vectorLayer.Features.Add(vectorFeature);
                }

                result.Layers.Add(vectorLayer);
            }

            return result;
        }
        
        byte[] readTillEnd(System.IO.Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        bool isGZipped(System.IO.Stream stream)
        {
            return isZipped(stream, 3, "1F-8B-08");
        }

        bool isZipped(System.IO.Stream stream, int signatureSize = 4, string expectedSignature = "50-4B-03-04")
        {
            if (stream.Length < signatureSize)
                return false;

            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            }
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            string actualSignature = System.BitConverter.ToString(signature);
            if (actualSignature == expectedSignature)
                return true;

            return false;
        } // End Function isZipped 


    }


}

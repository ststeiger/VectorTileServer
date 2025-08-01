
namespace VectorTileRenderer.Sources
{
    public interface ITileSource
    {
        System.Threading.Tasks.Task<System.IO.Stream> GetTile(int x, int y, int zoom);
    }
}

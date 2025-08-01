
namespace VectorTileRenderer.Sources
{
    public interface IVectorTileSource : ITileSource
    {
        System.Threading.Tasks.Task<VectorTile> GetVectorTile(int x, int y, int zoom);
    }
}

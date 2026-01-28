using Microsoft.UI.Xaml.Media.Imaging;

namespace PacmanGameProject.Game.Rendering;

public class TileSet
{
    public Dictionary<int, BitmapImage> Tiles { get; } = new();

    public void Load()
    {
        int[] ids =
        {
            6, 9, 10, 12, 13, 14, 16, 18, 19, 22, 24, 25, 26, 28, 29, 30,
            33, 34, 36, 37, 40, 41, 42, 46, 47, 99
        };

        foreach (int id in ids)
        {
            Tiles[id] = new BitmapImage(
                new Uri($"ms-appx:///Assets/Tiles/{id}.png"));
        }
    }

    public BitmapImage Get(int id) => Tiles[id];
}

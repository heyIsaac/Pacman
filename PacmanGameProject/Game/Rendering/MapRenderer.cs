using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.Rendering;

public class MapRenderer
{
    public List<Pellet> Pellets { get; } = new();
    public Dictionary<Pellet, Image> PelletSprites { get; } = new();

    public void Draw(Canvas canvas, int[,] layout, int tileSize)
    {
        canvas.Children.Clear();
        Pellets.Clear();
        PelletSprites.Clear();

        DrawMap(canvas, layout, tileSize);
    }

    private void DrawMap(Canvas canvas, int[,] layout, int tileSize)
    {
        for (int y = 0; y < layout.GetLength(0); y++)
        {
            for (int x = 0; x < layout.GetLength(1); x++)
            {
                int id = layout[y, x];

                int backgroundId = id;

                // se for pellet, o fundo é chão
                if (id == 40 || id == 46)
                    backgroundId = 37;

                // fundo
                Image tile = new Image
                {
                    Source = new BitmapImage(
                        new Uri($"ms-appx:///Assets/Tiles/{backgroundId}.png")),
                    Width = tileSize,
                    Height = tileSize
                };

                Canvas.SetLeft(tile, x * tileSize);
                Canvas.SetTop(tile, y * tileSize);
                Canvas.SetZIndex(tile, 0);
                canvas.Children.Add(tile);

                // pellet por cima
                if (id == 40 || id == 46)
                {
                    Image pelletSprite = new Image
                    {
                        Source = new BitmapImage(
                            new Uri($"ms-appx:///Assets/Tiles/{id}.png")),
                        Width = tileSize,
                        Height = tileSize
                    };

                    Canvas.SetLeft(pelletSprite, x * tileSize);
                    Canvas.SetTop(pelletSprite, y * tileSize);
                    Canvas.SetZIndex(pelletSprite, 1);
                    canvas.Children.Add(pelletSprite);

                    var pelletEntity = new Pellet(
                        x * tileSize,
                        y * tileSize,
                        tileSize,
                        id == 46 // power pellet
                    );

                    Pellets.Add(pelletEntity);
                    PelletSprites[pelletEntity] = pelletSprite;
                }
            }
        }
    }
}




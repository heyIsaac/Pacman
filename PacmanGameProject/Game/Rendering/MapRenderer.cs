using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.Rendering;

// classe desenha o mapa e cria pellets
public class MapRenderer
{
    public List<Pellet> Pellets { get; } = new();
    public Dictionary<Pellet, Image> PelletSprites { get; } = new();

    // metodo desenha mapa inteiro
    public void Draw(Canvas canvas, int[,] layout, int tileSize)
    {
        // limpa todos os elementos visuas canvas
        canvas.Children.Clear();
        
        // limpa lista pellets
        Pellets.Clear();
        PelletSprites.Clear();

        // desenha mapa baseado matriz layout
        DrawMap(canvas, layout, tileSize);
    }

    // Desenha cada tile do mapa
    private void DrawMap(Canvas canvas, int[,] layout, int tileSize)
    {
        // percorre linhas mapa
        for (int y = 0; y < layout.GetLength(0); y++)
        {
            // coluna
            for (int x = 0; x < layout.GetLength(1); x++)
            {
                
                int id = layout[y, x]; // ID TILE

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

                // posicionamento tile no canvas
                Canvas.SetLeft(tile, x * tileSize);
                Canvas.SetTop(tile, y * tileSize);
                Canvas.SetZIndex(tile, 0);
                
                // add tile canvas
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

                    // posicionamento pellet canvas
                    Canvas.SetLeft(pelletSprite, x * tileSize);
                    Canvas.SetTop(pelletSprite, y * tileSize);
                    Canvas.SetZIndex(pelletSprite, 1);
                    
                    // add pellet canvas
                    canvas.Children.Add(pelletSprite);

                    var pelletEntity = new Pellet(
                        x * tileSize,
                        y * tileSize,
                        tileSize,
                        id == 46 // power pellet
                    );

                    // armazena a entidade e seu sprite
                    Pellets.Add(pelletEntity);
                    PelletSprites[pelletEntity] = pelletSprite;
                }
            }
        }
    }
}




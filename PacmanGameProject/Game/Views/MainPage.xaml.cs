using Windows.System;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Rendering;

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private GameLoop _gameLoop;
    private SpriteRenderer _renderer;
    private List<Rect> _walls;

    // Lista para as moedas
    private List<Image> _coinImages = new List<Image>();

    // Tamanho 24  (mudar pra 28 depois)
    private const int TILE_SIZE = 24;

    public MainPage()
    {
        this.InitializeComponent();

        _walls = GenerateWalls();

        // Gera as moedas antes do resto
        GenerateCoins();

        _renderer = new SpriteRenderer(PacmanImage,
            new List<Image>
            {
                BlinkyImage,
                PinkyImage,
                InkyImage,
                ClydeImage
            });

        _gameLoop = new GameLoop();

        // --- POSICIONAMENTO ---

        _gameLoop.Pacman.X = 13.5 * TILE_SIZE - 10;
        _gameLoop.Pacman.Y = 23 * TILE_SIZE - 2;

        // 1. Blinky (Vermelho)
        _gameLoop.Ghosts[0].X = 16.5 * TILE_SIZE - 10;
        _gameLoop.Ghosts[0].Y = 15 * TILE_SIZE - 2;

        // 2. Pinky (Rosa)
        _gameLoop.Ghosts[1].X = 17.5 * TILE_SIZE - 10;
        _gameLoop.Ghosts[1].Y = 16 * TILE_SIZE - 2;

        // 3. Inky (Azul) 
        _gameLoop.Ghosts[2].X = 18.5 * TILE_SIZE - 10;
        _gameLoop.Ghosts[2].Y = 16 * TILE_SIZE - 2;

        // 4. Clyde (Laranja) 
        _gameLoop.Ghosts[3].X = 15.5 * TILE_SIZE - 10;
        _gameLoop.Ghosts[3].Y = 16 * TILE_SIZE - 2;

        _gameLoop.OnUpdate += Draw;
        _gameLoop.WallCheck = Collides;

        _gameLoop.Start();

        // Se quiser testar o alinhamento, descomente a linha abaixo:
         DrawDebugWalls();
    }

    private List<Rect> GenerateWalls()
    {
        var walls = new List<Rect>();
        int rows = MapData.Layout.GetLength(0);
        int cols = MapData.Layout.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileType = MapData.Layout[y, x];

                if (tileType == 1 || tileType == 9)
                {
                    walls.Add(new Rect(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
                }
            }
        }
        return walls;
    }

    // --- Método para gerar moedas ---
    private void GenerateCoins()
    {
        int rows = MapData.Layout.GetLength(0);
        int cols = MapData.Layout.GetLength(1);

        // Tamanho da moedinha visual (pequena, 4px)
        double coinSize = 4;

        // Cálculo para centralizar a moeda no bloco de 24px
        // (24 - 4) / 2 = 10px de offset
        double offset = (TILE_SIZE - coinSize) / 2;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // Se for 0 (Chão), desenha uma moeda
                if (MapData.Layout[y, x] == 0)
                {
                    Image coin = new Image();
                    coin.Source = new BitmapImage(new Uri("ms-appx:///Assets/coletaveis/moedinha.png"));
                    coin.Width = coinSize;
                    coin.Height = coinSize;

                    // Posiciona no Canvas centralizado
                    Canvas.SetLeft(coin, x * TILE_SIZE + offset);
                    Canvas.SetTop(coin, y * TILE_SIZE + offset);

                    // ZIndex 1: Fica acima do fundo (0) e abaixo dos personagens
                    Canvas.SetZIndex(coin, 1);

                    // Adiciona ao Canvas e à lista
                    GameCanvas.Children.Add(coin);
                    _coinImages.Add(coin);
                }
            }
        }
    }

    private void DrawDebugWalls()
    {
        if (DebugLayer == null) return;

        DebugLayer.Visibility = Visibility.Visible;
        foreach (var wall in _walls)
        {
            var r = new Rectangle
            {
                Width = wall.Width,
                Height = wall.Height,
                Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 255, 0, 0)),
                Stroke = new SolidColorBrush(Microsoft.UI.Colors.Red),
                StrokeThickness = 1
            };
            Canvas.SetLeft(r, wall.X);
            Canvas.SetTop(r, wall.Y);
            DebugLayer.Children.Add(r);
        }
    }

    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        GameCanvas.Focus(FocusState.Programmatic);
    }

    private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Left:
            case VirtualKey.A:
                InputManager.CurrentDirection = Direction.Left;
                break;
            case VirtualKey.Right:
            case VirtualKey.D:
                InputManager.CurrentDirection = Direction.Right;
                break;
            case VirtualKey.Up:
            case VirtualKey.W:
                InputManager.CurrentDirection = Direction.Up;
                break;
            case VirtualKey.Down:
            case VirtualKey.S:
                InputManager.CurrentDirection = Direction.Down;
                break;
        }
    }

    private void GameCanvas_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        InputManager.CurrentDirection = Direction.None;
    }

    private void Draw()
    {
        _renderer.Draw(_gameLoop.Pacman);
        _renderer.DrawGhosts(_gameLoop.Ghosts);
    }

    private bool Collides(double newX, double newY)
    {
        // Lógica de colisão
        double hitboxSize = 12;
        double offset = 8;

        // Coordenadas exatas da caixa de colisão
        double hitX = newX + offset;
        double hitY = newY + offset;

        foreach (var wall in _walls)
        {
            // Verifica se a Hitbox bate na Parede
            if (hitX < wall.X + wall.Width &&
                hitX + hitboxSize > wall.X &&
                hitY < wall.Y + wall.Height &&
                hitY + hitboxSize > wall.Y)
            {
                return true; // Bateu
            }
        }
        return false;
    }

    private void BackToMenu_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(MenuPage));
    }
}

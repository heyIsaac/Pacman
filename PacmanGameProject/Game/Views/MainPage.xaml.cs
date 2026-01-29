using Windows.System;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Rendering;

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private GameLoop _gameLoop;
    private SpriteRenderer _renderer;

    private const int TILE_SIZE = 8;
    private DateTime _startTime;

    private Dictionary<(int x, int y), Image> _pellets = new();
    private int _score = 0;
    public MainPage()
    {
        InitializeComponent();

        DrawMap();

        _renderer = new SpriteRenderer(PacmanImage,
            new List<Image>
            {
                BlinkyImage,
                PinkyImage,
                InkyImage,
                ClydeImage
            });

        _gameLoop = new GameLoop();
        _gameLoop.WallCheck = Collides;
        _gameLoop.OnUpdate += Draw;
        _startTime = DateTime.Now;
        
        _gameLoop.Ghosts[0].X = 13 * TILE_SIZE; // Blinky
        _gameLoop.Ghosts[0].Y = 13 * TILE_SIZE;

        _gameLoop.Ghosts[1].X = 14 * TILE_SIZE; // Pinky
        _gameLoop.Ghosts[1].Y = 14 * TILE_SIZE;

        _gameLoop.Ghosts[2].X = 14 * TILE_SIZE; // Inky
        _gameLoop.Ghosts[2].Y = 14 * TILE_SIZE;

        _gameLoop.Ghosts[3].X = 14 * TILE_SIZE; // Clyde
        _gameLoop.Ghosts[3].Y = 14 * TILE_SIZE;

        _gameLoop.Pacman.X = 13 * TILE_SIZE;
        _gameLoop.Pacman.Y = 23 * TILE_SIZE;

        _gameLoop.Start();
    }

    private void DrawMap()
    {
        for (int y = 0; y < MapData.Layout.GetLength(0); y++)
        {
            for (int x = 0; x < MapData.Layout.GetLength(1); x++)
            {
                int id = MapData.Layout[y, x];

                int backgroundId = id;

                // se for pastilha, fundo é chão --> 37
                if (id == 40 || id == 46)
                    backgroundId = 37;

                // desenha o fundo
                Image tile = new Image
                {
                    Source = new BitmapImage(new Uri($"ms-appx:///Assets/Tiles/{backgroundId}.png")),
                    Width = TILE_SIZE,
                    Height = TILE_SIZE
                };

                Canvas.SetLeft(tile, x * TILE_SIZE);
                Canvas.SetTop(tile, y * TILE_SIZE);
                Canvas.SetZIndex(tile, 0);
                GameCanvas.Children.Add(tile);

                // desenha a pastilha por cima
                if (id == 40 || id == 46)
                {
                    Image pellet = new Image
                    {
                        Source = new BitmapImage(new Uri($"ms-appx:///Assets/Tiles/{id}.png")),
                        Width = TILE_SIZE,
                        Height = TILE_SIZE
                    };

                    Canvas.SetLeft(pellet, x * TILE_SIZE);
                    Canvas.SetTop(pellet, y * TILE_SIZE);
                    Canvas.SetZIndex(pellet, 1);

                    GameCanvas.Children.Add(pellet);
                    _pellets[(x, y)] = pellet;
                }
            }
        }
    }


    private bool Collides(double newX, double newY)
    {
        double centerX = newX + TILE_SIZE / 2;
        double centerY = newY + TILE_SIZE / 2;

        int tileX = (int)(centerX / TILE_SIZE);
        int tileY = (int)(centerY / TILE_SIZE);

        if (tileX < 0 || tileY < 0 ||
            tileX >= 28 || tileY >= 31)
            return true;

        int tileId = MapData.Layout[tileY, tileX];

        return MapData.IsWall(tileId);
    }

    private void Draw()
    {
        _renderer.Draw(_gameLoop.Pacman);
        _renderer.DrawGhosts(_gameLoop.Ghosts);
        CheckPelletCollision();
        UpdateTime();
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
                InputManager.DesiredDirection = Direction.Left;
                break;
            case VirtualKey.Right:
            case VirtualKey.D:
                InputManager.DesiredDirection = Direction.Right;
                break;
            case VirtualKey.Up:
            case VirtualKey.W:
                InputManager.DesiredDirection = Direction.Up;
                break;
            case VirtualKey.Down:
            case VirtualKey.S:
                InputManager.DesiredDirection = Direction.Down;
                break;
        }
    }
    
    private void CheckPelletCollision()
    {
        int tileX = (int)((_gameLoop.Pacman.X + TILE_SIZE / 2) / TILE_SIZE);
        int tileY = (int)((_gameLoop.Pacman.Y + TILE_SIZE / 2) / TILE_SIZE);

        var key = (tileX, tileY);

        if (_pellets.ContainsKey(key))
        {
            // remove da tela
            GameCanvas.Children.Remove(_pellets[key]);
            _pellets.Remove(key);

            // marca o mapa como vazio 
            MapData.Layout[tileY, tileX] = 99;

            // pontua
            _score += 10;
            ScoreText.Text = $"SCORE: {_score}";
        }
    }
    
    private void UpdateTime()
    {
        TimeSpan elapsed = DateTime.Now - _startTime;
        TimeText.Text = $"TIME: {elapsed:mm\\:ss}";
    }
}

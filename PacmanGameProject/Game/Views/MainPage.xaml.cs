using Windows.System;
using Windows.Foundation;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
    private const int TILE_SIZE = 24;

    public MainPage()
    {
        this.InitializeComponent();

        _walls = GenerateWalls();

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
      
        double hitboxSize = 14;

       
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

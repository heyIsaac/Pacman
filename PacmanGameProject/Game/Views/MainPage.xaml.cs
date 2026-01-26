using System.Diagnostics;
using Windows.Foundation;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Rendering;
using Uno;

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private GameLoop _gameLoop;
    private SpriteRenderer _renderer;
    private WriteableBitmap _mapBitmap;
    private List<Rect> _walls;
    

    public MainPage()
    {
        InitializeComponent();

        _gameLoop = new GameLoop();
        _renderer = new SpriteRenderer(PacmanImage, 
            new List<Image>
            {
                BlinkyImage,
                PinkyImage,
                InkyImage,
                ClydeImage
            });

        _gameLoop.OnUpdate += Draw;
        
        _gameLoop.WallCheck = Collides;
        _gameLoop.Start();
    }

    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        GameCanvas.Focus(FocusState.Programmatic);
        
        _walls = new List<Rect>
        {
            new Rect(0, 0, 650, 20),     // topo
            new Rect(0, 556, 650, 20),   // baixo
            new Rect(0, 0, 20, 576),     // esquerda
            new Rect(630, 0, 20, 576),   // direita

            new Rect(100, 100, 200, 20), // Wall1
            new Rect(300, 200, 20, 150)  // Wall2
        };
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

            default:
                return;
        }
    }

    private void GameCanvas_KeyUp(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Left:
            case VirtualKey.A:
            case VirtualKey.Right:
            case VirtualKey.D:
            case VirtualKey.Up:
            case VirtualKey.W:
            case VirtualKey.Down:
            case VirtualKey.S:
                InputManager.CurrentDirection = Direction.None;
                break;
        }
    }

    private void Draw()
    {
        _renderer.Draw(_gameLoop.Pacman);
        _renderer.DrawGhosts(_gameLoop.Ghosts);
    }
    
    private bool Collides(double newX, double newY)
    {
        double pacSize = 22;

        foreach (var wall in _walls)
        {
            if (newX < wall.X + wall.Width &&
                newX + pacSize > wall.X &&
                newY < wall.Y + wall.Height &&
                newY + pacSize > wall.Y)
            {
                return true;
            }
        }

        return false;
    }

}




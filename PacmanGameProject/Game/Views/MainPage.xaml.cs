using System.Diagnostics;
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

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private GameLoop _gameLoop;
    private SpriteRenderer _renderer;
    private WriteableBitmap _mapBitmap;
    

    public MainPage()
    {
        InitializeComponent();

        _gameLoop = new GameLoop();
        _renderer = new SpriteRenderer(PacmanImage);

        _gameLoop.OnUpdate += Draw;
        _gameLoop.Start();
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
    }
    
}




using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Rendering;
using Windows.System;
using PacmanGameProject.Game.Services;

using NAudio.Wave;

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private GameLoop _gameLoop;
    private SpriteRenderer _renderer;

    private const int TILE_SIZE = 8;
    private DateTime _startTime;

    private Dictionary<(int x, int y), Image> _pellets = new();
    private int _score = 0;

    private int _lives = 3;
    private bool _isGameOver = false;

    private int[,] _currentLayout;

    private GameAudioService _audioService;

    public MainPage()
    {
        InitializeComponent();

        //  Clona o mapa original
        _currentLayout = (int[,])MapData.Layout.Clone();

        DrawMap();

        _renderer = new SpriteRenderer(
            PacmanImage,
            new List<Image>
            {
                BlinkyImage, PinkyImage, InkyImage, ClydeImage
            }
        );

        _gameLoop = new GameLoop();
        _gameLoop.WallCheck = Collides;
        _gameLoop.OnUpdate += Draw;

        _startTime = DateTime.Now;

        // Posições iniciais
        _gameLoop.Ghosts[0].X = 13 * TILE_SIZE;
        _gameLoop.Ghosts[0].Y = 13 * TILE_SIZE;
        _gameLoop.Ghosts[1].X = 14 * TILE_SIZE;
        _gameLoop.Ghosts[1].Y = 14 * TILE_SIZE;
        _gameLoop.Ghosts[2].X = 14 * TILE_SIZE;
        _gameLoop.Ghosts[2].Y = 14 * TILE_SIZE;
        _gameLoop.Ghosts[3].X = 14 * TILE_SIZE;
        _gameLoop.Ghosts[3].Y = 14 * TILE_SIZE;
        _gameLoop.Pacman.X = 13 * TILE_SIZE;
        _gameLoop.Pacman.Y = 23 * TILE_SIZE;
        
        _audioService = new GameAudioService();
        _audioService.SetupAudio();
        _audioService.SetupBackgroundMusic();

        _gameLoop.Start();

    }

    private void DrawMap()
    {
        for (int y = 0; y < _currentLayout.GetLength(0); y++)
        {
            for (int x = 0; x < _currentLayout.GetLength(1); x++)
            {
                int id = _currentLayout[y, x];

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
        double left = newX;
        double right = newX + TILE_SIZE - 1;
        double top = newY;
        double bottom = newY + TILE_SIZE - 1;

        int tileLeft = (int)(left / TILE_SIZE);
        int tileRight = (int)(right / TILE_SIZE);
        int tileTop = (int)(top / TILE_SIZE);
        int tileBottom = (int)(bottom / TILE_SIZE);


        if (tileLeft < 0 || tileTop < 0 ||
            tileRight >= _currentLayout.GetLength(1) ||
            tileBottom >= _currentLayout.GetLength(0))
            return true;

        if (MapData.IsWall(_currentLayout[tileTop, tileLeft])) return true;
        if (MapData.IsWall(_currentLayout[tileTop, tileRight])) return true;
        if (MapData.IsWall(_currentLayout[tileBottom, tileLeft])) return true;
        if (MapData.IsWall(_currentLayout[tileBottom, tileRight])) return true;

        return false;
    }

    private void Draw()
    {
        // Se for Game Over, para de desenhar/atualizar
        if (_isGameOver) return;

        _renderer.Draw(_gameLoop.Pacman);
        _renderer.DrawGhosts(_gameLoop.Ghosts);

        CheckPelletCollision();
        CheckGhostCollision();
        UpdateTime();

        _audioService.Update();
    }

    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        GameCanvas.Focus(FocusState.Programmatic);
    }

    private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_isGameOver) return; // Bloqueia controles se game over

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

            _currentLayout[tileY, tileX] = 99;

            // pontua
            _score += 10;
            if (ScoreText != null)
                ScoreText.Text = $"SCORE: {_score}";
            
            _audioService.PelletEaten();
            
        }
    }

    private void CheckGhostCollision()
    {

        double hitMargin = 4;

        foreach (var ghost in _gameLoop.Ghosts)
        {
            // Calcula distância entre Pacman e Fantasma
            double diffX = Math.Abs(_gameLoop.Pacman.X - ghost.X);
            double diffY = Math.Abs(_gameLoop.Pacman.Y - ghost.Y);

            // Se tocar
            if (diffX < hitMargin && diffY < hitMargin)
            {
                HandleDeath();
                break; // Sai do loop para não perder 2 vidas de uma vez
            }
        }
    }

    private void HandleDeath()
    {
        _lives--;

        if (LivesText != null)
        {
            LivesText.Text = $"LIVES: {_lives}";
        }

        //debug
        System.Diagnostics.Debug.WriteLine($"PACMAN MORREU! Vidas restantes: {_lives}");

        if (_lives <= 0)
        {
            GameOver();
        }
        else
        {
            ResetPositions();
        }
    }

    private void ResetPositions()
    {

    // Reseta Pacman
        _gameLoop.Pacman.X = 13 * TILE_SIZE;
        _gameLoop.Pacman.Y = 23 * TILE_SIZE;
        InputManager.DesiredDirection = Direction.None;

        // Reseta Fantasmas
        _gameLoop.Ghosts[0].X = 13 * TILE_SIZE; _gameLoop.Ghosts[0].Y = 13 * TILE_SIZE;
        _gameLoop.Ghosts[1].X = 14 * TILE_SIZE; _gameLoop.Ghosts[1].Y = 14 * TILE_SIZE;
        _gameLoop.Ghosts[2].X = 14 * TILE_SIZE; _gameLoop.Ghosts[2].Y = 14 * TILE_SIZE;
        _gameLoop.Ghosts[3].X = 14 * TILE_SIZE; _gameLoop.Ghosts[3].Y = 14 * TILE_SIZE;
    }

    private async void GameOver()
    {
        _isGameOver = true;

        // Para tudo
        _gameLoop.Stop();
        
        _audioService.StopAll();

        // Delay de 2 segundos antes de mostrar a tela de game over
        await System.Threading.Tasks.Task.Delay(2000);

        // Atualiza a pontuação final e mostra a tela
        if (FinalScoreText != null)
            FinalScoreText.Text = $"SCORE: {_score}";

        if (GameOverOverlay != null)
            GameOverOverlay.Visibility = Visibility.Visible;
    }
    

    private void RestartGame_Click(object sender, RoutedEventArgs e)
    {
        // Isso força o construtor a rodar de novo, resetando o mapa, bolinhas e vidas do zero.
        this.Frame.Navigate(this.GetType());
    }

    // 3. Lógica do Botão "Menu"
    private void BackToMenu_Click(object sender, RoutedEventArgs e)
    {

        this.Frame.Navigate(typeof(MenuPage));
    }
    

    private void UpdateTime()
    {
        if (TimeText == null) return;

        TimeSpan elapsed = DateTime.Now - _startTime;
        TimeText.Text = $"TIME: {elapsed:mm\\:ss}";
    }
}

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

    private List<Pellet> _pellets = new();
    private Dictionary<Pellet, Image> _pelletSprites = new();
    
    private int _score = 0;

    private int _lives = 3;
    private bool _isGameOver = false;

    private int[,] _currentLayout;

    private GameAudioService _audioService;

    private CollisionService _collisionService;
    
    private MapRenderer _mapRenderer;

    public MainPage()
    {
        InitializeComponent();

        //  Clona o mapa original
        _currentLayout = (int[,])MapData.Layout.Clone();

        _mapRenderer = new MapRenderer();
        _mapRenderer.Draw(MapCanvas, _currentLayout, TILE_SIZE);

        _pellets = _mapRenderer.Pellets;
        _pelletSprites = _mapRenderer.PelletSprites;

        _renderer = new SpriteRenderer(
            PacmanImage,
            new List<Image>
            {
                BlinkyImage, PinkyImage, InkyImage, ClydeImage
            }
        );

        _collisionService = new CollisionService(_currentLayout, TILE_SIZE);

        _gameLoop = new GameLoop();
        _gameLoop.WallCheck = _collisionService.CollidesWithWall;
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
    
    private void Draw()
    {
        if (_isGameOver) return;

        // desenha entidades
        _renderer.Draw(_gameLoop.Pacman);
        _renderer.DrawGhosts(_gameLoop.Ghosts);

        // colisões
        CheckPelletCollision();

        foreach (var ghost in _gameLoop.Ghosts)
        {
            if (_collisionService.Collides(_gameLoop.Pacman, ghost))
            {
                HandleDeath();
                break;
            }
        }

        // UI
        UpdateTime();

        // áudio
        _audioService.Update();
    }

    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        SpriteCanvas.Focus(FocusState.Programmatic);
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
        Pellet? collidedPellet = null;

        foreach (var pellet in _pellets)
        {
            if (_collisionService.Collides(_gameLoop.Pacman, pellet))
            {
                collidedPellet = pellet;
                break;
            }
        }

        if (collidedPellet != null)
        {
            // remove da tela
            MapCanvas.Children.Remove(_pelletSprites[collidedPellet]);

            _pelletSprites.Remove(collidedPellet);
            _pellets.Remove(collidedPellet);

            _score += 10;
            ScoreText.Text = $"SCORE: {_score}";

            _audioService.PelletEaten();
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

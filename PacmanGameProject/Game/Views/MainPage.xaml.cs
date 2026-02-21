using Microsoft.UI.Xaml.Input;
using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Rendering;
using Windows.System;
using PacmanGameProject.Game.Services;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private GameLoop _gameLoop;
    private bool _isGameOver = false;
    private SpriteRenderer _renderer;

    private IPelletService _pelletService;
    private IGameStateService _gameStateService;

    private const int TILE_SIZE = 8;
    private DateTime _startTime;

    private List<Pellet> _pellets = new();
    private Dictionary<Pellet, Image> _pelletSprites = new();

    private int _score = 0;
    private int[,] _currentLayout;

    private GameAudioService _audioService;
    private CollisionService _collisionService;
    private MapRenderer _mapRenderer;
    private EntitySpawnService _entitySpawnService;

    private bool _frightenedMode = false;
    private DateTime _frightenedStart;
    private const int FRIGHTENED_TIME = 7000;

    public MainPage()
    {
        InitializeComponent();

        _currentLayout = (int[,])MapData.Layout.Clone();
        _mapRenderer = new MapRenderer();
        _mapRenderer.Draw(MapCanvas, _currentLayout, TILE_SIZE);

        _pellets = _mapRenderer.Pellets;
        _pelletSprites = _mapRenderer.PelletSprites;

        _renderer = new SpriteRenderer(
            PacmanImage,
            new List<Image> { BlinkyImage, PinkyImage, InkyImage, ClydeImage }
        );

        _collisionService = new CollisionService(_currentLayout, TILE_SIZE);
        _audioService = new GameAudioService();
        _audioService.SetupAudio();
        _audioService.SetupBackgroundMusic();

        _gameStateService = new GameStateService();

        _gameLoop = new GameLoop(_collisionService);
        _gameLoop.OnUpdate += Draw;

        _pelletService = new PelletService(_pellets, _pelletSprites, _collisionService, MapCanvas, _gameStateService);
        _pelletService.OnPowerPelletEaten += ActivateFrightenedMode;

        _pelletService.OnPelletEaten += points =>
        {
            _gameStateService.AddScore(points);
            _audioService.PelletEaten();
        };

        _gameStateService.OnScoreChanged += score =>
        {
            _score = score; 
            ScoreText.Text = $"SCORE: {score}";
        };

        _gameStateService.OnLifeChanged += lives => LivesText.Text = $"LIVES: {lives}";
        _gameStateService.OnGameOver += () => GameOver();
        _gameStateService.OnScoreChanged += score => ScoreText.Text = $"SCORE: {score}";

        _gameStateService.OnGameWon += () => GameWon();

        _entitySpawnService = new EntitySpawnService();
        _entitySpawnService.SpawnEntities(_gameLoop);

        _startTime = DateTime.Now;
        _gameLoop.Start();
    }
    private async void GameWon()
    {
        if (_isGameOver) return; // Reusa a flag para travar os controles
        _isGameOver = true;
        
        _gameLoop.Stop();
        
        // Para a música de fundo e toca um som de vitória se tiver
        _audioService.StopAll();
        // _audioService.PlayVictorySound(); 

        await System.Threading.Tasks.Task.Delay(1000); // Pausa de 1 segundo

        // Salva a pontuação 
        if (_score > 0)
        {
            PacmanGameProject.Game.Services.ScoreService.SaveScore("player", _score);
        }

        // Mostra a tela de vitória
        if (VictoryScoreText != null) VictoryScoreText.Text = $"SCORE: {_score}";
        if (VictoryOverlay != null) VictoryOverlay.Visibility = Visibility.Visible;
    }

    private void Draw()
    {
        if (_isGameOver) return;

        _renderer.Draw(_gameLoop.Pacman);

        double remainingTime = 0;
        if (_frightenedMode)
        {
            var elapsed = DateTime.Now - _frightenedStart;
            remainingTime = FRIGHTENED_TIME - elapsed.TotalMilliseconds;
        }

        _renderer.DrawGhosts(_gameLoop.Ghosts, remainingTime);

        _pelletService.CheckCollision(_gameLoop.Pacman);

        CheckGhostCollisions();

        // Fim do modo frightened
        if (_frightenedMode)
        {
            var elapsed = DateTime.Now - _frightenedStart;
            if (elapsed.TotalMilliseconds > FRIGHTENED_TIME)
            {
                _frightenedMode = false;

                // Volta música normal
                _audioService.ResumeNormalMusic();

                foreach (var ghost in _gameLoop.Ghosts)
                {
                    if (ghost.CurrentState == GhostState.Frightened)
                        ghost.CurrentState = GhostState.Scatter;
                }
            }
        }

        UpdateTime();
        _audioService.Update();
    }

    private void CheckGhostCollisions()
    {
        foreach (var ghost in _gameLoop.Ghosts)
        {
            if (_collisionService.Collides(_gameLoop.Pacman, ghost))
            {
                if (ghost.CurrentState == GhostState.Frightened)
                {
                    // Pacman come Fantasma
                    ghost.SendToHouse();
                    _gameStateService.AddScore(200);

                    // Toca som de comer fantasma
                    _audioService.PlayEatGhost();
                }
                else if (ghost.CurrentState != GhostState.Eaten && ghost.CurrentState != GhostState.InHouse)
                {
                    // Pacman Morre
                    HandlePacmanDeath();
                }
                break;
            }
        }
    }

    private void HandlePacmanDeath()
    {
        _gameStateService.PacmanDied();
        if (!_isGameOver)
        {
            _entitySpawnService.ResetPositions(_gameLoop);
        }
    }

    private void ActivateFrightenedMode()
    {
        _frightenedMode = true;
        _frightenedStart = DateTime.Now;

        // Toca música de tensão
        _audioService.PlayFrightenedMusic();

        foreach (var ghost in _gameLoop.Ghosts)
        {
            if (ghost.CurrentState != GhostState.InHouse && ghost.CurrentState != GhostState.Eaten)
            {
                ghost.CurrentState = GhostState.Frightened;
                ghost.ForceReverseDirection();
            }
        }
    }

    private void GameCanvas_Loaded(object sender, RoutedEventArgs e) { SpriteCanvas.Focus(FocusState.Programmatic); }

    private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_isGameOver) return;
        switch (e.Key)
        {
            case VirtualKey.Left: case VirtualKey.A: InputManager.DesiredDirection = Direction.Left; break;
            case VirtualKey.Right: case VirtualKey.D: InputManager.DesiredDirection = Direction.Right; break;
            case VirtualKey.Up: case VirtualKey.W: InputManager.DesiredDirection = Direction.Up; break;
            case VirtualKey.Down: case VirtualKey.S: InputManager.DesiredDirection = Direction.Down; break;
        }
    }

    private void RestartGame_Click(object sender, RoutedEventArgs e)
    {
        _audioService.StopAll();
        _gameLoop.Stop();
        this.Frame.Navigate(this.GetType());
    }

    private void BackToMenu_Click(object sender, RoutedEventArgs e)
    {
        _gameLoop.Stop();
        _audioService.StopAll();
        _audioService.Dispose();
        this.Frame.Navigate(typeof(MenuPage));
    }

    private void UpdateTime()
    {
        if (TimeText == null) return;
        TimeSpan elapsed = DateTime.Now - _startTime;
        TimeText.Text = $"TIME: {elapsed:mm\\:ss}";
    }

    private async void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        _gameLoop.Stop();
        _audioService.PlayDeath();
        await System.Threading.Tasks.Task.Delay(2000);

        if (_score > 0)
        {
            // Salva a pontuação
            ScoreService.SaveScore("player", _score);
        }

        if (FinalScoreText != null) FinalScoreText.Text = $"SCORE: {_score}";
        if (GameOverOverlay != null) GameOverOverlay.Visibility = Visibility.Visible;
    }
}

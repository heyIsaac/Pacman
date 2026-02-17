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

    // O Serviço de Spawn
    private EntitySpawnService _entitySpawnService;

    private bool _frightenedMode = false;
    private DateTime _frightenedStart;
    private const int FRIGHTENED_TIME = 7000;

    public MainPage()
    {
        InitializeComponent();

        // 1. Mapa
        _currentLayout = (int[,])MapData.Layout.Clone();
        _mapRenderer = new MapRenderer();
        _mapRenderer.Draw(MapCanvas, _currentLayout, TILE_SIZE);

        _pellets = _mapRenderer.Pellets;
        _pelletSprites = _mapRenderer.PelletSprites;

        // 2. Renderer
        _renderer = new SpriteRenderer(
            PacmanImage,
            new List<Image> { BlinkyImage, PinkyImage, InkyImage, ClydeImage }
        );

        // 3. Serviços Básicos
        _collisionService = new CollisionService(_currentLayout, TILE_SIZE);
        _audioService = new GameAudioService();
        _audioService.SetupAudio();
        _audioService.SetupBackgroundMusic();

        _gameStateService = new GameStateService();

        // 4. Game Loop
        _gameLoop = new GameLoop(_collisionService);
        _gameLoop.OnUpdate += Draw;

        // 5. Pellets
        _pelletService = new PelletService(_pellets, _pelletSprites, _collisionService, MapCanvas, _gameStateService);
        _pelletService.OnPowerPelletEaten += ActivateFrightenedMode;

        _pelletService.OnPelletEaten += points =>
        {
            _gameStateService.AddScore(points);
            _audioService.PelletEaten();
        };

        // 6. UI
        _gameStateService.OnLifeChanged += lives => LivesText.Text = $"LIVES: {lives}";
        _gameStateService.OnGameOver += () => GameOver();
        _gameStateService.OnScoreChanged += score => ScoreText.Text = $"SCORE: {score}";

        // 7. SPAWN INICIAL (Usando o Serviço)
        _entitySpawnService = new EntitySpawnService();
        _entitySpawnService.SpawnEntities(_gameLoop);

        // 8. Start
        _startTime = DateTime.Now;
        _gameLoop.Start();
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
        UpdateFrightenedMode();

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
                    ghost.SendToHouse();
                    _gameStateService.AddScore(200);
                }
                else if (ghost.CurrentState != GhostState.Eaten && ghost.CurrentState != GhostState.InHouse)
                {
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

    private void UpdateFrightenedMode()
    {
        if (_frightenedMode)
        {
            var elapsed = DateTime.Now - _frightenedStart;
            if (elapsed.TotalMilliseconds > FRIGHTENED_TIME)
            {
                _frightenedMode = false;
                foreach (var ghost in _gameLoop.Ghosts)
                {
                    if (ghost.CurrentState == GhostState.Frightened)
                        ghost.CurrentState = GhostState.Scatter;
                }
            }
        }
    }

    private void ActivateFrightenedMode()
    {
        _frightenedMode = true;
        _frightenedStart = DateTime.Now;

        foreach (var ghost in _gameLoop.Ghosts)
        {
            // Só afeta quem está ativo no labirinto
            if (ghost.CurrentState != GhostState.InHouse &&
                ghost.CurrentState != GhostState.Eaten)
            {
                ghost.CurrentState = GhostState.Frightened;

                // Isso faz com que eles "fujam" do Pacman instantaneamente
                ghost.ForceReverseDirection();
            }
        }
    }

    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        SpriteCanvas.Focus(FocusState.Programmatic);
    }

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
        if (FinalScoreText != null) FinalScoreText.Text = $"SCORE: {_score}";
        if (GameOverOverlay != null) GameOverOverlay.Visibility = Visibility.Visible;
    }
}

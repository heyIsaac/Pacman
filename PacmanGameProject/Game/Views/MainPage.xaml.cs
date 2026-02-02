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
        
        _entitySpawnService = new EntitySpawnService();
        _entitySpawnService.SpawnEntities(_gameLoop);
        
        _audioService = new GameAudioService();
        _audioService.SetupAudio();
        _audioService.SetupBackgroundMusic();
        
        _gameStateService = new GameStateService();
        
        _pelletService = new PelletService( _pellets, _pelletSprites, _collisionService, MapCanvas, _gameStateService);
        
        _pelletService.OnPelletEaten += points =>
        {
            _gameStateService.AddScore(points);
            _audioService.PelletEaten();
        };
        
        _gameStateService.OnLifeChanged += lives =>
        {
            LivesText.Text = $"LIVES: {lives}";
        };

        _gameStateService.OnGameOver += () =>
        {
            GameOver();
        };
        
        _gameStateService.OnScoreChanged += score =>
        {
            ScoreText.Text = $"SCORE: {score}";
        };

        _gameLoop.Start();

    }
    
    private void Draw()
    {
        if (_isGameOver) return;

        // desenha entidades
        _renderer.Draw(_gameLoop.Pacman);
        _renderer.DrawGhosts(_gameLoop.Ghosts);

        // colisões
        _pelletService.CheckCollision(_gameLoop.Pacman);

        foreach (var ghost in _gameLoop.Ghosts)
        {
            if (_collisionService.Collides(_gameLoop.Pacman, ghost))
            {
                _gameStateService.PacmanDied();
                if (!_isGameOver)
                      _entitySpawnService.ResetPositions(_gameLoop);
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

        if (_isGameOver) return;
        
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

    private void RestartGame_Click(object sender, RoutedEventArgs e)
    {
        _audioService.StopAll();
        _gameLoop.Stop();
        
        // Isso força o construtor a rodar de novo, resetando o mapa, bolinhas e vidas do zero.
        this.Frame.Navigate(this.GetType());
    }

    // 3. Lógica do Botão "Menu"
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

        // Para tudo
        _gameLoop.Stop();
        
        _audioService.PlayDeath();

        // Delay de 2 segundos antes de mostrar a tela de game over
        await System.Threading.Tasks.Task.Delay(2000);

        _audioService.PlayDeath();
        
        // Atualiza a pontuação final e mostra a tela
        if (FinalScoreText != null)
            FinalScoreText.Text = $"SCORE: {_score}";

        if (GameOverOverlay != null)
            GameOverOverlay.Visibility = Visibility.Visible;
    }
}

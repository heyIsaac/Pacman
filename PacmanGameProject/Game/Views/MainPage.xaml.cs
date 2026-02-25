using Microsoft.UI.Xaml.Input;
using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Rendering;
using Windows.System;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Services;

namespace PacmanGameProject.Game.Views;

// View principal jogo
public sealed partial class MainPage : Page
{
    // centraliza serviços jogo
    private readonly GameInitializerService _game = new();
    private MapRenderer _mapRenderer; // desenha mapa

    private bool _isGameOver = false;
    private int _score = 0; 
    private int[,] _currentLayout;
    private DateTime _startTime;
    private const int TILE_SIZE = 8;

    public MainPage()
    {
        InitializeComponent();

        _currentLayout = (int[,])MapData.Layout.Clone(); // clona layout para nao alterar o original qnd coletar pellets
        
        // Desenha mapa e gera pellets
        _mapRenderer = new MapRenderer();
        _mapRenderer.Draw(MapCanvas, _currentLayout, TILE_SIZE);

        _game.Initialize(
            mapCanvas:          MapCanvas,
            pacmanImage:        PacmanImage,
            ghostImages:        new List<Image> { BlinkyImage, PinkyImage, InkyImage, ClydeImage },
            layout:             _currentLayout,
            pellets:            _mapRenderer.Pellets,
            pelletSprites:      _mapRenderer.PelletSprites,
            activateFrightened: ActivateFrightenedMode,
            onPelletEaten:      points => _game.GameStateService.AddScore(points),
            onScoreChanged:     score  => { _score = score; ScoreText.Text = $"SCORE: {score}"; },
            onLifeChanged:      lives  => LivesText.Text = $"LIVES: {lives}",
            onGameOver:         () => GameOver(),
            onGameWon:          () => GameWon(),
            isGameOver:         () => _isGameOver
        );

        // Evento de att do GameLoop para metodo Draw
        _game.GameLoop.OnUpdate += Draw;
        _startTime = DateTime.Now;
        _game.GameLoop.Start();
    }

    // executa cada frame GameLoop e renderiza e verifica colisoes
    private void Draw()
    {
        if (_isGameOver) return;

        // Renderiza pacman e fantasmas
        _game.Renderer.Draw(_game.GameLoop.Pacman);
        _game.Renderer.DrawGhosts(_game.GameLoop.Ghosts, _game.FrightenedService.RemainingTime);

        // Checa colisao
        _game.PelletService.CheckCollision(_game.GameLoop.Pacman);
        _game.GhostCollisionService.Check(_game.GameLoop.Pacman, _game.GameLoop.Ghosts);

        _game.FrightenedService.Update(_game.GameLoop.Ghosts); // att timer modo Frightened

        UpdateTime();
        _game.AudioService.Update();
    }

    // coloca fantasmas modo Frightened e troca música
    private void ActivateFrightenedMode()
    {
        _game.FrightenedService.Activate(_game.GameLoop.Ghosts);
        _game.AudioService.PlayFrightenedMusic();
    }

    // Exibe tela de vitoria
    private async void GameWon()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        _game.GameLoop.Stop();
        _game.AudioService.StopAll();
        await Task.Delay(1000);
        if (_score > 0) ScoreService.SaveScore("player", _score);
        if (VictoryScoreText != null) VictoryScoreText.Text = $"SCORE: {_score}";
        if (VictoryOverlay != null) VictoryOverlay.Visibility = Visibility.Visible;
    }

    // Exibe tela de Game Over
    private async void GameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;
        _game.GameLoop.Stop();
        _game.AudioService.PlayDeath();
        await Task.Delay(2000);
        if (_score > 0) ScoreService.SaveScore("player", _score);
        if (FinalScoreText != null) FinalScoreText.Text = $"SCORE: {_score}";
        if (GameOverOverlay != null) GameOverOverlay.Visibility = Visibility.Visible;
    }

    // Foca nos canvas sprites
    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        SpriteCanvas.Focus(FocusState.Programmatic);
    }

    // Captura de Input do teclado
    private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_isGameOver) return;
        switch (e.Key)
        {
            case VirtualKey.Left:  case VirtualKey.A: InputManager.DesiredDirection = Direction.Left;  break;
            case VirtualKey.Right: case VirtualKey.D: InputManager.DesiredDirection = Direction.Right; break;
            case VirtualKey.Up:    case VirtualKey.W: InputManager.DesiredDirection = Direction.Up;    break;
            case VirtualKey.Down:  case VirtualKey.S: InputManager.DesiredDirection = Direction.Down;  break;
        }
    }

    // Reinicia o jogo
    private void RestartGame_Click(object sender, RoutedEventArgs e)
    {
        _game.AudioService.StopAll();
        _game.GameLoop.Stop();
        this.Frame.Navigate(this.GetType());
    }

    // volta menu principal parando o jogo e áudio
    private void BackToMenu_Click(object sender, RoutedEventArgs e)
    {
        _game.GameLoop.Stop();
        _game.AudioService.StopAll();
        _game.AudioService.Dispose(); 
        this.Frame.Navigate(typeof(MenuPage));
    }

    // att cronometro exibido no HUD a cada frame
    private void UpdateTime()
    {
        if (TimeText == null) return;
        TimeSpan elapsed = DateTime.Now - _startTime;
        TimeText.Text = $"TIME: {elapsed:mm\\:ss}";
    }
}

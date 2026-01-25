using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;

namespace PacmanGameProject.Game.Views;

public sealed partial class MainPage : Page
{
    private readonly Pacman _pacman = new Pacman();
    private readonly DispatcherTimer _timer = new DispatcherTimer();

    public MainPage()
    {
        this.InitializeComponent();

        _pacman.X = 100;
        _pacman.Y = 100;

        this.Loaded += MainPage_Loaded;

        // Configura timer
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += GameLoop;
        _timer.Start();
    }

    private void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Focar no Canvas para capturar teclado
        GameCanvas.Focus(FocusState.Programmatic);

        // Registrar KeyDown no próprio Page
        this.KeyDown += MainPage_KeyDown;
    }

    private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Left: InputManager.CurrentDirection = Direction.Left; break;
            case Windows.System.VirtualKey.Right: InputManager.CurrentDirection = Direction.Right; break;
            case Windows.System.VirtualKey.Up: InputManager.CurrentDirection = Direction.Up; break;
            case Windows.System.VirtualKey.Down: InputManager.CurrentDirection = Direction.Down; break;
        }
    }

    private void GameLoop(object sender, object e)
    {
        // Atualiza posição do Pacman
        _pacman.UpdateDirection(InputManager.CurrentDirection);

        // Limites do Canvas
        _pacman.X = Math.Max(0, Math.Min(GameCanvas.ActualWidth - PacmanImage.Width, _pacman.X));
        _pacman.Y = Math.Max(0, Math.Min(GameCanvas.ActualHeight - PacmanImage.Height, _pacman.Y));

        // Desenha
        Canvas.SetLeft(PacmanImage, _pacman.X);
        Canvas.SetTop(PacmanImage, _pacman.Y);
    }
}

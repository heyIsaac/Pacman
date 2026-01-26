using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Input;

namespace PacmanGameProject.Game.Engine;

public class GameLoop
{
    private readonly DispatcherTimer _timer;
    public Pacman Pacman { get; } = new();
    public Func<double, double, bool>? WallCheck;
    
    public event Action? OnUpdate;

    public GameLoop()
    {
        Pacman.X = 100;
        Pacman.Y = 200;

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Update;
    }

    public void Start() => _timer.Start();

    private void Update(object? sender, object e)
    {
        Pacman.Move(InputManager.CurrentDirection, WallCheck);
        OnUpdate?.Invoke();
    }

}

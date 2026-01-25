using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Input;

namespace PacmanGameProject.Game.Engine;

public class GameLoop
{
    private readonly DispatcherTimer _timer;
    public Pacman Pacman { get; } = new();

    public GameLoop()
    {
        Pacman.X = 100;
        Pacman.Y = 100;

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Update;
    }

    public void Start() => _timer.Start();

    private void Update(object? sender, object e)
    {
        Pacman.UpdateDirection(InputManager.CurrentDirection);
    }

}

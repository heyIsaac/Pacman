using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;

namespace PacmanGameProject.Game.Engine;

public class GameLoop
{
    private readonly DispatcherTimer _timer;
    public Pacman Pacman { get; } = new();
    public List<Ghost> Ghosts { get;  } = new();
    public Func<double, double, bool>? WallCheck;
    
    public event Action? OnUpdate;

    public GameLoop()
    {

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Update;
        
        Ghosts.Add(new Ghost(GhostType.Blinky, 0, 0));
        Ghosts.Add(new Ghost(GhostType.Pinky, 0, 0));
        Ghosts.Add(new Ghost(GhostType.Inky, 0, 0));
        Ghosts.Add(new Ghost(GhostType.Clyde, 0, 0));
    }
    
    public void Start() => _timer.Start();

    private void Update(object? sender, object e)
    {
        Pacman.DesiredDirection = InputManager.DesiredDirection;
        Pacman.Update(WallCheck);


        foreach (var ghost in Ghosts)
            ghost.Update(Pacman, WallCheck);

        OnUpdate?.Invoke();
    }

}

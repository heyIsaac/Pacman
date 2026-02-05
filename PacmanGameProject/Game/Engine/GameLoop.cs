using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Services;

namespace PacmanGameProject.Game.Engine;

public class GameLoop
{
    // Timer que vai controlar a atualização do jogo
    private readonly DispatcherTimer _timer;
    
    public Pacman Pacman { get; } = new();
    public List<Ghost> Ghosts { get;  } = new();
    public Func<double, double, bool>? WallCheck;
    
    private readonly CollisionService _collisionService;

    
    // Evento disparado a cada atualização do jogo
    public event Action? OnUpdate;

    public GameLoop(CollisionService collisionService)
    {
         _collisionService = collisionService;
         
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16); // aqui representa 60 FPS
        _timer.Tick += Update;
        
        // Criação dos fantasmas
        Ghosts.Add(new Ghost(GhostType.Blinky, 14 * 8, 14 * 8));
        Ghosts.Add(new Ghost(GhostType.Pinky, 13 * 8, 15 * 8));
        Ghosts.Add(new Ghost(GhostType.Inky, 15 * 8, 15 * 8));
        Ghosts.Add(new Ghost(GhostType.Clyde, 14 * 8, 16 * 8));
        
        Ghosts[0].InHouse = false;
    }
    
    // inicia o loop do jogo
    public void Start() => _timer.Start();

    // para o loop do jogo
    public void Stop() => _timer.Stop();

    // Método que vai ser chamado a cada frame
    private void Update(object? sender, object e)
    {
        // Atualiza a direção desejada do pacman
        Pacman.DesiredDirection = InputManager.DesiredDirection;
        
        // atualiza a lógica do pacman
        Pacman.Update((x, y) => _collisionService.CollidesWithWall(Pacman, x, y));

        // atualiza cada fantasma
        foreach (var ghost in Ghosts)
            ghost.Update(Pacman, (x, y) => _collisionService.CollidesWithWall(ghost, x, y));

        // dispara evento para a interface redesenhar
        OnUpdate?.Invoke();
    }

}

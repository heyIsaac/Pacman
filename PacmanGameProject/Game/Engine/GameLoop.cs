using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;

namespace PacmanGameProject.Game.Engine;

public class GameLoop
{
    // Timer que vai controlar a atualização do jogo
    private readonly DispatcherTimer _timer;
    
    public Pacman Pacman { get; } = new();
    public List<Ghost> Ghosts { get;  } = new();
    public Func<double, double, bool>? WallCheck;
    
    // Evento disparado a cada atualização do jogo
    public event Action? OnUpdate;

    public GameLoop()
    {

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16); // aqui representa 60 FPS
        _timer.Tick += Update;
        
        // Criação dos fantasmas
        Ghosts.Add(new Ghost(GhostType.Blinky, 0, 0));
        Ghosts.Add(new Ghost(GhostType.Pinky, 0, 0));
        Ghosts.Add(new Ghost(GhostType.Inky, 0, 0));
        Ghosts.Add(new Ghost(GhostType.Clyde, 0, 0));
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
        Pacman.Update(WallCheck);

        // atualiza cada fantasma
        foreach (var ghost in Ghosts)
            ghost.Update(Pacman, WallCheck);

        // dispara evento para a interface redesenhar
        OnUpdate?.Invoke();
    }

}

using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Services;

namespace PacmanGameProject.Game.Engine;

public class GameLoop
{
    private readonly DispatcherTimer _timer;
    private readonly CollisionService _collisionService;

    public Pacman Pacman { get; } = new();
    public List<Ghost> Ghosts { get; } = new();

    public event Action? OnUpdate;

    private const int MAP_COLS = 28;
    private const int MAP_ROWS = 31;
    
    private readonly GhostWaveService _waveService = new();

    public GameLoop(CollisionService collisionService)
    {
        _collisionService = collisionService;

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        _timer.Tick += Update;

        InitializeGhosts();
    }

    private void InitializeGhosts()
    {
        // Posições baseadas no Grid (Tile * 8 pixels)
        Ghosts.Add(new Ghost(GhostType.Blinky, 13 * 8, 11 * 8)); // Fora (Topo - já liberado no início)
        Ghosts.Add(new Ghost(GhostType.Pinky, 13 * 8, 14 * 8));  // Centro
        Ghosts.Add(new Ghost(GhostType.Inky, 11 * 8, 14 * 8));   // Esquerda
        Ghosts.Add(new Ghost(GhostType.Clyde, 15 * 8, 14 * 8));  // Direita

        foreach (var g in Ghosts)
        {
            g.CurrentState = g.Type == GhostType.Blinky
                ? GhostState.Scatter
                : GhostState.InHouse;
        }

        // Blinky começa fora, então marcamos como "já saiu"
        var blinky = Ghosts.First(g => g.Type == GhostType.Blinky);
        blinky.MarkAsExited();
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();

    private void Update(object? sender, object e)
    {
        const double deltaTime = 0.016;

        _waveService.Update(deltaTime, Ghosts);

        Pacman.DesiredDirection = InputManager.DesiredDirection;
        Pacman.Update((x, y) => _collisionService.CollidesWithWall(Pacman, x, y));

        var blinky = Ghosts.First(g => g.Type == GhostType.Blinky);

        foreach (var ghost in Ghosts)
            ghost.Update(Pacman, blinky, _waveService.GlobalGhostState, MakeWallCheck(ghost));

        OnUpdate?.Invoke();
    }

    // função de colisão específica para o fantasma.
    // Gerencia a lógica da Porta da Casa: fantasmas normais são bloqueados e fantasmas comidos (olhos) podem passar.
    private static Func<int, int, bool> MakeWallCheck(Ghost ghost)
    {
        return (gridX, gridY) =>
        {
            // Se for a linha 14 (Túnel), permitimos X negativo ou X > Colunas
            if (gridY == 14)
            {
                // Se saiu do mapa na linha 14, NÃO é parede (retorna false)
                if (gridX < 0 || gridX >= MAP_COLS) return false;
            }
            else
            {
                // Em outras linhas, sair do mapa é parede
                if (gridX < 0 || gridY < 0 || gridX >= MAP_COLS || gridY >= MAP_ROWS)
                    return true;
            }

            // Regra da Porta (Mantida)
            if (gridY == Ghost.DOOR_ROW &&
                gridX >= Ghost.DOOR_COL_MIN &&
                gridX <= Ghost.DOOR_COL_MAX)
            {
                return ghost.CurrentState != GhostState.Eaten;
            }

            return MapData.IsWall(MapData.Layout[gridY, gridX]);
        };
    }
    

    public void ResetRoundTimer() => _waveService.Reset();
}

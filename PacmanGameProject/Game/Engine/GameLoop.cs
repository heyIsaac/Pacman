using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Services;

namespace PacmanGameProject.Game.Engine;

// Responsável pelo orquestamento do jogo (tick, entidades)
public class GameLoop
{
    private readonly DispatcherTimer _timer;
    private readonly CollisionService _collisionService;

    // principais entidades
    public Pacman Pacman { get; } = new();
    public List<Ghost> Ghosts { get; } = new();

    // Evento disparado após cada atualização -> desenha
    public event Action? OnUpdate;

    // Dimensões do mapa
    private const int MAP_COLS = 28;
    private const int MAP_ROWS = 31;
    
    // Gerencia as ondas de Scatter e Chase e libera fantasmas da casa
    private readonly GhostWaveService _waveService = new();

    public GameLoop(CollisionService collisionService)
    {
        _collisionService = collisionService;
        
        // config timer
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

    // executa a cada frame pelo DispatcherTimer
    private void Update(object? sender, object e)
    {
        const double deltaTime = 0.016; // aproximadamente 60 FPS em segundos

        // att ondas de comportamento e liberação fantasma
        _waveService.Update(deltaTime, Ghosts);

        // Att input e movimento pacman
        Pacman.DesiredDirection = InputManager.DesiredDirection;
        Pacman.Update((x, y) => _collisionService.CollidesWithWall(Pacman, x, y));

        // Passa blinky para outros fantasmas, pois o Inky necessita da sua posição
        var blinky = Ghosts.First(g => g.Type == GhostType.Blinky);

        // Att cada fantasma com sua própria função de colisão
        foreach (var ghost in Ghosts)
            ghost.Update(Pacman, blinky, _waveService.GlobalGhostState, MakeWallCheck(ghost));

        // Notifica renderer que frame foi att
        OnUpdate?.Invoke();
    }

    // Cria uma função que verifica colisão específica para cada fantasma
    // Gerencia a lógica da Porta da Casa: fantasmas normais são bloqueados e fantasmas modo Eaten podem passar.
    private static Func<int, int, bool> MakeWallCheck(Ghost ghost)
    {
        return (gridX, gridY) =>
        {
            // Se for a linha 14 (Túnel), permitimos X negativo ou X > Colunas
            if (gridY == 14)
            {
                if (gridX < 0 || gridX >= MAP_COLS) return false;
            }
            else
            {
                // Sair do mapa é parede
                if (gridX < 0 || gridY < 0 || gridX >= MAP_COLS || gridY >= MAP_ROWS)
                    return true;
            }

            // Regra da Porta -> Eaten passa, os outros não
            if (gridY == Ghost.DOOR_ROW &&
                gridX >= Ghost.DOOR_COL_MIN &&
                gridX <= Ghost.DOOR_COL_MAX)
            {
                return ghost.CurrentState != GhostState.Eaten;
            }

            // Verifica se tile é parede
            return MapData.IsWall(MapData.Layout[gridY, gridX]);
        };
    }
    
    // Reset timer das ondas após morte Pacman
    public void ResetRoundTimer() => _waveService.Reset();
}

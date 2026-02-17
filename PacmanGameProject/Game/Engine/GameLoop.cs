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

    // Configuração das ondas de comportamento (Scatter/Chase) para o Nível 1
    private readonly List<(GhostState Mode, double Duration)> _level1Waves = new()
    {
        (GhostState.Scatter, 7),
        (GhostState.Chase,   20),
        (GhostState.Scatter, 7),
        (GhostState.Chase,   20),
        (GhostState.Scatter, 5),
        (GhostState.Chase,   20),
        (GhostState.Scatter, 5),
        (GhostState.Chase,   99999) // Chase permanente no final
    };

    private int _currentWaveIndex = 0;
    private double _waveTimerSeconds = 0;
    private GhostState _globalGhostState = GhostState.Scatter;

    // Tempo (em segundos) que cada fantasma espera antes de sair no início do jogo
    private readonly Dictionary<GhostType, double> _releaseDelay = new()
    {
        { GhostType.Blinky, 0.0  },
        { GhostType.Pinky,  4.0  },
        { GhostType.Inky,   8.0  },
        { GhostType.Clyde,  12.0 },
    };
    private double _gameTimeSeconds = 0;

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
        _gameTimeSeconds += deltaTime;

        UpdateGhostReleases(deltaTime);
        UpdateGlobalGhostState(deltaTime);

        Pacman.DesiredDirection = InputManager.DesiredDirection;
        Pacman.Update((x, y) => _collisionService.CollidesWithWall(Pacman, x, y));

        var blinky = Ghosts.First(g => g.Type == GhostType.Blinky);

        foreach (var ghost in Ghosts)
            ghost.Update(Pacman, blinky, _globalGhostState, MakeWallCheck(ghost));

        OnUpdate?.Invoke();
    }

    // função de colisão específica para o fantasma.
    // Gerencia a lógica da Porta da Casa: fantasmas normais são bloqueados e fantasmas comidos (olhos) podem passar.
    private static Func<int, int, bool> MakeWallCheck(Ghost ghost)
    {
        return (gridX, gridY) =>
        {
            // Proteção contra OutOfBounds
            if (gridX < 0 || gridY < 0 || gridX >= MAP_COLS || gridY >= MAP_ROWS)
                return true;

            // Regra da Porta (Linha 12, Colunas 13-14)
            if (gridY == Ghost.DOOR_ROW &&
                gridX >= Ghost.DOOR_COL_MIN &&
                gridX <= Ghost.DOOR_COL_MAX)
            {
                return ghost.CurrentState != GhostState.Eaten;
            }

            return MapData.IsWall(MapData.Layout[gridY, gridX]);
        };
    }

    private void UpdateGhostReleases(double deltaTime)
    {
        foreach (var ghost in Ghosts)
        {
            if (ghost.CurrentState != GhostState.InHouse) continue;

            // Prioridade 1: Verifica penalidade de respawn (se morreu recentemente)
            if (ghost.RespawnTimeRemaining > 0)
            {
                ghost.RespawnTimeRemaining -= deltaTime;
                continue; // Mantém na casa até o timer zerar
            }

            // Prioridade 2: Liberação padrão de início de jogo
            if (_releaseDelay.TryGetValue(ghost.Type, out double delay) &&
                _gameTimeSeconds >= delay)
            {
                ghost.Release();
            }
        }
    }

    private void UpdateGlobalGhostState(double deltaTime)
    {
        if (_currentWaveIndex >= _level1Waves.Count) return;

        _waveTimerSeconds += deltaTime;

        if (_waveTimerSeconds >= _level1Waves[_currentWaveIndex].Duration)
        {
            _currentWaveIndex++;
            _waveTimerSeconds = 0;

            if (_currentWaveIndex < _level1Waves.Count)
            {
                _globalGhostState = _level1Waves[_currentWaveIndex].Mode;

                foreach (var ghost in Ghosts)
                {
                    // Força inversão de direção na troca de onda (Scatter <-> Chase),
                    // exceto se o fantasma estiver ocupado (Casa, Morto, Assustado ou Saindo)
                    if (ghost.CurrentState != GhostState.InHouse &&
                        ghost.CurrentState != GhostState.Eaten &&
                        ghost.CurrentState != GhostState.Frightened &&
                        !ghost.IsExiting)
                    {
                        ghost.ForceReverseDirection();
                    }
                }
            }
        }
    }

    public void ResetRoundTimer()
    {
        _gameTimeSeconds = 0;
        _currentWaveIndex = 0;
        _waveTimerSeconds = 0;
        _globalGhostState = GhostState.Scatter;
    }
}

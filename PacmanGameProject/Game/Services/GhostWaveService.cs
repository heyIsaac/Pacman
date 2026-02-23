using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Services;

// Classe responsável em gerenciar as ondas de comportamento dos fantasmas
public class GhostWaveService
{
    
    // Sequencia ondas
    private readonly List<(GhostState Mode, double Duration)> _waves = new()
    {
        (GhostState.Scatter, 7),
        (GhostState.Chase, 20),
        (GhostState.Scatter, 7),
        (GhostState.Chase, 20),
        (GhostState.Scatter, 5),
        (GhostState.Chase, 20),
        (GhostState.Scatter, 5),
        (GhostState.Chase, 99999)
    };
    
    // Delay de liberação de cada fantasma
    private readonly Dictionary<GhostType, double> _releaseDelay = new()
    {
        { GhostType.Blinky, 0.0  },
        { GhostType.Pinky,  4.0  },
        { GhostType.Inky,   8.0  },
        { GhostType.Clyde,  12.0 },
    };

    private int _currentWaveIndex = 0;
    private double _waveTimer = 0; // tempo decorrido
    private double _gameTime = 0; // tempo total jogo

    public GhostState GlobalGhostState { get; private set; } = GhostState.Scatter;

    // att cada frame pelo GameLoop
    public void Update(double deltaTime, List<Ghost> ghosts)
    {
        _gameTime += deltaTime;

        UpdateReleases(deltaTime, ghosts);
        UpdateWaves(deltaTime, ghosts);
    }

    // Libera fantasmas da casa conforme o tempo do jogo avança
    private void UpdateReleases(double deltaTime, List<Ghost> ghosts)
    {
        foreach (var ghost in ghosts)
        {
            if (ghost.CurrentState != GhostState.InHouse) continue;

            // Penalidade de respawn
            if (ghost.RespawnTimeRemaining > 0)
            {
                ghost.RespawnTimeRemaining -= deltaTime;
                continue;
            }

            // Liberação padrão
            if (_releaseDelay.TryGetValue(ghost.Type, out double delay) &&
                _gameTime >= delay)
            {
                ghost.Release();
            }
        }
    }

    // Avança ondas Scatter e Chase conforme timer da onda atual expira
    private void UpdateWaves(double deltaTime, List<Ghost> ghosts)
    {
        // Se passou por todas ondas, permanece no ultimo estado
        if (_currentWaveIndex >= _waves.Count) return;

        _waveTimer += deltaTime;
        
        if (_waveTimer < _waves[_currentWaveIndex].Duration) return;

        // avança proxima onda
        _currentWaveIndex++;
        _waveTimer = 0;

        if (_currentWaveIndex >= _waves.Count) return;

        // att estado global nova onda
        GlobalGhostState = _waves[_currentWaveIndex].Mode;

        // Inverte direção de todos os fantasmas na troca de onda
        foreach (var ghost in ghosts)
        {
            if (ghost.CurrentState != GhostState.InHouse &&
                ghost.CurrentState != GhostState.Eaten &&
                ghost.CurrentState != GhostState.Frightened &&
                !ghost.IsExiting)
            {
                ghost.ForceReverseDirection();
            }
        }
    }

    // Reset do estado inicial (tudo e chamado apos pacman morrer)
    public void Reset()
    {
        _gameTime = 0;
        _currentWaveIndex = 0;
        _waveTimer = 0;
        GlobalGhostState = GhostState.Scatter;
    }
}

using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Services;

public class GhostWaveService
{
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
    private double _waveTimer = 0;
    private double _gameTime = 0;

    public GhostState GlobalGhostState { get; private set; } = GhostState.Scatter;

    public void Update(double deltaTime, List<Ghost> ghosts)
    {
        _gameTime += deltaTime;

        UpdateReleases(deltaTime, ghosts);
        UpdateWaves(deltaTime, ghosts);
    }

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

    private void UpdateWaves(double deltaTime, List<Ghost> ghosts)
    {
        if (_currentWaveIndex >= _waves.Count) return;

        _waveTimer += deltaTime;

        if (_waveTimer < _waves[_currentWaveIndex].Duration) return;

        _currentWaveIndex++;
        _waveTimer = 0;

        if (_currentWaveIndex >= _waves.Count) return;

        GlobalGhostState = _waves[_currentWaveIndex].Mode;

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

    public void Reset()
    {
        _gameTime = 0;
        _currentWaveIndex = 0;
        _waveTimer = 0;
        GlobalGhostState = GhostState.Scatter;
    }
}

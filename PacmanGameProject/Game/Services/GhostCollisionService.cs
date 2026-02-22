using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class GhostCollisionService : IGhostCollisionService
{
    private readonly ICollisionService _collisionService;
    private readonly IGameStateService _gameStateService;

    public event Action? OnPacmanDied;
    public event Action? OnGhostEaten;

    public GhostCollisionService(ICollisionService collisionService, IGameStateService gameStateService)
    {
        _collisionService = collisionService;
        _gameStateService = gameStateService;
    }
    
    public void Check(Pacman pacman, List<Ghost> ghosts)
    {
        foreach (var ghost in ghosts)
        {
            if (!_collisionService.Collides(pacman, ghost)) continue;

            if (ghost.CurrentState == GhostState.Frightened)
            {
                ghost.SendToHouse();
                _gameStateService.AddScore(200);
                OnGhostEaten?.Invoke();
            }
            
            else if (ghost.CurrentState != GhostState.Eaten && ghost.CurrentState != GhostState.InHouse)
            {
                _gameStateService.PacmanDied();
                OnPacmanDied?.Invoke();
            }
            
            break;
        }
    }
}

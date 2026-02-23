using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

// Classe responsável em verificar colisoes entre o Pacman e os fantasmas a cada frame
public class GhostCollisionService : IGhostCollisionService
{
    private readonly ICollisionService _collisionService;
    private readonly IGameStateService _gameStateService;

    public event Action? OnPacmanDied; // Quando fantasma colide pacman
    public event Action? OnGhostEaten; // Pacman come fantasma frightened mode

    public GhostCollisionService(ICollisionService collisionService, IGameStateService gameStateService)
    {
        _collisionService = collisionService;
        _gameStateService = gameStateService;
    }
    
    // Checa a colisão entre pacman e os fantasmas
    public void Check(Pacman pacman, List<Ghost> ghosts)
    {
        foreach (var ghost in ghosts)
        {
            if (!_collisionService.Collides(pacman, ghost)) continue;

            if (ghost.CurrentState == GhostState.Frightened)
            {
                // Pacman come fantasma e muda para Eaten e pontua
                ghost.CurrentState = GhostState.Eaten;
                _gameStateService.AddScore(200);
                OnGhostEaten?.Invoke();
            }
            
            else if (ghost.CurrentState != GhostState.Eaten && ghost.CurrentState != GhostState.InHouse)
            {
                // Fantasma normal mata Pacman
                _gameStateService.PacmanDied();
                OnPacmanDied?.Invoke();
            }
            
            // Para loop
            break;
        }
    }
}

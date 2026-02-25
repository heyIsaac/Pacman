using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.Services.interfaces;

public interface IGhostCollisionService
{
    event Action? OnPacmanDied;
    event Action? OnGhostEaten;
    
    void Check(Pacman pacman, List<Ghost> ghosts);
}

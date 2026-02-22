using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

public interface IGhostBehavior
{
    (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky);
}

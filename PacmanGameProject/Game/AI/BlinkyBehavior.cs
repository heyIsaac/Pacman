using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

public class BlinkyBehavior : IGhostBehavior
{
    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        var pPos = pacman.GridPosition;
        return (pPos.X, pPos.Y);
    }
}

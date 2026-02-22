using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

public class ClydeBehavior : IGhostBehavior
{
    private readonly (int x, int y) _scatterTarget = (0, 31);

    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        var pPos = pacman.GridPosition;

        double distSq = Math.Pow(self.GridPosition.x - pPos.X, 2)
                        + Math.Pow(self.GridPosition.y - pPos.Y, 2);
        
        return distSq >= 64 ? (pPos.X, pPos.Y) : _scatterTarget;
    }
}

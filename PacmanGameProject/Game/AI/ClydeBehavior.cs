using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

// O comportamento do fantasma Clyde -> Laranja
// A ideia é ele perseguir o pacman quando está longe e foge para o canto quando pacman está perto

public class ClydeBehavior : IGhostBehavior
{
    // tile de scatter do Clyde 
    private readonly (int x, int y) _scatterTarget = (0, 31);

    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        var pPos = pacman.GridPosition;

        // calcula distância ao quadrado entre Clyde e Pacman
        double distSq = Math.Pow(self.GridPosition.x - pPos.X, 2)
                        + Math.Pow(self.GridPosition.y - pPos.Y, 2);
        
        // Se a distância for 8 tiles, ele persegue o Pacman, se não vai para o canto de scatter
        return distSq >= 64 ? (pPos.X, pPos.Y) : _scatterTarget;
    }
}

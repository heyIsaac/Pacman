using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.AI;

// Comportamento do fantasma Pinky -> Rosa
// Tenta emboscar mirando 4 tiles a frente do Pacman
public class PinkyBehavior : IGhostBehavior
{
    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        var pPos = pacman.GridPosition;
        var pDir = pacman.CurrentDirection;
        var (dx, dy) = DirectionToVector(pDir);

        // mira 4 tiles
        int tx = pPos.X + dx * 4;
        int ty = pPos.Y + dy * 4;
        
        // Pacman vai para cima, X também é deslocado 4 tiles para a esquerda
        if (pDir == Direction.Up) tx -= 4;

        return (tx, ty);
    }

    private static (int dx, int dy) DirectionToVector(Direction dir) => dir switch
    {
        Direction.Left  => (-1, 0),
        Direction.Right => (1, 0),
        Direction.Up    => (0, -1),
        Direction.Down  => (0, 1),
        _               => (0, 0)
    };

}

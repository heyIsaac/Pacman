using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.AI;

public class InkyBehavior :IGhostBehavior
{
    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        var pPos = pacman.GridPosition;
        var pDir = pacman.CurrentDirection;
        var (dx, dy) = DirectionToVector(pDir);

        int px = pPos.X + dx * 2;
        int py = pPos.Y + dy * 2;
        
        if (pDir == Direction.Up) px -= 2;

        int bx = blinky?.GridPosition.x ?? 0;
        int by = blinky?.GridPosition.y ?? 0;

        return (px + (px - bx), py + (py - by));
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

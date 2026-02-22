using PacmanGameProject.Game.AI;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Services;

public class GhostDirectionService
{
    private readonly Random _rand = new();

    public Direction Decide(Ghost ghost, IGhostBehavior ghostBehavior, (int x, int y) scatterTarget,
        Pacman pacman, Ghost? blinky, Func<int, int, bool> isTileBlocked)
    {
        var target = ResolveTarget(ghost, ghostBehavior, scatterTarget, pacman, blinky);
        return DecideBestDirection(ghost, target, isTileBlocked);
    }

    private (int x, int y) ResolveTarget(Ghost ghost, IGhostBehavior ghostBehavior, (int x, int y) scatterTarget,
        Pacman pacman, Ghost? blinky)
    {
        if (ghost.CurrentState == GhostState.Frightened)
        {
            var (gx, gy) = ghost.GridPosition;
            return (gx + _rand.Next(-5, 6), gy + _rand.Next(-5, 6));
        }

        if (ghost.CurrentState == GhostState.Scatter)
            return scatterTarget;

        return ghostBehavior.GetTargetTile(ghost, pacman, blinky);
    }

    private static Direction DecideBestDirection(Ghost ghost, (int x, int y) target, Func<int, int, bool> isTileBlocked)
    {
        var (gx, gy) = ghost.GridPosition;
        Direction opposite = GetOppositeDirection(ghost.CurrentDirection);
        var candidates = new[] { Direction.Up, Direction.Left, Direction.Down, Direction.Right };

        Direction bestDir = Direction.None;
        double minDist = double.MaxValue;
        bool foundValid = false;
        
        foreach (var dir in candidates)
        {
            if (dir == opposite) continue;

            var (ddx, ddy) = DirectionToVector(dir);
            int nx = gx + ddx;
            int ny = gy + ddy;

            if (isTileBlocked(nx, ny)) continue;

            double dist = GetDistanceSq(nx, ny, target.x, target.y);
            if (dist < minDist)
            {
                minDist = dist;
                bestDir = dir;
                foundValid = true;
            }
        }

        if (foundValid) return bestDir;

        // Fallback: tenta a direção oposta
        var (odx, ody) = DirectionToVector(opposite);
        if (!isTileBlocked(gx + odx, gy + ody)) return opposite;

        return ghost.CurrentDirection;
    }
    
    private static double GetDistanceSq(int x1, int y1, int x2, int y2)
        => Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2);

    private static (int dx, int dy) DirectionToVector(Direction dir) => dir switch
    {
        Direction.Left  => (-1, 0),
        Direction.Right => (1, 0),
        Direction.Up    => (0, -1),
        Direction.Down  => (0, 1),
        _               => (0, 0)
    };

    private static Direction GetOppositeDirection(Direction dir) => dir switch
    {
        Direction.Left  => Direction.Right,
        Direction.Right => Direction.Left,
        Direction.Up    => Direction.Down,
        Direction.Down  => Direction.Up,
        _               => Direction.None
    };
    
    
}

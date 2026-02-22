using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Movement;

public class GhostExitHandler
{
    private const int TILE_SIZE = 8;
    private const int HOUSE_EXIT_COL = 13;
    private const int EXIT_ROW = 11;

    private static readonly double EXIT_TARGET_Y = EXIT_ROW * TILE_SIZE;
    private static readonly double HOUSE_EXIT_X = HOUSE_EXIT_COL * TILE_SIZE;

    private readonly Queue<(double px, double py)> _waypoints = new();

    public bool IsExiting => _waypoints.Count > 0;

    public void BuildExitWaypoints(Ghost ghost)
    {
        _waypoints.Clear();
        if (Math.Abs(ghost.X - HOUSE_EXIT_X) > 0.5)
        {
            _waypoints.Enqueue((HOUSE_EXIT_X, ghost.Y));
        }
        
        _waypoints.Enqueue((HOUSE_EXIT_X, EXIT_TARGET_Y));
    }

    public void Clear() => _waypoints.Clear();

    public bool Step(Ghost ghost)
    {
        if (_waypoints.Count == 0) return true;

        var (tx, ty) = _waypoints.Peek();
        double dx = tx - ghost.X;
        double dy = ty - ghost.Y;

        if (Math.Abs(dx) > ghost.Speed)
        {
            ghost.X += Math.Sign(dx) * ghost.Speed;
            ghost.CurrentDirection = dx > 0 ? Direction.Right : Direction.Left;
            return false;
        }

        if (Math.Abs(dy) > ghost.Speed)
        {
            ghost.X = tx;
            ghost.Y += Math.Sign(dy) * ghost.Speed;
            ghost.CurrentDirection = dy > 0 ? Direction.Down : Direction.Up;
            return false;
        }
        
        ghost.X = tx;
        ghost.Y = ty;
        _waypoints.Dequeue();
        
        if (_waypoints.Count == 0)
        {
            ghost.CurrentDirection = Direction.Left;
            return true;
        }

        return false;

    }
    
    public bool NeedsExit(Ghost ghost)
    {
        return ghost.Y > EXIT_TARGET_Y + 0.1;
    }
}

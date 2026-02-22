using PacmanGameProject.Game.AI;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Interfaces;
using PacmanGameProject.Game.Movement;

namespace PacmanGameProject.Game.Entities;

public class Ghost : ICollidable
{
    private const int TILE_SIZE = 8;

    // Constantes do Mapa
    public const int DOOR_ROW = 12;
    public const int EXIT_ROW = 11;
    public const int DOOR_COL_MIN = 13;
    public const int DOOR_COL_MAX = 14;

    private const int HOUSE_EXIT_COL = 13;
    private const double EXIT_TARGET_Y = EXIT_ROW * TILE_SIZE;
    private const double HOUSE_EXIT_X = HOUSE_EXIT_COL * TILE_SIZE;

    public double RespawnTimeRemaining { get; set; } = 0;
    private const double HOUSE_CENTER_X = 13.5 * TILE_SIZE;

    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = 1.0;
    public double Size => TILE_SIZE;

    public GhostType Type { get; }
    public GhostState CurrentState { get; set; } = GhostState.InHouse;
    public Direction CurrentDirection { get; private set; } = Direction.Left;

    private readonly (int x, int y) _scatterTarget;
    private readonly Random _rand = new();
    private readonly Queue<(double px, double py)> _exitWaypoints = new();
    public bool IsExiting => _exitWaypoints.Count > 0;

    private readonly IGhostBehavior _behavior;
    private readonly GhostMover _ghostMover = new();

    private bool _hasExited = false;

    public Ghost(GhostType type, double startX, double startY)
    {
        Type = type;
        X = startX;
        Y = startY;
        _scatterTarget = type switch
        {
            GhostType.Blinky => (25, -3),
            GhostType.Pinky => (2, -3),
            GhostType.Inky => (27, 31),
            GhostType.Clyde => (0, 31),
            _ => (0, 0)
        };
        
        _behavior = type switch
        {
            GhostType.Blinky => new BlinkyBehavior(),
            GhostType.Pinky  => new PinkyBehavior(),
            GhostType.Inky   => new InkyBehavior(),
            GhostType.Clyde  => new ClydeBehavior(),
            _                => new BlinkyBehavior()
        };
    }

    public (int x, int y) GridPosition => ((int)(X / TILE_SIZE), (int)(Y / TILE_SIZE));

    public void Release()
    {
        if (CurrentState != GhostState.InHouse) return;
        _hasExited = false;
        CurrentState = GhostState.Scatter;
        BuildExitWaypoints();
    }

    public void MarkAsExited() => _hasExited = true;

  
    public void SendToHouse()
    {
        _hasExited = false;
        _exitWaypoints.Clear();
        X = HOUSE_CENTER_X;
        Y = 14 * TILE_SIZE;
        _ghostMover.AlignToGrid(this);

        CurrentState = GhostState.InHouse;

        // Durante esse tempo, o GameLoop NÃO vai liberar este fantasma.
        RespawnTimeRemaining = 5.0;
    }

    // MÉTODO de reset do ghost: Limpa toda a memória do fantasma
    public void Reset(double startX, double startY, GhostState initialState)
    {
        X = startX;
        Y = startY;
        _ghostMover.AlignToGrid(this);

        CurrentState = initialState;
        CurrentDirection = Direction.Left;

        _exitWaypoints.Clear();
        // Se começa fora (Blinky), já marca como saído. Se dentro, falso.
        _hasExited = (initialState != GhostState.InHouse);
    }

    public void Update(Pacman pacman, Ghost blinky, GhostState globalState, Func<int, int, bool> isTileBlocked)
    {

        if (CurrentState == GhostState.Frightened)
        {
            Speed = 0.6; // frightened
        }
        else if (CurrentState == GhostState.Eaten)
        {
            Speed = 3.0; // eaten
        }
        else
        {
            Speed = 1.0; // Velocidade normal
        }

        if (CurrentState != GhostState.Eaten &&
            CurrentState != GhostState.InHouse &&
            CurrentState != GhostState.Frightened &&
            !IsExiting)
        {
            CurrentState = globalState;
        }

        if (!_hasExited && CurrentState != GhostState.InHouse && !IsExiting && Y > EXIT_TARGET_Y + 0.1)
        {
            BuildExitWaypoints();
        }

        if (IsExiting) { PerformWaypointMovement(); return; }
        if (CurrentState == GhostState.InHouse) return;

        if (_ghostMover.IsAtTileCenter(this))
        {
            _ghostMover.AlignToGrid(this);
            CurrentDirection = DecideDirection(pacman, blinky, isTileBlocked);
        }
        _ghostMover.Move(this, isTileBlocked);
    }

    private void BuildExitWaypoints()
    {
        _exitWaypoints.Clear();
        if (Math.Abs(X - HOUSE_EXIT_X) > 0.5) _exitWaypoints.Enqueue((HOUSE_EXIT_X, Y));
        _exitWaypoints.Enqueue((HOUSE_EXIT_X, EXIT_TARGET_Y));
    }

    private void PerformWaypointMovement()
    {
        var (tx, ty) = _exitWaypoints.Peek();
        double dx = tx - X, dy = ty - Y;

        if (Math.Abs(dx) > Speed) { X += Math.Sign(dx) * Speed; CurrentDirection = dx > 0 ? Direction.Right : Direction.Left; return; }
        if (Math.Abs(dy) > Speed) { X = tx; Y += Math.Sign(dy) * Speed; CurrentDirection = dy > 0 ? Direction.Down : Direction.Up; return; }

        X = tx; Y = ty;
        _exitWaypoints.Dequeue();
        if (!IsExiting) { _hasExited = true; CurrentDirection = Direction.Left; }
    }

   
    private Direction DecideDirection(Pacman pacman, Ghost blinky, Func<int, int, bool> isTileBlocked)
    {
        var (gx, gy) = GridPosition;
        var target = GetTargetTile(pacman, blinky);
        Direction opposite = GetOppositeDirection(CurrentDirection);
        var candidates = new[] { Direction.Up, Direction.Left, Direction.Down, Direction.Right };
        Direction bestDir = Direction.None;
        double minDist = double.MaxValue;
        bool foundValid = false;

        foreach (var dir in candidates)
        {
            if (dir == opposite) continue;
            var (ddx, ddy) = DirectionToVector(dir);
            int nx = gx + ddx, ny = gy + ddy;
            if (isTileBlocked(nx, ny)) continue;
            double dist = GetDistanceSq(nx, ny, target.x, target.y);
            if (dist < minDist) { minDist = dist; bestDir = dir; foundValid = true; }
        }
        if (foundValid) return bestDir;

        var (odx, ody) = DirectionToVector(opposite);
        if (!isTileBlocked(gx + odx, gy + ody)) return opposite;
        return CurrentDirection;
    }

    private (int x, int y) GetTargetTile(Pacman pacman, Ghost blinky)
    {
        if (CurrentState == GhostState.Frightened) { var (gx, gy) = GridPosition; return (gx + _rand.Next(-5, 6), gy + _rand.Next(-5, 6)); }
        if (CurrentState == GhostState.Scatter) return _scatterTarget;

        var pPos = pacman.GridPosition;
        var pDir = pacman.CurrentDirection;
        switch (Type)
        {
            case GhostType.Blinky: return (pPos.X, pPos.Y);
            case GhostType.Pinky:
                var (pDx, pDy) = DirectionToVector(pDir);
                int tx = pPos.X + pDx * 4, ty = pPos.Y + pDy * 4;
                if (pDir == Direction.Up) tx -= 4; return (tx, ty);
            case GhostType.Inky:
                var (iDx, iDy) = DirectionToVector(pDir);
                int px = pPos.X + iDx * 2, py = pPos.Y + iDy * 2;
                if (pDir == Direction.Up) px -= 2;
                int bx = blinky?.GridPosition.x ?? 0, by = blinky?.GridPosition.y ?? 0;
                return (px + (px - bx), py + (py - by));
            case GhostType.Clyde:
                if (GetDistanceSq(GridPosition.x, GridPosition.y, pPos.X, pPos.Y) >= 64) return (pPos.X, pPos.Y);
                return _scatterTarget;
        }
        return (pPos.X, pPos.Y);
    }

   
    
    private static double GetDistanceSq(int x1, int y1, int x2, int y2) => Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2);
    private static (int dx, int dy) DirectionToVector(Direction dir) => dir switch { Direction.Left => (-1, 0), Direction.Right => (1, 0), Direction.Up => (0, -1), Direction.Down => (0, 1), _ => (0, 0) };
    private static Direction GetOppositeDirection(Direction dir) => dir switch { Direction.Left => Direction.Right, Direction.Right => Direction.Left, Direction.Up => Direction.Down, Direction.Down => Direction.Up, _ => Direction.None };
    public void ForceReverseDirection() { CurrentDirection = GetOppositeDirection(CurrentDirection); }
}

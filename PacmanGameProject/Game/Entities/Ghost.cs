using PacmanGameProject.Game.AI;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Interfaces;
using PacmanGameProject.Game.Movement;
using PacmanGameProject.Game.Services;

namespace PacmanGameProject.Game.Entities;

public class Ghost : ICollidable
{
    private const int TILE_SIZE = 8;

    // Constantes do Mapa
    public const int DOOR_ROW = 12;
    public const int EXIT_ROW = 11;
    public const int DOOR_COL_MIN = 13;
    public const int DOOR_COL_MAX = 14;

  
    private const double EXIT_TARGET_Y = EXIT_ROW * TILE_SIZE;
   

    public double RespawnTimeRemaining { get; set; } = 0;
    private const double HOUSE_CENTER_X = 13.5 * TILE_SIZE;

    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = 1.0;
    public double Size => TILE_SIZE;
    public bool IsExiting => _exitHandler.IsExiting;

    public GhostType Type { get; }
    public GhostState CurrentState { get; set; } = GhostState.InHouse;
    public Direction CurrentDirection { get; set; } = Direction.Left;

    private readonly (int x, int y) _scatterTarget;
    private readonly Random _rand = new();
    
    private readonly IGhostBehavior _behavior;
    
    private readonly GhostMover _ghostMover = new();
    private readonly GhostExitHandler _exitHandler = new();
    private readonly GhostDirectionService _directionService = new();

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
        _exitHandler.BuildExitWaypoints(this);
    }

    public void MarkAsExited() => _hasExited = true;

  
    public void SendToHouse()
    {
        _hasExited = false;
        _exitHandler.Clear();
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

        _exitHandler.Clear();
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
            _exitHandler.BuildExitWaypoints(this);
        }

        if (IsExiting)
        {
            bool finished = _exitHandler.Step(this);
            if (finished) _hasExited = true;
            return;
        }
        if (CurrentState == GhostState.InHouse) return;

        if (_ghostMover.IsAtTileCenter(this))
        {
            _ghostMover.AlignToGrid(this);
            CurrentDirection = _directionService.Decide(this, _behavior, _scatterTarget, pacman, blinky, isTileBlocked);
        }
        _ghostMover.Move(this, isTileBlocked);
    }
    
    
    public void ForceReverseDirection()
    {
        CurrentDirection = GhostMover.GetOppositeDirection(CurrentDirection);
    }
}

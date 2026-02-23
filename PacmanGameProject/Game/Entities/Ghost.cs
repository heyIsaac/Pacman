using PacmanGameProject.Game.AI;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Interfaces;
using PacmanGameProject.Game.Movement;
using PacmanGameProject.Game.Services;

namespace PacmanGameProject.Game.Entities;

// Classe entidade principal dos fantasmas -> estado e ciclo de vida
public class Ghost : ICollidable
{
    private const int TILE_SIZE = 8;

    // Constantes do Mapa
    public const int DOOR_ROW = 12;
    public const int EXIT_ROW = 11;
    public const int DOOR_COL_MIN = 13;
    public const int DOOR_COL_MAX = 14;
    
    // Tempo restante de penalidade
    public double RespawnTimeRemaining { get; set; } = 0;
    
    // posição central da casa e porta
    private const double HOUSE_CENTER_X = 13.5 * TILE_SIZE;
    private const int HOUSE_DOOR_COL = 13;
    private const int HOUSE_DOOR_ROW = 12;

    // Posição atual em pixels 
    public double X { get; set; }
    public double Y { get; set; }
    
    public double Speed { get; set; } = 1.0; // velocidade movimento pixels por frame
    public double Size => TILE_SIZE; // tamanho dos fantasmas em pixels
    public bool IsExiting => _exitHandler.IsExiting; // delega propriedade IsExiting ao handler de saída

    // Tipo, estado e direção dos fantasmas
    public GhostType Type { get; }
    public GhostState CurrentState { get; set; } = GhostState.InHouse;
    public Direction CurrentDirection { get; set; } = Direction.Left;

    private readonly (int x, int y) _scatterTarget; // Tile do modo Scatter
    
    private readonly IGhostBehavior _behavior; // Comportamento de cada fantasma
    
    // Services de movimento, saída e direção
    private readonly GhostMover _ghostMover = new();
    private readonly GhostExitHandler _exitHandler = new();
    private readonly GhostDirectionService _directionService = new();

    private bool _hasExited = false; // controla saida do fantasma da casa

    public Ghost(GhostType type, double startX, double startY)
    {
        Type = type;
        X = startX;
        Y = startY;
        
        // Define tile de scatter para cada fantasma
        _scatterTarget = type switch
        {
            GhostType.Blinky => (25, -3),
            GhostType.Pinky => (2, -3),
            GhostType.Inky => (27, 31),
            GhostType.Clyde => (0, 31),
            _ => (0, 0)
        };
        
        // instancia o comportamento de cada fantasma
        _behavior = type switch
        {
            GhostType.Blinky => new BlinkyBehavior(),
            GhostType.Pinky  => new PinkyBehavior(),
            GhostType.Inky   => new InkyBehavior(),
            GhostType.Clyde  => new ClydeBehavior(),
            _                => new BlinkyBehavior()
        };
    }

    // converte posição em pixels para coordenada no grid de tiles
    public (int x, int y) GridPosition => ((int)(X / TILE_SIZE), (int)(Y / TILE_SIZE));

    // libera fantasma da casa
    public void Release()
    {
        if (CurrentState != GhostState.InHouse) return;
        _hasExited = false;
        CurrentState = GhostState.Scatter;
        _exitHandler.BuildExitWaypoints(this);
    }

    public void MarkAsExited() => _hasExited = true; // marca que fantasma saiu

    // manda fantasma para casa  
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

    // Att cada frame usando GameLoop
    public void Update(Pacman pacman, Ghost blinky, GhostState globalState, Func<int, int, bool> isTileBlocked)
    {

        UpdateSpeed(); // att velocidade

        // Sincronização com o estado Scatter ou Chase
        if (CurrentState != GhostState.Eaten &&
            CurrentState != GhostState.InHouse &&
            CurrentState != GhostState.Frightened &&
            !IsExiting)
        {
            CurrentState = globalState;
        }

        // reconstroi waypoints se sair do alinhamento da casa sem ter saido
        if (!_hasExited && CurrentState != GhostState.InHouse && !IsExiting && _exitHandler.NeedsExit(this))
            _exitHandler.BuildExitWaypoints(this);

        // processa movimento saida usando waypoints
        if (IsExiting)
        {
            bool finished = _exitHandler.Step(this);
            if (finished) _hasExited = true;
            return;
        }
        
        if (CurrentState == GhostState.InHouse) return; // fantasma dentro casa nao move

        // cada tile center decide nova direção
        if (_ghostMover.IsAtTileCenter(this))
        {
            
            _ghostMover.AlignToGrid(this);
            
            // Se estado Eaten chegou porta, entra casa
            if (CurrentState == GhostState.Eaten)
            {
                var (gx, gy) = GridPosition;
                if (gx == HOUSE_DOOR_COL && gy == HOUSE_DOOR_ROW)
                {
                    SendToHouse();
                    return;
                }
            }
            
            // analisa decisao de direção
            CurrentDirection = _directionService.Decide(this, _behavior, _scatterTarget, pacman, blinky, isTileBlocked);
        }
        
        // move fantasma direção atual
        _ghostMover.Move(this, isTileBlocked);
        
        
    }
    
    // Inverte direção atual -> troca entre Scatter/Chase e modo Frightened
    public void ForceReverseDirection()
    {
        CurrentDirection = GhostMover.GetOppositeDirection(CurrentDirection);
    }
    
    // Velocidade de acordo com estado atual fantasma
    private void UpdateSpeed()
    {
        Speed = CurrentState switch
        {
            GhostState.Frightened => 0.5,
            GhostState.Eaten      => 2.0,
            _                     => 1.0
        };
    }
}

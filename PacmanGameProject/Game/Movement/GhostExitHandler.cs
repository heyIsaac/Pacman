using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Movement;

// Representa o guia para os fantasma sairem fora da casa usando waypoints
public class GhostExitHandler
{
    private const int TILE_SIZE = 8;
    private const int HOUSE_EXIT_COL = 13; // coluna central
    private const int EXIT_ROW = 11; // linha destino final

    // posiçoes pixels calculados a partir das constantes do grid
    private static readonly double EXIT_TARGET_Y = EXIT_ROW * TILE_SIZE;
    private static readonly double HOUSE_EXIT_X = HOUSE_EXIT_COL * TILE_SIZE;

    // Fila de pontos que o fantasma deve percorrer para sair de casa
    private readonly Queue<(double px, double py)> _waypoints = new();

    public bool IsExiting => _waypoints.Count > 0; // True enquanto waypoints pendentes

    
    // Constroi caminho de saida baseado na posição atual dos fantasmas
    public void BuildExitWaypoints(Ghost ghost)
    {
        _waypoints.Clear();
        
        // move para coluna central da porta caso seja necessário
        if (Math.Abs(ghost.X - HOUSE_EXIT_X) > 0.5)
        {
            _waypoints.Enqueue((HOUSE_EXIT_X, ghost.Y));
        }
        
        // Sobe até o tile de saída
        _waypoints.Enqueue((HOUSE_EXIT_X, EXIT_TARGET_Y));
    }

    public void Clear() => _waypoints.Clear(); // limpa todos os waypoints

    
    // Avança fantasma direção ao próximo waypoint
    public bool Step(Ghost ghost)
    {
        if (_waypoints.Count == 0) return true;

        var (tx, ty) = _waypoints.Peek();
        double dx = tx - ghost.X;
        double dy = ty - ghost.Y;

        // movimento horizontal até alinhamento com o waypoint
        if (Math.Abs(dx) > ghost.Speed)
        {
            ghost.X += Math.Sign(dx) * ghost.Speed;
            ghost.CurrentDirection = dx > 0 ? Direction.Right : Direction.Left;
            return false;
        }

        // Movimento vertical até alinhamento com o waypoint
        if (Math.Abs(dy) > ghost.Speed)
        {
            ghost.X = tx;
            ghost.Y += Math.Sign(dy) * ghost.Speed;
            ghost.CurrentDirection = dy > 0 ? Direction.Down : Direction.Up;
            return false;
        }
        
        // Chega no waypoint
        ghost.X = tx;
        ghost.Y = ty;
        _waypoints.Dequeue();
        
        // Se nao tem waypoints, saída OK
        if (_waypoints.Count == 0)
        {
            ghost.CurrentDirection = Direction.Left;
            return true;
        }

        return false;

    }
    
    // Verifica fantasma ta abaixo da linha de saída
    public bool NeedsExit(Ghost ghost)
    {
        return ghost.Y > EXIT_TARGET_Y + 0.1;
    }
}

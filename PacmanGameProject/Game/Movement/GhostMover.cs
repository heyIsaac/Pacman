using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Movement;

// Classe responsável pelo movimento físico do fantasma pelo mapa
public class GhostMover
{
    private const int TILE_SIZE = 8;
    private const int MAP_WIDTH_TILES = 28;
    
    // Move fantasma na direção atual verificando colisoes
    public void Move(Ghost ghost, Func<int, int, bool> isTileBlocked)
    {
        var (dx, dy) = DirectionToVector(ghost.CurrentDirection);

        int gridX = (int)(ghost.X / TILE_SIZE);
        int gridY = (int)(ghost.Y / TILE_SIZE);

        // Rail-Lock: trava o eixo perpendicular ao movimento no centro do tile
        if (dx != 0) ghost.Y = gridY * TILE_SIZE;
        if (dy != 0) ghost.X = gridX * TILE_SIZE;
        
        double nextX = ghost.X + dx * ghost.Speed;
        double nextY = ghost.Y + dy * ghost.Speed;

        // Detecção de "Nariz" (Leading Edge)
        int chkX = gridX, chkY = gridY;

        if (dx > 0) chkX = (int)((nextX + TILE_SIZE - 0.1) / TILE_SIZE); // Borda direita
        else if (dx < 0) chkX = (int)((nextX + 0.1) / TILE_SIZE); // Borda esquerda

        if (dy > 0) chkY = (int)((nextY + TILE_SIZE - 0.1) / TILE_SIZE); // Borda inferior
        else if (dy < 0) chkY = (int)((nextY + 0.1) / TILE_SIZE); // Borda superior

        // Verifica colisão com parede
        if (isTileBlocked(chkX, chkY))
        {
            // Grampeia na posição do tile atual
            ghost.X = gridX * TILE_SIZE;
            ghost.Y = gridY * TILE_SIZE;
        }
        else
        {
            // Caminho livre
            ghost.X = nextX;
            ghost.Y = nextY;
        }

        // Lógica do Túnel (Wrap Around)
        ApplyTunnelWrap(ghost);
    }
    
    // Verifica se o fantasma está suficientemente próximo do centro de um tile e decide troca de direção
    public bool IsAtTileCenter(Ghost ghost)
    {
        // detecta mesmo em movimento
        double tol = ghost.Speed * 0.55;
        double modX = Math.Abs(ghost.X % TILE_SIZE);
        double modY = Math.Abs(ghost.Y % TILE_SIZE);
        return (modX < tol || modX > TILE_SIZE - tol)
               && (modY < tol || modY > TILE_SIZE - tol);
    }

    // Alinha fantasma ao centro exato do tile mais próximo
    public void AlignToGrid(Ghost ghost)
    {
        ghost.X = Math.Round(ghost.X / TILE_SIZE) * TILE_SIZE;
        ghost.Y = Math.Round(ghost.Y / TILE_SIZE) * TILE_SIZE;
    }
    
    // Aplica wrap around horizontal, saindo de um lado e aparecendo no outro
    private static void ApplyTunnelWrap(Ghost ghost) // linha 14
    {
        double mapWidth = MAP_WIDTH_TILES * TILE_SIZE;

        if (ghost.X <= -ghost.Size)
            ghost.X = mapWidth;
        else if (ghost.X >= mapWidth)
            ghost.X = -ghost.Size;
    }

    // Converte direção em vetor
    public static (int dx, int dy) DirectionToVector(Direction dir) => dir switch
    {
        Direction.Left  => (-1, 0),
        Direction.Right => (1, 0),
        Direction.Up    => (0, -1),
        Direction.Down  => (0, 1),
        _               => (0, 0)
    };
    
    // retorna direção oposta
    public static Direction GetOppositeDirection(Direction dir) => dir switch
    {
        Direction.Left  => Direction.Right,
        Direction.Right => Direction.Left,
        Direction.Up    => Direction.Down,
        Direction.Down  => Direction.Up,
        _               => Direction.None
    };
}

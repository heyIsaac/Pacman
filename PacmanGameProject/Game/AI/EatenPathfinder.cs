using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.AI;

// Pathfinder dos fantasmas no modo Eaten -> Usa BFS para encontrar caminho
public static class EatenPathfinder
{
    // coords casa
    private const int HOUSE_DOOR_COL = 13;
    private const int HOUSE_DOOR_ROW = 12;
    
    public static Direction FindNextDirection(
        Ghost ghost,
        Func<int, int, bool> isTileBlocked)
    {
        // posição atual do fantasma
        var (startX, startY) = ghost.GridPosition;
        var target = (x: HOUSE_DOOR_COL, y: HOUSE_DOOR_ROW);

        // Quando tiver porta vai manter a direção atual
        if (startX == target.x && startY == target.y)
            return ghost.CurrentDirection;

        // BFS (Breadth-First Serach) -> é um algoritmo para encontrar caminho + curto
        var queue = new Queue<(int x, int y)>();
        var visited = new Dictionary<(int, int), (int x, int y)>(); // filho -> pai
        var firstDir = new Dictionary<(int, int), Direction>(); // tile -> primeira direção tomada

        // Inicia o BFS
        queue.Enqueue((startX, startY));
        visited[(startX, startY)] = (-1, -1);

        // Direções possiveis 
        var dirs = new[]
        {
            (Direction.Up,    0, -1),
            (Direction.Down,  0,  1),
            (Direction.Left, -1,  0),
            (Direction.Right, 1,  0)
        };

        // Algoritmo BFS até encontrar a porta
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();

            foreach (var (dir, dx, dy) in dirs)
            {
                int nx = cx + dx;
                int ny = cy + dy;
                
                // ignora os tiles visitados
                if (visited.ContainsKey((nx, ny))) continue;
                
                // ignora tiles bloqueados por parede
                if (isTileBlocked(nx, ny)) continue;

                // marca tiles como visitado
                visited[(nx, ny)] = (cx, cy);

                // Registra a primeira direção tomada a partir do start
                firstDir[(nx, ny)] = (cx == startX && cy == startY)
                    ? dir
                    : firstDir[(cx, cy)];

                // encontra a porta, então retorna a primeira direção do caminho
                if (nx == target.x && ny == target.y)
                    return firstDir[(nx, ny)];

                queue.Enqueue((nx, ny));
            }
        }

        // Fallback: continua na direção atual
        return ghost.CurrentDirection;
    }
}

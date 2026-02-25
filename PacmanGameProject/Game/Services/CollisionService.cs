using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Interfaces;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class CollisionService : ICollisionService
{
    private readonly int[,] _map;
    private readonly int _tileSize;

    // Linha do Túnel
    private const int TUNNEL_ROW = 14;

    public CollisionService(int[,] map, int tileSize)
    {
        _map = map;
        _tileSize = tileSize;
    }

    public bool CollidesWithWall(ICollidable entity, double newX, double newY)
    {
        double left = newX;
        double right = newX + _tileSize - 1;
        double top = newY;
        double bottom = newY + _tileSize - 1;

        int tileLeft = (int)(left / _tileSize);
        int tileRight = (int)(right / _tileSize);
        int tileTop = (int)(top / _tileSize);
        int tileBottom = (int)(bottom / _tileSize);

        // Se estivermos na linha do túnel, permitimos que X saia dos limites (retorna false/sem colisão)
        if (tileTop == TUNNEL_ROW && tileBottom == TUNNEL_ROW)
        {
            // Ignora verificação de limites laterais nesta linha específica
        }
        else
        {
            // Verificação padrão para o resto do mapa
            if (tileLeft < 0 || tileTop < 0 ||
                tileRight >= _map.GetLength(1) ||
                tileBottom >= _map.GetLength(0))
                return true;
        }

        // Se o índice for válido dentro do array, verifica se é parede
        // Math.Clamp para evitar IndexOutOfRange enquanto ele atravessa a borda
        int safeLeft = Math.Clamp(tileLeft, 0, _map.GetLength(1) - 1);
        int safeRight = Math.Clamp(tileRight, 0, _map.GetLength(1) - 1);
        int safeTop = Math.Clamp(tileTop, 0, _map.GetLength(0) - 1);
        int safeBottom = Math.Clamp(tileBottom, 0, _map.GetLength(0) - 1);

        if (MapData.IsWall(_map[safeTop, safeLeft])) return true;
        if (MapData.IsWall(_map[safeTop, safeRight])) return true;
        if (MapData.IsWall(_map[safeBottom, safeLeft])) return true;
        if (MapData.IsWall(_map[safeBottom, safeRight])) return true;

        // Lógica da Porta da casinha 
        int tl = _map[safeTop, safeLeft];
        int tr = _map[safeTop, safeRight];
        int bl = _map[safeBottom, safeLeft];
        int br = _map[safeBottom, safeRight];

        if (tl == MapData.GHOST_DOOR || tr == MapData.GHOST_DOOR ||
            bl == MapData.GHOST_DOOR || br == MapData.GHOST_DOOR)
        {
            if (entity is Pacman) return true;
            if (entity is Ghost ghost)
            {
                // Se for fantasma, só passa se for Olhos ou se o jogo permitir
                return ghost.CurrentState != Enums.GhostState.Eaten;
            }
        }

        return false;
    }

    public bool Collides(ICollidable a, ICollidable b)
    {
        double diffX = Math.Abs(a.X - b.X);
        double diffY = Math.Abs(a.Y - b.Y);
        return diffX < a.Size && diffY < a.Size;
    }
}

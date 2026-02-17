using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums; // Adicione este namespace
using PacmanGameProject.Game.Interfaces;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class CollisionService : ICollisionService
{
    private readonly int[,] _map;
    private readonly int _tileSize;

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

        if (tileLeft < 0 || tileTop < 0 ||
            tileRight >= _map.GetLength(1) ||
            tileBottom >= _map.GetLength(0))
            return true;

        if (MapData.IsWall(_map[tileTop, tileLeft])) return true;
        if (MapData.IsWall(_map[tileTop, tileRight])) return true;
        if (MapData.IsWall(_map[tileBottom, tileLeft])) return true;
        if (MapData.IsWall(_map[tileBottom, tileRight])) return true;

        int tl = _map[tileTop, tileLeft];
        int tr = _map[tileTop, tileRight];
        int bl = _map[tileBottom, tileLeft];
        int br = _map[tileBottom, tileRight];

        // LÓGICA DA PORTA DA CASA (GHOST DOOR)
        if (tl == MapData.GHOST_DOOR ||
            tr == MapData.GHOST_DOOR ||
            bl == MapData.GHOST_DOOR ||
            br == MapData.GHOST_DOOR)
        {
            if (entity is Pacman) return true; // Pacman nunca entra

            if (entity is Ghost ghost)
            {
                // CORREÇÃO: Usar o Enum CurrentState
                // Se ele foi comido (olhos) ou está dentro de casa (mas o jogo mandou sair), ele passa.
                // Nota: A lógica de saída agora é controlada pela mudança de estado no GameLoop.
                // Se o estado NÃO for InHouse, ele tecnicamente pode passar pela porta para sair.
                if (ghost.CurrentState == GhostState.Eaten ||
                    ghost.CurrentState != GhostState.InHouse)
                    return false; // Permite passar

                return true; // Bloqueia se deve ficar na casa
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

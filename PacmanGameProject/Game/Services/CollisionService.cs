using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Services;

public class CollisionService
{
    private readonly int[,] _map;
    private readonly int _tileSize;

    public CollisionService(int[,] map, int tileSize)
    {
        _map = map;
        _tileSize = tileSize;
    }
    
    public bool CollidesWithWall(double newX, double newY)
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

        return false;
    }
    
    public bool Collides(ICollidable a, ICollidable b)
    {
        double diffX = Math.Abs(a.X - b.X);
        double diffY = Math.Abs(a.Y - b.Y);

        return diffX < a.Size && diffY < a.Size;
    }
}

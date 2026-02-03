using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Interfaces;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

// service trata colisoes do jogo
public class CollisionService : ICollisionService
{
    private readonly int[,] _map; 
    private readonly int _tileSize; 

    public CollisionService(int[,] map, int tileSize)
    {
        _map = map;
        _tileSize = tileSize;
    }
    
    // verifica se posição colide com uma parede
    public bool CollidesWithWall(double newX, double newY)
    {
        // bouding box
        double left = newX;
        double right = newX + _tileSize - 1;
        double top = newY;
        double bottom = newY + _tileSize - 1;

        // converte posição em pixels para posição no mapa de tiles
        int tileLeft = (int)(left / _tileSize);
        int tileRight = (int)(right / _tileSize);
        int tileTop = (int)(top / _tileSize);
        int tileBottom = (int)(bottom / _tileSize);

        // Verifica se saiu fora do mapa -> considerando como colisao
        if (tileLeft < 0 || tileTop < 0 ||
            tileRight >= _map.GetLength(1) ||
            tileBottom >= _map.GetLength(0))
            return true;

        // verifica colisoes com paredes no 4 cantos da entidade
        if (MapData.IsWall(_map[tileTop, tileLeft])) return true;
        if (MapData.IsWall(_map[tileTop, tileRight])) return true;
        if (MapData.IsWall(_map[tileBottom, tileLeft])) return true;
        if (MapData.IsWall(_map[tileBottom, tileRight])) return true;

        return false;
    }
    
    // verifica colisao entre duas entities
    public bool Collides(ICollidable a, ICollidable b)
    {
        
        // diferença de posiçoes entre 2 objetos
        double diffX = Math.Abs(a.X - b.X);
        double diffY = Math.Abs(a.Y - b.Y);
        
        return diffX < a.Size && diffY < a.Size;
    }
}

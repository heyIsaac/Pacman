namespace PacmanGameProject.Game.Engine;

public static class GameMap
{
    public const int TileSize = 16;

    public static readonly int[,] Level1 =
    {
        { 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 0, 0, 0, 1 },
        { 1, 0, 1, 1, 0, 1, 0, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1 }
    };
    
    public static bool IsWall(double x, double y)
    {
        int gridX = (int)(x / TileSize);
        int gridY = (int)(y / TileSize);

        // Verifica limites da matriz
        if (gridY < 0 || gridY >= Level1.GetLength(0) || gridX < 0 || gridX >= Level1.GetLength(1))
            return true;

        return Level1[gridY, gridX] == 1;
    }
}

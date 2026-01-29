using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Entities;

public class Pacman
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed = 1;
    public Direction CurrentDirection { get; private set; } = Direction.Left;
    public Direction DesiredDirection { get; set; } = Direction.Left;
    
    private const int TILE_SIZE = 8;

    public void Update(Func<double, double, bool>? wallCheck)
    {
        if (IsCentered())
            TryChangeDirection(wallCheck);

        Move(wallCheck);
    }
    private bool IsCentered()
    {
        return ((int)X % TILE_SIZE == 0) &&
               ((int)Y % TILE_SIZE == 0);
    }

    private void TryChangeDirection(Func<double, double, bool>? wallCheck)
    {
        if (DesiredDirection == CurrentDirection) return;

        var (dx, dy) = DirectionToVector(DesiredDirection);
        
        
        double testX = X + dx * TILE_SIZE;
        double testY = Y + dy * TILE_SIZE;
        
        if (wallCheck == null || !wallCheck(testX, testY))
            CurrentDirection = DesiredDirection;
    }
    
    private void Move( Func<double, double, bool>? wallCheck)
    {
       
        var (dx, dy) = DirectionToVector(CurrentDirection);

        double nextX = X + dx * Speed;
        double nextY = Y + dy * Speed;
        
        if (wallCheck == null || !wallCheck(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }
    }

    private (int dx, int dy) DirectionToVector(Direction dir)
    {
        return dir switch
        {
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            _ => (0, 0)
        };
    }
}



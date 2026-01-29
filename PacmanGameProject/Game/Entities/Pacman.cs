using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Entities;

public class Pacman
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed = 1;
    public Direction CurrentDirection { get; private set; } = Direction.Left;
    public Direction DesiredDirection { get; set; } = Direction.Left;

    public void Update(Func<double, double, bool>? wallCheck)
    {
        TryChangeDirection(wallCheck);
        Move(CurrentDirection, wallCheck);
    }

    private void TryChangeDirection(Func<double, double, bool>? wallCheck)
    {
        if (DesiredDirection == CurrentDirection) return;

        double testX = X, testY = Y;

        switch (DesiredDirection)
        {
            case Direction.Left: testX -= Speed; break;
            case Direction.Right: testX += Speed; break;
            case Direction.Up: testY -= Speed; break;
            case Direction.Down: testY += Speed; break;
        }

        if (wallCheck == null || !wallCheck(testX, testY))
            CurrentDirection = DesiredDirection;
    }
    
    private void Move(Direction dir, Func<double, double, bool>? wallCheck)
    {
        double nextX = X;
        double nextY = Y;

        switch (dir)
        {
            case Direction.Left: nextX -= Speed; break;
            case Direction.Right: nextX += Speed; break;
            case Direction.Up: nextY -= Speed; break;
            case Direction.Down: nextY += Speed; break;
        }

        if (wallCheck == null || !wallCheck(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }
    }
}



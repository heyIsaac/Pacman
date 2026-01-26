using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Entities;

public class Pacman
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed = 10;

    public void Move(Direction dir, Func<double, double, bool>? wallCheck)
    {
        double nextX = X;
        double nextY = Y;
        
        switch (dir)
        {
            case Direction.Left:  nextX -= Speed; break;
            case Direction.Right: nextX += Speed; break;
            case Direction.Up:    nextY -= Speed; break;
            case Direction.Down:  nextY += Speed; break;
        }
        if (wallCheck == null || !wallCheck(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }
        
    }
    
    
}



using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Entities;

public class Pacman
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed = 10;

    public void Move(Direction dir, Func<double, double, bool> isWall)
    {
        double nextX = X;
        double nextY = Y;
        
        switch (dir)
        {
            case Direction.Left: X -= Speed; break;
            case Direction.Right: X += Speed; break;
            case Direction.Up: Y -= Speed; break;
            case Direction.Down: Y += Speed; break;
        }
        
        if (!isWall(nextX, nextY) && 
            !isWall(nextX + 30, nextY) && 
            !isWall(nextX, nextY + 30) && 
            !isWall(nextX + 30, nextY + 30))
        {
            X = nextX;
            Y = nextY;
        }
    }
    
    
}



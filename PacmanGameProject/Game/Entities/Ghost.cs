using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Entities;

public class Ghost
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; } = 1;
    public GhostType Type { get; }

    private Direction _currentDirection = Direction.Left;
    private Random _rand = new();

    public Ghost(GhostType type, double x, double y)
    {
        Type = type;
        X = x;
        Y = y;

    }

    public void Update(Pacman pacman, Func<double, double, bool>? wallCheck)
    {
        switch (Type)
        {
            case GhostType.Blinky:
                _currentDirection = GetDirectionTo(pacman.X, pacman.Y);
                break;
            
            case GhostType.Pinky:
                _currentDirection = GetDirectionTo(pacman.X + 64, pacman.Y);
                break;
            
            case GhostType.Inky:
                if (_rand.Next(0, 20) == 0)
                    _currentDirection = (Direction)_rand.Next(1, 5);
                break;
            
            case GhostType.Clyde:
                double dist = Math.Sqrt(Math.Pow(pacman.X - X, 2) + Math.Pow(pacman.Y - Y, 2));
                _currentDirection = dist < 100
                    ? GetDirectionTo(0, 0)
                    : GetDirectionTo(pacman.X, pacman.Y);
                break;
        }
        
        Move(wallCheck);
    }
    
    
    private Direction GetDirectionTo(double tx, double ty)
    {
        if (Math.Abs(tx - X) > Math.Abs(ty - Y))
            return tx < X ? Direction.Left : Direction.Right;
        else
            return ty < Y ? Direction.Up : Direction.Down;
    }

    private void Move(Func<double, double, bool>? wallCheck)
    {
        double nextX = X;
        double nextY = Y;
    
        switch (_currentDirection)
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
        else
        {
            _currentDirection = (Direction)_rand.Next(1, 5);
        }
    }
    
}

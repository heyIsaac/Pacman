using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Entities;

public class Pacman
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed = 2;

    public void UpdateDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: X -= Speed; break;
            case Direction.Right: X += Speed; break;
            case Direction.Up: Y -= Speed; break;
            case Direction.Down: Y += Speed; break;
        }
    }
}



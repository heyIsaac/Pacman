using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Entities;

public class Pellet : ICollidable
{
    public double X { get; }
    public double Y { get; }
    public double Size { get; }
    
    public bool IsPowerPellet { get; }

    public Pellet(double x, double y, double size, bool isPowerPellet = false)
    {
        X = x;
        Y = y;
        Size = size;
        IsPowerPellet = isPowerPellet;
    }
}

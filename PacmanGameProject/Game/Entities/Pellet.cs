using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Entities;

// Classe representa os pellets no mapa
public class Pellet : ICollidable
{
    // posição
    public double X { get; }
    public double Y { get; }
    public double Size { get; } // tamanho
    
    public bool IsPowerPellet { get; } // decide se tem poder ou não

    public Pellet(double x, double y, double size, bool isPowerPellet = false)
    {
        X = x;
        Y = y;
        Size = size;
        IsPowerPellet = isPowerPellet;
    }
}

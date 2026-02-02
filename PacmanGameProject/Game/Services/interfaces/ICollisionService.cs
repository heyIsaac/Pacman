using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Services.interfaces;

public interface ICollisionService
{
    public bool CollidesWithWall(double newX, double newY);
    public bool Collides(ICollidable a, ICollidable b);
}

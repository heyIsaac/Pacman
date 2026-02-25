using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Services.interfaces;

public interface ICollisionService
{
    public bool CollidesWithWall(ICollidable entity, double newX, double newY);
    public bool Collides(ICollidable a, ICollidable b);
}

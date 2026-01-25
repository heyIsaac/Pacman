using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.Rendering;

public class SpriteRenderer
{
    private readonly Image _pacmanImage;

    public SpriteRenderer(Image pacmanImage)
    {
        _pacmanImage = pacmanImage;
    }

    public void Draw(Pacman pacman)
    {
        Canvas.SetLeft(_pacmanImage, pacman.X);
        Canvas.SetTop(_pacmanImage, pacman.Y);
    }
}

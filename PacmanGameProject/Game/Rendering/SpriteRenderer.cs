using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.Rendering;

public class SpriteRenderer
{
    private readonly Image _pacmanImage;
    private readonly List<Image> _ghostImages;

    public SpriteRenderer(Image pacmanImage, List<Image> ghostImages)
    {
        _pacmanImage = pacmanImage;
        _ghostImages = ghostImages;
    }

    public void Draw(Pacman pacman)
    {
        Canvas.SetLeft(_pacmanImage, pacman.X);
        Canvas.SetTop(_pacmanImage, pacman.Y);
    }
    
    public void DrawGhosts(List<Ghost> ghosts)
    {
        for (int i = 0; i < ghosts.Count; i++)
        {
            Canvas.SetLeft(_ghostImages[i], ghosts[i].X);
            Canvas.SetTop(_ghostImages[i], ghosts[i].Y);
        }
    }
}

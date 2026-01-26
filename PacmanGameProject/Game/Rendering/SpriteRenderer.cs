using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Rendering;

public class SpriteRenderer
{
    private readonly Image _pacmanImage;
    private readonly BitmapImage[] _pacmanFrames;
    private int _currentFrame = 0;
    private int _frameCounter = 0;
    private int _frameDelay = 4;
    private readonly List<Image> _ghostImages;

    public SpriteRenderer(Image pacmanImage, List<Image> ghostImages)
    {
        _pacmanImage = pacmanImage;
        _ghostImages = ghostImages;

         _pacmanFrames = new BitmapImage[]
                {
                    new BitmapImage(new Uri("ms-appx:///Assets/pacman/live/pacman_live_1.png")),
                    new BitmapImage(new Uri("ms-appx:///Assets/pacman/live/pacman_live_2.png")),
                    new BitmapImage(new Uri("ms-appx:///Assets/pacman/live/pacman_live_3.png"))
                };
    }

    public void Draw(Pacman pacman)
    {
        Canvas.SetLeft(_pacmanImage, pacman.X);
        Canvas.SetTop(_pacmanImage, pacman.Y);

        _frameCounter++;
        if (_frameCounter >= _frameDelay)
        {
            _currentFrame++;
            if (_currentFrame >= _pacmanFrames.Length)
                _currentFrame = 0;

            _pacmanImage.Source = _pacmanFrames[_currentFrame];
            _frameCounter = 0;
        }
        
        SetRotation(pacman.CurrentDirection);
    }
    
    public void DrawGhosts(List<Ghost> ghosts)
    {
        for (int i = 0; i < ghosts.Count; i++)
        {
            Canvas.SetLeft(_ghostImages[i], ghosts[i].X);
            Canvas.SetTop(_ghostImages[i], ghosts[i].Y);
        }
    }
    
    public void SetRotation(Direction dir) 
    {
        double angle = dir switch
        {
            Direction.Up => 270,
            Direction.Down => 90,
            Direction.Left => 180,
            Direction.Right => 0,
            _ => 0
        };

        _pacmanImage.RenderTransform = new RotateTransform { Angle = angle };
    }
}

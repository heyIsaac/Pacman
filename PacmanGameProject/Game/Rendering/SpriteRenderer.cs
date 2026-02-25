using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
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
    private Dictionary<string, BitmapImage[][]> _ghostSprites;

    private readonly BitmapImage _frightened1;
    private readonly BitmapImage _frightened2;
    private readonly BitmapImage _flash1;
    private readonly BitmapImage _flash2;
    
    private readonly BitmapImage[] _eyeSprites;

    private int _ghostFrame = 0;
    private int _ghostFrameCounter = 0;
    private int _ghostFrameDelay = 6;

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

        // 2. Loads Normal Ghosts
        LoadGhostSprites();

        // 3. LOADS FRIGHTENED GHOSTS
        _frightened1 = new BitmapImage(new Uri("ms-appx:///Assets/ghosts/state/frightened/frightened_1.png"));
        _frightened2 = new BitmapImage(new Uri("ms-appx:///Assets/ghosts/state/frightened/frightened_2.png"));
        _flash1 = new BitmapImage(new Uri("ms-appx:///Assets/ghosts/state/frightened/frightened_end_1.png"));
        _flash2 = new BitmapImage(new Uri("ms-appx:///Assets/ghosts/state/frightened/frightened_end_2.png"));
        
        // 4. LOADS EYE SPRITES (Eaten mode)
        _eyeSprites = new BitmapImage[]
        {
            new(new Uri("ms-appx:///Assets/ghosts/state/eaten/ghost_eye_right.png")),
            new(new Uri("ms-appx:///Assets/ghosts/state/eaten/ghost_eye_left.png")),
            new(new Uri("ms-appx:///Assets/ghosts/state/eaten/ghost_eye_up.png")),
            new(new Uri("ms-appx:///Assets/ghosts/state/eaten/ghost_eye_down.png"))
        };
    }

    private void LoadGhostSprites()
    {
        _ghostSprites = new Dictionary<string, BitmapImage[][]>();
        _ghostSprites["blinky"] = LoadGhost("blinky");
        _ghostSprites["pinky"] = LoadGhost("pinky");
        _ghostSprites["inky"] = LoadGhost("inky");
        _ghostSprites["clyde"] = LoadGhost("clyde");
    }

    private BitmapImage[][] LoadGhost(string name)
    {
        // 0=Right, 1=Left, 2=Up, 3=Down
        return new BitmapImage[][]
        {
            new BitmapImage[] { new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_direita_1.png")), new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_direita_2.png")) },
            new BitmapImage[] { new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_esquerda_1.png")), new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_esquerda_2.png")) },
            new BitmapImage[] { new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_encima_1.png")),   new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_encima_2.png")) },
            new BitmapImage[] { new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_embaixo_1.png")),  new(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_embaixo_2.png")) }
        };
    }

    // desenha pacman tela
    public void Draw(Pacman pacman)
    {

        // att posição pacman sprite
        Canvas.SetLeft(_pacmanImage, pacman.X - 2);
        Canvas.SetTop(_pacmanImage, pacman.Y - 2);

        // controla animaçao
        _frameCounter++;
        if (_frameCounter >= _frameDelay)
        {
            _currentFrame++;

            // volta para primeiro frame quando passar ultimo
            if (_currentFrame >= _pacmanFrames.Length)
                _currentFrame = 0;

            // att imagem pacman
            _pacmanImage.Source = _pacmanFrames[_currentFrame];
            _frameCounter = 0;
        }

        SetRotation(pacman.CurrentDirection);
    }

    // desenha todos os fantasmas na tela
    public void DrawGhosts(List<Ghost> ghosts, double frightenedTimeRemaining = 0)
    {
        _ghostFrameCounter++;
        if (_ghostFrameCounter >= _ghostFrameDelay)
        {
            _ghostFrame++;
            if (_ghostFrame >= 2) _ghostFrame = 0;
            _ghostFrameCounter = 0;
        }

        for (int i = 0; i < ghosts.Count; i++)
        {
            var ghost = ghosts[i];
            var image = _ghostImages[i];

            Canvas.SetLeft(image, ghost.X - 2);
            Canvas.SetTop(image, ghost.Y - 2);

            // FRIGHTENED MODE
            if (ghost.CurrentState == GhostState.Frightened)
            {
                image.Opacity = 1.0;

                // se falta menos de 2s, começa a piscar
                if (frightenedTimeRemaining > 0 && frightenedTimeRemaining < 2000)
                {
                    if (_ghostFrame == 0)
                        image.Source = _frightened1;
                    else
                        image.Source = _flash1;
                }
                else
                {
                    image.Source = _ghostFrame == 0 ? _frightened1 : _frightened2;
                }
            }
            // EATEN MODE (Eyes)
            else if (ghost.CurrentState == GhostState.Eaten)
            {
                image.Opacity = 1.0;
                int dirIndex = GetDirectionIndex(ghost.CurrentDirection);
                image.Source = _eyeSprites[dirIndex];
                
                
            }
            // NORMAL MODE
            else
            {
                image.Opacity = 1.0;
                string key = GetGhostKey(ghost.Type);
                int dirIndex = GetDirectionIndex(ghost.CurrentDirection);
                image.Source = _ghostSprites[key][dirIndex][_ghostFrame];
            }
        }
    }

    private string GetGhostKey(GhostType type) => type switch
    {
        GhostType.Blinky => "blinky",
        GhostType.Pinky => "pinky",
        GhostType.Inky => "inky",
        GhostType.Clyde => "clyde",
        _ => "blinky"
    };

    private int GetDirectionIndex(Direction dir) => dir switch
    {
        Direction.Right => 0,
        Direction.Left => 1,
        Direction.Up => 2,
        Direction.Down => 3,
        _ => 0
    };

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

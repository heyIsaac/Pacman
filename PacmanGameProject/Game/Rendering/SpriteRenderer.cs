using Microsoft.UI.Xaml.Media.Imaging;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Rendering;

// classe responsavel desenhar os sprites do pacman e dos fantasmas
public class SpriteRenderer
{
    private readonly Image _pacmanImage;
    private readonly BitmapImage[] _pacmanFrames; 
    private int _currentFrame = 0; // frame atual
    private int _frameCounter = 0; // contador de frame para melhor controle de troca
    private int _frameDelay = 4; // qntd de att para trocar frame
    private readonly List<Image> _ghostImages;
    
    private Dictionary<string, BitmapImage[][]> _ghostSprites;
    
    private int _ghostFrame = 0;
    private int _ghostFrameCounter = 0;
    private int _ghostFrameDelay = 6;

    public SpriteRenderer(Image pacmanImage, List<Image> ghostImages)
    {
        _pacmanImage = pacmanImage;
        _ghostImages = ghostImages;

        // carregamento frames
         _pacmanFrames = new BitmapImage[]
                {
                    new BitmapImage(new Uri("ms-appx:///Assets/pacman/live/pacman_live_1.png")),
                    new BitmapImage(new Uri("ms-appx:///Assets/pacman/live/pacman_live_2.png")),
                    new BitmapImage(new Uri("ms-appx:///Assets/pacman/live/pacman_live_3.png"))
                };

                LoadGhostSprites();
    }


    private void LoadGhostSprites()
    {
        _ghostSprites = new Dictionary<string, BitmapImage[][]>();
        
        _ghostSprites["blinky"] = LoadGhost("blinky");
        _ghostSprites["pinky"]  = LoadGhost("pinky");
        _ghostSprites["inky"]   = LoadGhost("inky");
        _ghostSprites["clyde"]  = LoadGhost("clyde");
    }
    
    private BitmapImage[][] LoadGhost(string name)
    {
        // ordem: 0=Right,1=Left,2=Up,3=Down
        return new BitmapImage[][]
        {
            new BitmapImage[]
            {
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_direita_1.png")),
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_direita_2.png"))
            },
            new BitmapImage[]
            {
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_esquerda_1.png")),
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_esquerda_2.png"))
            },
            new BitmapImage[]
            {
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_encima_1.png")),
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_encima_2.png"))
            },
            new BitmapImage[]
            {
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_embaixo_1.png")),
                new BitmapImage(new Uri($"ms-appx:///Assets/ghosts/type/{name}/{name}_embaixo_2.png"))
            }
        };
    }
    
    // desenha pacman tela
    public void Draw(Pacman pacman)
    {
        
        // att posição pacman sprite
        Canvas.SetLeft(_pacmanImage, pacman.X);
        Canvas.SetTop(_pacmanImage, pacman.Y);

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
        
        // att rotação pacman de acordo com direção atual
        SetRotation(pacman.CurrentDirection);
    }
    
    // desenha todos os fantasmas na tela
    public void DrawGhosts(List<Ghost> ghosts)
    {
        _ghostFrameCounter++;
        if (_ghostFrameCounter >= _ghostFrameDelay)
        {
            _ghostFrame++;
            if (_ghostFrame >= 2)
                _ghostFrame = 0;

            _ghostFrameCounter = 0;
        }

        for (int i = 0; i < ghosts.Count; i++)
        {
            var ghost = ghosts[i];
            var image = _ghostImages[i];

            Canvas.SetLeft(image, ghost.X);
            Canvas.SetTop(image, ghost.Y);

            int dirIndex = ghost.CurrentDirection switch
            {
                Direction.Right => 0,
                Direction.Left  => 1,
                Direction.Up    => 2,
                Direction.Down  => 3,
                _ => 0
            };

            string key = ghost.Type switch
            {
                GhostType.Blinky => "blinky",
                GhostType.Pinky  => "pinky",
                GhostType.Inky   => "inky",
                GhostType.Clyde  => "clyde",
                _ => "blinky"
            };

            image.Source = _ghostSprites[key][dirIndex][_ghostFrame];
        }
    }
    
    //ajusta rotação do sprite do pacman de acordo sua direção
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

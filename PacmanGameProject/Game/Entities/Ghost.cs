using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Entities;

public class Ghost : ICollidable
{
    // posição
    public double X { get; set; }
    public double Y { get; set; }
    
    // tamanho fantasma
    public double Size => TILE_SIZE;

    public double Speed { get; set; } = 1;
    public GhostType Type { get; }

    private Direction _currentDirection = Direction.Left;
    private Random _rand = new();

    // tamanho tile mapa
    private const int TILE_SIZE = 8;

    public Ghost(GhostType type, double x, double y)
    {
        Type = type;
        X = x;
        Y = y;
    }

    // att comportamento fantasma
    public void Update(Pacman pacman, Func<double, double, bool>? wallCheck)
    {
        
        // muda direção quando alinhar com grid
        if (IsCentered())
        {
            switch (Type)
            {
                case GhostType.Blinky: // PERSEGUE O PACMAN
                    SetDirectionTo(pacman.X, pacman.Y);
                    break;

                case GhostType.Pinky: // MIRA UM POUCO A FRENTE DO PACMAN
                    SetDirectionTo(pacman.X + 4 * TILE_SIZE, pacman.Y);
                    break;

                case GhostType.Inky: // DE VEZ EM QUANDO SE MOVE ALEATORIAMENTE
                    if (_rand.Next(0, 10) == 0)
                        SetRandomDirection();
                    break;

                case GhostType.Clyde: // FOGE SE TIVER PERTO, SENAO PERSEGUE
                    double dist = DistanceTo(pacman.X, pacman.Y);
                    if (dist < 8 * TILE_SIZE)
                        SetDirectionTo(0, 0);
                    else
                        SetDirectionTo(pacman.X, pacman.Y);
                    break;
            }
        }

        // move fantasma
        Move(wallCheck);
    }

    // Verifica se o fantasma está alinhado ao grid
    private bool IsCentered()
    {
        return ((int)X % TILE_SIZE == 0) &&
               ((int)Y % TILE_SIZE == 0);
    }

    private void Move(Func<double, double, bool>? wallCheck)
    {
        var (dx, dy) = DirectionToVector(_currentDirection);

        double nextX = X + dx * Speed;
        double nextY = Y + dy * Speed;

        if (wallCheck == null || !wallCheck(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }

        else
        {
            X = Math.Round(X / TILE_SIZE) * TILE_SIZE;
            Y = Math.Round(Y / TILE_SIZE) * TILE_SIZE;

            SetRandomDirection();
        }
    }

    private void SetDirectionTo(double tx, double ty)
    {
        double dx = tx - X;
        double dy = ty - Y;

        if (Math.Abs(dx) > Math.Abs(dy))
            _currentDirection = dx < 0 ? Direction.Left : Direction.Right;
        else
            _currentDirection = dy < 0 ? Direction.Up : Direction.Down;
    }

    private void SetRandomDirection()
    {
        _currentDirection = (Direction)_rand.Next(1, 5);
    }

    private double DistanceTo(double tx, double ty)
    {
        return Math.Sqrt(Math.Pow(tx - X, 2) + Math.Pow(ty - Y, 2));
    }

    private (int dx, int dy) DirectionToVector(Direction dir)
    {
        return dir switch
        {
            Direction.Left  => (-1, 0),
            Direction.Right => (1, 0),
            Direction.Up    => (0, -1),
            Direction.Down  => (0, 1),
            _ => (0, 0)
        };
    }
}


using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Interfaces;

namespace PacmanGameProject.Game.Entities;

public class Pacman : ICollidable
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Size => TILE_SIZE;

    public double Speed = 1;
    public Direction CurrentDirection { get; private set; } = Direction.Left;
    public Direction DesiredDirection { get; set; } = Direction.Left;

    private const int TILE_SIZE = 8;

    // A IA precisa saber em qual azulejo (Tile) o Pacman está (Ex: 10, 15) e não o pixel exato (Ex: 80.5, 120.0).
    public (int X, int Y) GridPosition => ((int)(X / TILE_SIZE), (int)(Y / TILE_SIZE));

    // Atualiza pacman cada frame
    public void Update(Func<double, double, bool>? wallCheck)
    {
        // Tenta virar antes de mover.
        if (IsCentered())
        {
            TryChangeDirection(wallCheck);
        }
        // Se não estiver centralizado, mas a direção desejada for OPOSTA à atual,
        // permitimos virar imediatamente (reversal).
        else if (DesiredDirection == GetOppositeDirection(CurrentDirection))
        {
            CurrentDirection = DesiredDirection;
        }

        Move(wallCheck);
    }

    // Verifica se esta centralizado grid (com margem de erro pequena para float)
    private bool IsCentered()
    {
        double tolerance = 0.5; // Margem de segurança
        return Math.Abs(X % TILE_SIZE) < tolerance && Math.Abs(Y % TILE_SIZE) < tolerance;
    }

    // Tenta mudar direção atual para direção desejada
    private void TryChangeDirection(Func<double, double, bool>? wallCheck)
    {
        if (DesiredDirection == CurrentDirection) return;

        var (dx, dy) = DirectionToVector(DesiredDirection);

        // Verifica o tile vizinho exato
        // O ideal é verificar o CENTRO do próximo tile
        double testX = (Math.Round(X / TILE_SIZE) + dx) * TILE_SIZE;
        double testY = (Math.Round(Y / TILE_SIZE) + dy) * TILE_SIZE;

        if (wallCheck == null || !wallCheck(testX, testY))
        {
            // Ajusta posição para o centro exato para fazer a curva perfeita
            X = Math.Round(X / TILE_SIZE) * TILE_SIZE;
            Y = Math.Round(Y / TILE_SIZE) * TILE_SIZE;

            CurrentDirection = DesiredDirection;
        }
    }

    // Move pacman direção atual
    private void Move(Func<double, double, bool>? wallCheck)
    {
        var (dx, dy) = DirectionToVector(CurrentDirection);

        double nextX = X + dx * Speed;
        double nextY = Y + dy * Speed;

        // Verifica colisão
        if (wallCheck == null || !wallCheck(nextX, nextY))
        {
            X = nextX;
            Y = nextY;
        }

        // Mapa tem 28 tiles * 8px = 224px de largura
        double mapWidth = 28 * 8;

        // Se sair pela esquerda (-size), vai pra direita
        if (X <= -Size)
            X = mapWidth;

        // Se sair pela direita, vai pra esquerda
        else if (X >= mapWidth)
            X = -Size;
    }
    private (int dx, int dy) DirectionToVector(Direction dir)
    {
        return dir switch
        {
            Direction.Left => (-1, 0),
            Direction.Right => (1, 0),
            Direction.Up => (0, -1),
            Direction.Down => (0, 1),
            _ => (0, 0)
        };
    }

    private Direction GetOppositeDirection(Direction dir) => dir switch
    {
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        _ => Direction.None
    };

    public void Reset(double startX, double startY)
    {
        X = startX;
        Y = startY;

        CurrentDirection = Direction.Left;
        DesiredDirection = Direction.Left;
    }
}

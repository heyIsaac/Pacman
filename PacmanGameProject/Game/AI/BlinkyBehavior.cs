using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

// Comportamento do fantasma Blinky -> Vermelho
// O foco é a perseguição de forma direta, mirando sempre no tile exato que o pacman está
public class BlinkyBehavior : IGhostBehavior
{
    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        // retorna posição atual do pacman no grid como alvo
        var pPos = pacman.GridPosition;
        return (pPos.X, pPos.Y);
    }
}

using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

// Interface para definir contrato de comportamento para cada fantasma
public interface IGhostBehavior
{
    // retorna tile alvo no grid que o fanstama deve perseguir
    (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky);
}

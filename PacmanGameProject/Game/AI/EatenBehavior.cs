using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

// Comportamento do fantasma no estado Eaten -> Olho voltando para centro
// Mira diretamente na porta da casa
public class EatenBehavior : IGhostBehavior
{
    // coords da porta
    private const int HOUSE_DOOR_COL = 13;
    private const int HOUSE_DOOR_ROW = 12;
    
    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        // retorna a porta da casa como destino
        return (HOUSE_DOOR_COL, HOUSE_DOOR_ROW);
    }
}

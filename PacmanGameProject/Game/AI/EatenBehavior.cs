using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.AI;

public class EatenBehavior : IGhostBehavior
{
    private const int HOUSE_DOOR_COL = 13;
    private const int HOUSE_DOOR_ROW = 12;
    
    public (int x, int y) GetTargetTile(Ghost self, Pacman pacman, Ghost? blinky)
    {
        return (HOUSE_DOOR_COL, HOUSE_DOOR_ROW);
    }
}

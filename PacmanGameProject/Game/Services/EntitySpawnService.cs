using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

// service para definir posiçoes inicias das entidades
public class EntitySpawnService : IEntitySpawnService
{
    private const int TILE_SIZE = 8;

    // posicionamento do pacman e fantasmas
    public void SpawnEntities(GameLoop gameLoop)
    {
                    // Posições iniciais dos fantasmas
                    gameLoop.Ghosts[0].X = 13 * TILE_SIZE;
                    gameLoop.Ghosts[0].Y = 13 * TILE_SIZE;
                    gameLoop.Ghosts[1].X = 14 * TILE_SIZE;
                    gameLoop.Ghosts[1].Y = 14 * TILE_SIZE;
                    gameLoop.Ghosts[2].X = 14 * TILE_SIZE;
                    gameLoop.Ghosts[2].Y = 14 * TILE_SIZE;
                    gameLoop.Ghosts[3].X = 14 * TILE_SIZE;
                    gameLoop.Ghosts[3].Y = 14 * TILE_SIZE;
                
                    // Posição inicial do Pacman
                    gameLoop.Pacman.X = 13 * TILE_SIZE;
                    gameLoop.Pacman.Y = 23 * TILE_SIZE;
    }
    
    // reseta as posiçoes apos morte ou reinicio do jogo
    public void ResetPositions(GameLoop gameLoop)
            {
                SpawnEntities(gameLoop);
                InputManager.DesiredDirection = Direction.None;
            }
}

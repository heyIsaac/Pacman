using PacmanGameProject.Game.Engine;

namespace PacmanGameProject.Game.Services.interfaces;

public interface IEntitySpawnService
{
    void SpawnEntities(GameLoop gameLoop);
    void ResetPositions(GameLoop gameLoop);
}

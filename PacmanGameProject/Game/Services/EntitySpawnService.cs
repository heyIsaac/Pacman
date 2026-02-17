using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Enums;
using PacmanGameProject.Game.Input;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class EntitySpawnService : IEntitySpawnService
{
    private const int TILE_SIZE = 8;

    // Definição Centralizada das Posições Iniciais
    private const double PACMAN_X = 13.5 * TILE_SIZE;
    private const double PACMAN_Y = 23 * TILE_SIZE;

    // Posições dos Fantasmas
    // Blinky: Acima da porta (Scatter/Fora)
    private const double BLINKY_X = 13 * TILE_SIZE;
    private const double BLINKY_Y = 11 * TILE_SIZE;

    // Pinky: Centro Casa
    private const double PINKY_X = 13 * TILE_SIZE;
    private const double PINKY_Y = 14 * TILE_SIZE;

    // Inky: Esq Casa
    private const double INKY_X = 11 * TILE_SIZE;
    private const double INKY_Y = 14 * TILE_SIZE;

    // Clyde: Dir Casa
    private const double CLYDE_X = 15 * TILE_SIZE;
    private const double CLYDE_Y = 14 * TILE_SIZE;

    public void SpawnEntities(GameLoop gameLoop)
    {
        // 1. Reseta Pacman
        if (gameLoop.Pacman != null)
        {
            gameLoop.Pacman.Reset(PACMAN_X, PACMAN_Y);
        }

        // 2. Reseta Fantasmas
        var blinky = gameLoop.Ghosts.FirstOrDefault(g => g.Type == GhostType.Blinky);
        if (blinky != null) blinky.Reset(BLINKY_X, BLINKY_Y, GhostState.Scatter);

        var pinky = gameLoop.Ghosts.FirstOrDefault(g => g.Type == GhostType.Pinky);
        if (pinky != null) pinky.Reset(PINKY_X, PINKY_Y, GhostState.InHouse);

        var inky = gameLoop.Ghosts.FirstOrDefault(g => g.Type == GhostType.Inky);
        if (inky != null) inky.Reset(INKY_X, INKY_Y, GhostState.InHouse);

        var clyde = gameLoop.Ghosts.FirstOrDefault(g => g.Type == GhostType.Clyde);
        if (clyde != null) clyde.Reset(CLYDE_X, CLYDE_Y, GhostState.InHouse);
    }

    public void ResetPositions(GameLoop gameLoop)
    {
        // 1. Reinicia o Timer do Jogo (Crucial para os fantasmas saírem da casinha de novo)
        gameLoop.ResetRoundTimer();

        // 2. Reposiciona fisicamente
        SpawnEntities(gameLoop);

        // 3. Reseta Inputs globais
        InputManager.DesiredDirection = Direction.Left;
    }
}

using PacmanGameProject.Game.Engine;
using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Rendering;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class GameInitializerService
{
    public GameLoop GameLoop { get; private set; }
    public SpriteRenderer Renderer { get; private set; }
    public IPelletService PelletService { get; private set; }
    public IGameStateService GameStateService { get; private set; }
    public IGhostCollisionService GhostCollisionService { get; private set; }
    public GameAudioService AudioService { get; private set; }
    public FrightenedModeService FrightenedService { get; private set; }
    public EntitySpawnService EntitySpawnService { get; private set; }

    private const int TILE_SIZE = 8;

    public void Initialize(
        Canvas mapCanvas,
        Image pacmanImage,
        List<Image> ghostImages,
        int[,] layout,
        List<Pellet> pellets,
        Dictionary<Pellet, Image> pelletSprites,
        Action activateFrightened,
        Action<int> onPelletEaten,
        Action<int> onScoreChanged,
        Action<int> onLifeChanged,
        Action onGameOver,
        Action onGameWon,
        Func<bool> isGameOver)
    {
        // Serviços base
        var collisionService = new CollisionService(layout, TILE_SIZE);

        AudioService = new GameAudioService();
        AudioService.SetupAudio();
        AudioService.SetupBackgroundMusic();

        GameStateService = new GameStateService();
        EntitySpawnService = new EntitySpawnService();
        FrightenedService = new FrightenedModeService();

        // GameLoop
        GameLoop = new GameLoop(collisionService);

        // Renderer
        Renderer = new SpriteRenderer(pacmanImage, ghostImages);

        // Pellets
        PelletService = new PelletService(pellets, pelletSprites, collisionService, mapCanvas, GameStateService);
        PelletService.OnPowerPelletEaten += activateFrightened;
        PelletService.OnPelletEaten += points =>
        {
            onPelletEaten(points);
            AudioService.PelletEaten();
        };

        // Ghost Collision
        GhostCollisionService = new GhostCollisionService(collisionService, GameStateService);
        GhostCollisionService.OnGhostEaten += AudioService.PlayEatGhost;
        GhostCollisionService.OnPacmanDied += () =>
        {
            if (!isGameOver())
                EntitySpawnService.ResetPositions(GameLoop);
        };

        // Frightened
        FrightenedService.OnFrightenedExpired += AudioService.ResumeNormalMusic;

        // State events
        GameStateService.OnScoreChanged += onScoreChanged;
        GameStateService.OnLifeChanged += onLifeChanged;
        GameStateService.OnGameOver += onGameOver;
        GameStateService.OnGameWon += onGameWon;

        // Spawn inicial
        EntitySpawnService.SpawnEntities(GameLoop);
    }
}


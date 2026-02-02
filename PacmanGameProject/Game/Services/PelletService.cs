using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class PelletService : IPelletService
{
    private readonly List<Pellet> _pellets;
    private readonly Dictionary<Pellet, Image> _sprites;
    private readonly ICollisionService _collisionService;
    private readonly Canvas _mapCanvas;

    private IGameStateService _gameStateService;

    public event Action<int>? OnPelletEaten;

    public PelletService(List<Pellet> pellets, Dictionary<Pellet, Image> sprites, ICollisionService collisionService,
        Canvas mapCanvas, IGameStateService gameStateService)
    {
        _pellets = pellets;
        _sprites = sprites;
        _collisionService = collisionService;
        _mapCanvas = mapCanvas;
        _gameStateService = gameStateService;
    }

    public void CheckCollision(Pacman pacman)
    {
        Pellet? collided = null;

        foreach (var pellet in _pellets)
        {
            if (_collisionService.Collides(pacman, pellet))
            {
                collided = pellet;
                break;
            }
        }

        if (collided == null) return;

        _mapCanvas.Children.Remove(_sprites[collided]);
        _sprites.Remove(collided);
        _pellets.Remove(collided);

        int points = collided.IsPowerPellet ? 50 : 10;
        OnPelletEaten?.Invoke(points);
    }
}

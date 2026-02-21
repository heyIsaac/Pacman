using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

// service controla os pellets
public class PelletService : IPelletService
{
    private readonly List<Pellet> _pellets;
    private readonly Dictionary<Pellet, Image> _sprites;
    private readonly ICollisionService _collisionService;
    private readonly Canvas _mapCanvas; // canvas onde ta desenhado pellets
    
    private IGameStateService _gameStateService;

    public event Action<int>? OnPelletEaten;
    public event Action? OnPowerPelletEaten;

    public PelletService(List<Pellet> pellets, Dictionary<Pellet, Image> sprites, ICollisionService collisionService,
        Canvas mapCanvas, IGameStateService gameStateService)
    {
        _pellets = pellets;
        _sprites = sprites;
        _collisionService = collisionService;
        _mapCanvas = mapCanvas;
        _gameStateService = gameStateService;
    }

    // verifica colisao pacman e os pellets
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

        if (_pellets.Count == 0)
        {
            _gameStateService.GameWon();
        }

        if (collided == null) return;

        _mapCanvas.Children.Remove(_sprites[collided]);
        _sprites.Remove(collided);
        _pellets.Remove(collided);

        if (collided.IsPowerPellet)
        {
            OnPowerPelletEaten?.Invoke();
            OnPelletEaten?.Invoke(50);
        }
        else
        {
            OnPelletEaten?.Invoke(10);
        }
        
    }
}

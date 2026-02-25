using PacmanGameProject.Game.Entities;

namespace PacmanGameProject.Game.Services.interfaces;

public interface IPelletService
{
    event Action<int> OnPelletEaten;
    void CheckCollision(Pacman pacman);
    event Action? OnPowerPelletEaten;
}

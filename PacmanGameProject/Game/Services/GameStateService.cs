using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class GameStateService : IGameStateService
{
    public int Lives { get; private set; } = 3;

    public event Action? OnGameOver;
    public event Action<int>? OnLifeChanged;

    public void PacmanDied()
    {
        Lives--;
        OnLifeChanged?.Invoke(Lives);
        
        if (Lives <= 0)
                OnGameOver?.Invoke();
    }
}

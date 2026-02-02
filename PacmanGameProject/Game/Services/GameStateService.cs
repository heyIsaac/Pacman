using PacmanGameProject.Game.Services.interfaces;

namespace PacmanGameProject.Game.Services;

public class GameStateService : IGameStateService
{
    public int Lives { get; private set; } = 3;
    public int Score { get; private set; } = 0;
    

    public event Action? OnGameOver;
    public event Action<int>? OnLifeChanged;
    public event Action<int>? OnScoreChanged;

    public void PacmanDied()
    {
        Lives--;
        OnLifeChanged?.Invoke(Lives);
        
        if (Lives <= 0)
                OnGameOver?.Invoke();
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }
    
    
}

namespace PacmanGameProject.Game.Services.interfaces;

public interface IGameStateService
{
    int Lives { get;  }
    int Score { get;  }
 
    event Action OnGameOver;
    event Action<int> OnLifeChanged;
    event Action<int> OnScoreChanged;
    void PacmanDied();
    void AddScore(int points);
}

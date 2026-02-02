namespace PacmanGameProject.Game.Services.interfaces;

public interface IGameStateService
{
    int Lives { get;  }
    event Action OnGameOver;
    event Action<int> OnLifeChanged;
    void PacmanDied();
}

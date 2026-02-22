using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Services;

public class FrightenedModeService
{
    private const int FRIGHTENED_TIME = 7000;

    private bool _isActive = false;
    private DateTime _startTime;

    public bool IsActive => _isActive;
    
    public double RemainingTime => 
        _isActive ? Math.Max(0, FRIGHTENED_TIME - (DateTime.Now - _startTime).TotalMilliseconds) : 0;

    public event Action? OnFrightenedExpired;

    public void Activate(List<Ghost> ghosts)
    {
        _isActive = true;
        _startTime = DateTime.Now;
        
        foreach (var ghost in ghosts)
        {
            if (ghost.CurrentState != GhostState.InHouse && ghost.CurrentState != GhostState.Eaten)
            {
                ghost.CurrentState = GhostState.Frightened;
                ghost.ForceReverseDirection();
            }
        }
    }

    public void Update(List<Ghost> ghosts)
    {
        if (!_isActive) return;

        if (RemainingTime <= 0)
        {
            _isActive = false;

            foreach (var ghost in ghosts)
            {
                if (ghost.CurrentState == GhostState.Frightened)
                    ghost.CurrentState = GhostState.Scatter;
            }

            OnFrightenedExpired?.Invoke();
        }
    }
}

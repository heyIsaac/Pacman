using PacmanGameProject.Game.Entities;
using PacmanGameProject.Game.Enums;

namespace PacmanGameProject.Game.Services;

// Classe responsável em gerenciar o modo Frightened 
public class FrightenedModeService
{
    private const int FRIGHTENED_TIME = 7000; // duração modo

    private bool _isActive = false;
    private DateTime _startTime;

    public bool IsActive => _isActive; // true quando modo ativado
    
    // Retorna o tempo restante em ms para piscar
    public double RemainingTime => 
        _isActive ? Math.Max(0, FRIGHTENED_TIME - (DateTime.Now - _startTime).TotalMilliseconds) : 0;

    // Evento que dispara quando modo expira
    public event Action? OnFrightenedExpired;

    // Ativa modo Frightened
    public void Activate(List<Ghost>
        ghosts)
    {
        _isActive = true;
        _startTime = DateTime.Now;
        
        foreach (var ghost in ghosts)
        {
            // Apenas fantasmas fora da casa e que não estão voltando são afetados
            if (ghost.CurrentState != GhostState.InHouse && ghost.CurrentState != GhostState.Eaten)
            {
                ghost.CurrentState = GhostState.Frightened;
                ghost.ForceReverseDirection();
            }
        }
    }

    // Att a cada frame, desativando modo quando timer expira
    public void Update(List<Ghost> ghosts)
    {
        if (!_isActive) return;

        if (RemainingTime <= 0)
        {
            _isActive = false;
            
            // volta fantasmas assustados modo Scatter
            foreach (var ghost in ghosts)
            {
                if (ghost.CurrentState == GhostState.Frightened)
                    ghost.CurrentState = GhostState.Scatter;
            }

            OnFrightenedExpired?.Invoke(); // volta musica normal
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmanGameProject.Game.Models;

// Representa a entrada no ranking de pontuações
public class ScoreEntry
{
    public string PlayerName { get; set; } // Nome jogador
    public int Score { get; set; } // Pontos
    public DateTime Date { get; set; } // Data e hora partida

    // Propriedade auxiliar para exibir a data formatada no XAML sem complicação
    public string FormattedDate => Date.ToString("dd/MM/yyyy HH:mm");
}

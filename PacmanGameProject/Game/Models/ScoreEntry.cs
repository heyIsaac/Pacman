using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmanGameProject.Game.Models;

public class ScoreEntry
{
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public DateTime Date { get; set; }

    // Propriedade auxiliar para exibir a data formatada no XAML sem complicação
    public string FormattedDate => Date.ToString("dd/MM/yyyy HH:mm");
}

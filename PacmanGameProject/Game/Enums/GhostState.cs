using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacmanGameProject.Game.Enums;

public enum GhostState
{
    Scatter,    // Espalhar (cada um para seu canto)
    Chase,      // Perseguir 
    Frightened, // Assustado 
    Eaten,      // Olhos voltando pra casa
    InHouse     // Dentro da casa
}

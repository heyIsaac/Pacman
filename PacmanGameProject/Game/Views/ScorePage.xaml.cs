using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PacmanGameProject.Game.Services;
using System.Collections.Generic;
using System.Linq;

namespace PacmanGameProject.Game.Views;

public sealed partial class ScorePage : Page
{
    public ScorePage()
    {
        this.InitializeComponent();
        LoadScores();
    }

    private void LoadScores()
    {
        // Jogadores estáticos
        var allScores = new List<ScoreRecord>
        {
            new ScoreRecord { Name = "Isaac", Points = 10000 },
            new ScoreRecord { Name = "Vinicius", Points = 400 },
            new ScoreRecord { Name = "Miguel", Points = 300 },
            new ScoreRecord { Name = "Eduarda", Points = 200 },
            new ScoreRecord { Name = "Pedro", Points = -50000 }
        };

        //  Carrega o histórico do player salvo no JSON
        var savedScores = ScoreService.LoadScores();

        // Junta as duas listas
        allScores.AddRange(savedScores);

        // Ordena do maior para o menor (Ranking) e pega os top 10
        var finalRanking = allScores.OrderByDescending(s => s.Points).Take(10).ToList();

        ScoreList.ItemsSource = finalRanking;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
        else
        {
            // volta pro menu se o histórico estiver vazio
            Frame.Navigate(typeof(MenuPage));
        }
    }
}

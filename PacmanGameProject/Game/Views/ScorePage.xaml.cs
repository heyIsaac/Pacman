using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
        // Score
        var scores = new[]
        {
            new { Name = "Isaac", Points = 10000 },
            new { Name = "Vinicius", Points = 400 },
            new { Name = "Miguel", Points = 300 },
            new { Name = "Eduarda", Points = 200 },
            new { Name = "Pedro", Points = -50000 },
            
        };

        ScoreList.ItemsSource = scores;
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

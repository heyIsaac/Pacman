using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PacmanGameProject.Game.Views;

public sealed partial class MenuPage : Page
{
    public MenuPage()
    {
        this.InitializeComponent();
    }

    private void OnStartGameClicked(object sender, RoutedEventArgs e)
    {
        // Navega para a MainPage (onde está o seu jogo atual)
        Frame.Navigate(typeof(MainPage));
    }

    private void OnScoreboardClicked(object sender, RoutedEventArgs e)
    {
        // Navega para a tela de pontuação
        Frame.Navigate(typeof(ScorePage));
    }

    private void OnExitClicked(object sender, RoutedEventArgs e)
    {
        // Fecha a aplicação
        Application.Current.Exit();
    }
}

// App.xaml.cs
using System.Windows;
using System.Windows.Input;

namespace MemoryMatchGame
{
    public partial class App : Application
    {
        // Gestion du déplacement de la fenêtre personnalisée
        public static void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                var window = Window.GetWindow((DependencyObject)sender);
                if (window != null)
                {
                    window.DragMove();
                }
            }
        }

        // Bouton de fermeture
        public static void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
            {
                window.Close();
            }
        }

        // Bouton de minimisation
        public static void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow((DependencyObject)sender);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
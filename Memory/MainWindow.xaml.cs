// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MemoryMatchGame
{
    public partial class MainWindow : Window
    {
        // Timer pour l'animation
        private System.Windows.Threading.DispatcherTimer? animationTimer;
    
        private List<Card> cards = new List<Card>();
        private HashSet<string> matchedPairs = new HashSet<string>();
        private List<int> flippedCardIndices = new List<int>();
        private bool isLocked = false;
        private string[] symbols = { "☀️", "❤️", "🌙", "⭐", "☁️", "🌸" };
        private List<Button> cardButtons = new List<Button>();
        private Random random = new Random();
        private DispatcherTimer? timer;

        public MainWindow()
        {
            InitializeComponent();
            NewGameButton.Click += (s, e) => InitGame();
            InitGame();
        }

        private void InitGame()
        {
            // Réinitialiser les variables du jeu
            matchedPairs.Clear();
            flippedCardIndices.Clear();
            isLocked = false;
            UpdateMatchesDisplay();

            // Mélanger les cartes
            cards = symbols.Concat(symbols)
                .Select((symbol, index) => new Card
                {
                    Id = index,
                    Symbol = symbol,
                    IsFlipped = false,
                    IsMatched = false
                })
                .OrderBy(_ => random.Next())
                .ToList();

            RenderBoard();
        }

        private void RenderBoard()
        {
            // Nettoyer le plateau de jeu
            GameBoard.Children.Clear();
            cardButtons.Clear();

            // Créer la grille 4x3
            for (int i = 0; i < 12; i++)
            {
                var card = cards[i];
                
                Button cardButton = new Button
                {
                    Width = 120,
                    Height = 120,
                    Margin = new Thickness(10),
                    Tag = i,
                    Background = new SolidColorBrush(Color.FromRgb(43, 36, 100)), // #2B2464
                    BorderThickness = new Thickness(0),
                    FontSize = 40,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                // Appliquer les styles pour les cartes retournées
                if (card.IsFlipped || card.IsMatched)
                {
                    cardButton.Content = card.Symbol;
                    cardButton.Background = new SolidColorBrush(Color.FromRgb(53, 43, 123)); // #352B7B
                }

                cardButton.Click += CardButton_Click;
                
                // Ajouter à la grille et à la liste
                GameBoard.Children.Add(cardButton);
                cardButtons.Add(cardButton);
            }
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLocked) return;
            
            var cardButton = (Button)sender;
            int index = (int)cardButton.Tag;
            var card = cards[index];

            if (card.IsFlipped || card.IsMatched || flippedCardIndices.Contains(index))
            {
                return;
            }

            // Retourner la carte
            card.IsFlipped = true;
            flippedCardIndices.Add(index);
            cardButton.Content = card.Symbol;
            cardButton.Background = new SolidColorBrush(Color.FromRgb(53, 43, 123)); // #352B7B

            if (flippedCardIndices.Count == 2)
            {
                isLocked = true;
                CheckMatch();
            }
        }

        private async void CheckMatch()
        {
            int firstIndex = flippedCardIndices[0];
            int secondIndex = flippedCardIndices[1];
            var firstCard = cards[firstIndex];
            var secondCard = cards[secondIndex];

            if (firstCard.Symbol == secondCard.Symbol)
            {
                // Les cartes correspondent
                firstCard.IsMatched = true;
                secondCard.IsMatched = true;
                matchedPairs.Add(firstCard.Symbol);
                UpdateMatchesDisplay();

                // Réinitialiser le tour
                ResetTurn();

                // Vérifier la victoire
                if (matchedPairs.Count == symbols.Length)
                {
                    await Task.Delay(500); // Petite pause avant d'afficher la victoire
                    ShowVictoryAnimation();
                }
            }
            else
            {
                // Les cartes ne correspondent pas
                await Task.Delay(1000);
                
                // Remettre les cartes face cachée
                firstCard.IsFlipped = false;
                secondCard.IsFlipped = false;
                cardButtons[firstIndex].Content = null;
                cardButtons[firstIndex].Background = new SolidColorBrush(Color.FromRgb(43, 36, 100)); // #2B2464
                cardButtons[secondIndex].Content = null;
                cardButtons[secondIndex].Background = new SolidColorBrush(Color.FromRgb(43, 36, 100)); // #2B2464
                
                ResetTurn();
            }
        }

        private void ResetTurn()
        {
            flippedCardIndices.Clear();
            isLocked = false;
        }

        private void UpdateMatchesDisplay()
        {
            MatchesText.Text = $"{matchedPairs.Count} of 6";
        }

        private void ShowVictoryAnimation()
        {
            // Créer la fenêtre de victoire
            var victoryWindow = new Window
            {
                Title = "Victoire",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new SolidColorBrush(Color.FromRgb(53, 43, 123)),
                ResizeMode = ResizeMode.NoResize
            };

            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var titleTextBlock = new TextBlock
            {
                Text = "🎉 Félicitations ! 🎉",
                FontSize = 24,
                Foreground = new SolidColorBrush(Color.FromRgb(229, 184, 232)), // #E5B8E8
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var messageTextBlock = new TextBlock
            {
                Text = "Vous avez trouvé toutes les paires !",
                FontSize = 16,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            };

            var playAgainButton = new Button
            {
                Content = "Rejouer",
                Width = 150,
                Height = 50,
                FontSize = 16,
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(139, 129, 210), 0), // #8B81D2
                        new GradientStop(Color.FromRgb(91, 77, 188), 1)    // #5B4DBC
                    }
                },
                Foreground = Brushes.White
            };

            playAgainButton.Click += (s, e) =>
            {
                victoryWindow.Close();
                InitGame();
            };

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(messageTextBlock);
            stackPanel.Children.Add(playAgainButton);

            victoryWindow.Content = stackPanel;
            victoryWindow.ShowDialog();
        }
    }

    public class Card
    {
        public int Id { get; set; }
        public string? Symbol { get; set; }
        public bool IsFlipped { get; set; }
        public bool IsMatched { get; set; }
    }
}
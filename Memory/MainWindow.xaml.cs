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
    {
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

        private async void InitGame()
        {
            // Réinitialiser les variables du jeu
            matchedPairs.Clear();
            flippedCardIndices.Clear();
            isLocked = false;
            UpdateMatchesDisplay();

            // Effet de chargement pendant le mélange
            if (GameBoard.Children.Count > 0)
            {
                // Petite animation de transition
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                GameBoard.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                
                await Task.Delay(350);
            }

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
            
            // Animation d'entrée des cartes
            GameBoard.Opacity = 0;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            GameBoard.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            
            // Animation séquentielle des cartes
            for (int i = 0; i < cardButtons.Count; i++)
            {
                var card = cardButtons[i];
                card.Opacity = 0;
                
                var cardAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150))
                {
                    BeginTime = TimeSpan.FromMilliseconds(i * 50)
                };
                
                var scaleTransform = new ScaleTransform(0.8, 0.8);
                card.RenderTransform = scaleTransform;
                card.RenderTransformOrigin = new Point(0.5, 0.5);
                
                var scaleXAnimation = new DoubleAnimation(0.8, 1.0, TimeSpan.FromMilliseconds(250))
                {
                    BeginTime = TimeSpan.FromMilliseconds(i * 50),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                
                var scaleYAnimation = new DoubleAnimation(0.8, 1.0, TimeSpan.FromMilliseconds(250))
                {
                    BeginTime = TimeSpan.FromMilliseconds(i * 50),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                
                card.BeginAnimation(UIElement.OpacityProperty, cardAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
            }
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
                    Style = (Style)FindResource("CardStyle"),
                    Tag = i
                };

                // Appliquer les styles pour les cartes retournées
                if (card.IsFlipped || card.IsMatched)
                {
                    cardButton.Content = card.Symbol;
                    
                    // Changer le style pour les cartes retournées
                    var border = (cardButton.Template.FindName("CardBorder", cardButton) as Border);
                    if (border != null)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(53, 43, 123)); // #352B7B
                        
                        // Ajouter un effet plus prononcé pour les cartes correspondantes
                        if (card.IsMatched)
                        {
                            border.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 184, 232)); // #E5B8E8
                            border.BorderThickness = new Thickness(3);
                            border.Effect = new DropShadowEffect
                            {
                                Color = Color.FromRgb(229, 184, 232),
                                ShadowDepth = 0,
                                BlurRadius = 15,
                                Opacity = 0.7
                            };
                        }
                    }
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

            // Animation de retournement
            FlipCard(cardButton, card);
            
            // Retourner la carte
            card.IsFlipped = true;
            flippedCardIndices.Add(index);

            if (flippedCardIndices.Count == 2)
            {
                isLocked = true;
                CheckMatch();
            }
        }
        
        private void FlipCard(Button cardButton, Card card)
        {
            // Préparation de l'animation
            var scaleTransform = new ScaleTransform();
            cardButton.RenderTransform = scaleTransform;
            cardButton.RenderTransformOrigin = new Point(0.5, 0.5);
            
            // Animation de retournement
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                AutoReverse = true
            };
            
            // Au milieu de l'animation, changer le contenu
            animation.Completed += (s, e) => 
            {
                cardButton.Content = card.Symbol;
                
                // Appliquer le style retourné
                var border = (cardButton.Template.FindName("CardBorder", cardButton) as Border);
                if (border != null)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(53, 43, 123)); // #352B7B
                }
            };
            
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        }

        private async void CheckMatch()
        {
            int firstIndex = flippedCardIndices[0];
            int secondIndex = flippedCardIndices[1];
            var firstCard = cards[firstIndex];
            var secondCard = cards[secondIndex];
            var firstButton = cardButtons[firstIndex];
            var secondButton = cardButtons[secondIndex];

            if (firstCard.Symbol == secondCard.Symbol)
            {
                // Les cartes correspondent
                await Task.Delay(300);
                
                // Animation de correspondance trouvée
                AnimateMatchedCards(firstButton, secondButton);
                
                firstCard.IsMatched = true;
                secondCard.IsMatched = true;
                matchedPairs.Add(firstCard.Symbol);
                UpdateMatchesDisplay();

                // Réinitialiser le tour
                ResetTurn();

                // Vérifier la victoire
                if (matchedPairs.Count == symbols.Length)
                {
                    await Task.Delay(800); // Pause avant d'afficher la victoire
                    ShowVictoryAnimation();
                }
            }
            else
            {
                // Les cartes ne correspondent pas
                await Task.Delay(1000);
                
                // Animation de retournement inversé
                FlipCardBack(firstButton);
                FlipCardBack(secondButton);
                
                // Remettre les cartes face cachée
                firstCard.IsFlipped = false;
                secondCard.IsFlipped = false;
                
                ResetTurn();
            }
        }
        
        private void AnimateMatchedCards(Button firstButton, Button secondButton)
        {
            Action<Button> animateMatch = (button) => 
            {
                var border = button.Template.FindName("CardBorder", button) as Border;
                if (border != null)
                {
                    // Animation de pulsation
                    var scaleTransform = new ScaleTransform(1, 1);
                    border.RenderTransform = scaleTransform;
                    border.RenderTransformOrigin = new Point(0.5, 0.5);
                    
                    var pulseAnimation = new DoubleAnimation
                    {
                        From = 1,
                        To = 1.1,
                        Duration = TimeSpan.FromMilliseconds(200),
                        AutoReverse = true
                    };
                    
                    // Bordure et effet de réussite
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 184, 232)); // #E5B8E8
                    border.BorderThickness = new Thickness(3);
                    
                    // Effet de lueur
                    border.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(229, 184, 232),
                        ShadowDepth = 0,
                        BlurRadius = 15,
                        Opacity = 0.7
                    };
                    
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
                }
            };
            
            animateMatch(firstButton);
            animateMatch(secondButton);
        }
        
        private void FlipCardBack(Button cardButton)
        {
            var scaleTransform = new ScaleTransform();
            cardButton.RenderTransform = scaleTransform;
            cardButton.RenderTransformOrigin = new Point(0.5, 0.5);
            
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(150),
                AutoReverse = true
            };
            
            animation.Completed += (s, e) => 
            {
                cardButton.Content = null;
                
                var border = (cardButton.Template.FindName("CardBorder", cardButton) as Border);
                if (border != null)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(43, 36, 100)); // #2B2464
                    border.Effect = new DropShadowEffect
                    {
                        ShadowDepth = 4,
                        Direction = 315,
                        Color = Colors.Black,
                        Opacity = 0.5,
                        BlurRadius = 10
                    };
                }
            };
            
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        }
        }

        private void ResetTurn()
        {
            flippedCardIndices.Clear();
            isLocked = false;
        }

        private void UpdateMatchesDisplay()
        {
            // Animation de mise à jour du compteur
            var oldText = MatchesText.Text;
            var newText = $"{matchedPairs.Count} of 6";
            
            if (oldText != newText)
            {
                // Animation de mise à jour uniquement si la valeur change
                var scaleTransform = new ScaleTransform(1, 1);
                MatchesText.RenderTransform = scaleTransform;
                MatchesText.RenderTransformOrigin = new Point(0.5, 0.5);
                
                var scaleAnimation = new DoubleAnimation(1, 1.3, TimeSpan.FromMilliseconds(150))
                {
                    AutoReverse = true
                };
                
                // Changement de couleur pour l'effet visuel
                var originalBrush = MatchesText.Foreground;
                var highlightBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Gold
                
                MatchesText.Foreground = highlightBrush;
                MatchesText.Text = newText;
                
                scaleAnimation.Completed += (s, e) => {
                    MatchesText.Foreground = originalBrush;
                };
                
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }
            else
            {
                MatchesText.Text = newText;
            }
            
            // Vérifier la progression et mettre à jour l'interface en conséquence
            if (matchedPairs.Count > 0)
            {
                // Calculer un pourcentage de progression (0-100%)
                double progressPercent = (double)matchedPairs.Count / symbols.Length * 100;
                
                // Mettre à jour l'apparence en fonction de la progression
                if (progressPercent >= 50)
                {
                    // Effet spécial quand on atteint la moitié
                    var gameBoard = this.FindName("GameBoardContainer") as Border;
                    if (gameBoard != null && progressPercent == 50) // exactement à 50%
                    {
                        var pulseAnimation = new DoubleAnimation(1, 1.03, TimeSpan.FromMilliseconds(300))
                        {
                            AutoReverse = true,
                            RepeatBehavior = new RepeatBehavior(2)
                        };
                        
                        var scaleTransform = new ScaleTransform(1, 1);
                        gameBoard.RenderTransform = scaleTransform;
                        gameBoard.RenderTransformOrigin = new Point(0.5, 0.5);
                        
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnimation);
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnimation);
                    }
                }
            }
        }

        private void ShowVictoryAnimation()
        {
            // Créer la fenêtre de victoire avec un style élégant
            var victoryWindow = new Window
            {
                Title = "Victoire",
                Width = 450,
                Height = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent
            };

            // Conteneur principal avec bord arrondi
            var mainBorder = new Border
            {
                CornerRadius = new CornerRadius(20),
                Padding = new Thickness(30),
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(42, 27, 105), 0), // #2A1B69
                        new GradientStop(Color.FromRgb(33, 21, 84), 1)    // #211554
                    }
                },
                Effect = new DropShadowEffect
                {
                    BlurRadius = 25,
                    ShadowDepth = 0,
                    Color = Colors.Black,
                    Opacity = 0.7
                }
            };

            // Ajout d'un motif d'arrière-plan subtil
            var patternOverlay = new Border
            {
                CornerRadius = new CornerRadius(20),
                Background = new DrawingBrush
                {
                    TileMode = TileMode.Tile,
                    Viewport = new Rect(0, 0, 30, 30),
                    ViewportUnits = BrushMappingMode.Absolute,
                    Drawing = new GeometryDrawing
                    {
                        Brush = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
                        Geometry = new EllipseGeometry(new Point(15, 15), 1, 1)
                    }
                }
            };

            // Création du contenu principal
            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Titre avec effet spécial
            var titleBlock = new TextBlock
            {
                Text = "🎉 Félicitations ! 🎉",
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                Foreground = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(229, 184, 232), 0), // #E5B8E8
                        new GradientStop(Color.FromRgb(180, 140, 210), 1)  // Couleur plus foncée
                    }
                },
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 3,
                    Direction = 320,
                    Color = Colors.Black,
                    Opacity = 0.5,
                    BlurRadius = 5
                }
            };

            // Animation du titre
            var titleTransform = new TranslateTransform();
            titleBlock.RenderTransform = titleTransform;
            titleBlock.RenderTransformOrigin = new Point(0.5, 0.5);

            var titleAnimation = new DoubleAnimation
            {
                From = -10,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new ElasticEase { Oscillations = 2, Springiness = 5 }
            };

            // Message de victoire
            var messageBlock = new TextBlock
            {
                Text = "Vous avez trouvé toutes les paires !",
                FontSize = 18,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 30),
                TextAlignment = TextAlignment.Center,
                Opacity = 0 // Commencer invisible pour l'animation
            };

            // Animation de fondu pour le message
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                BeginTime = TimeSpan.FromMilliseconds(400)
            };

            // Création d'un conteneur pour les confettis virtuels
            var confettiCanvas = new Canvas
            {
                Width = 450,
                Height = 350,
                IsHitTestVisible = false
            };

            // Création du bouton de rejouer avec style
            var playAgainButton = new Button
            {
                Content = "Rejouer",
                Width = 180,
                Height = 55,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                Cursor = System.Windows.Input.Cursors.Hand,
                Opacity = 0 // Pour l'animation
            };

            // Style du bouton
            playAgainButton.Template = new ControlTemplate(typeof(Button))
            {
                VisualTree = new FrameworkElementFactory(typeof(Border), "border")
            };
            var buttonBorder = new Border
            {
                CornerRadius = new CornerRadius(27.5),
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
                Effect = new DropShadowEffect
                {
                    ShadowDepth = 5,
                    Direction = 315,
                    Color = Colors.Black,
                    Opacity = 0.4,
                    BlurRadius = 10
                }
            };

            var buttonContentPresenter = new ContentPresenter
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            buttonBorder.Child = buttonContentPresenter;
            playAgainButton.Template = new ControlTemplate(typeof(Button));
            playAgainButton.Template.VisualTree = new FrameworkElementFactory(typeof(Border), "border");
            playAgainButton.ApplyTemplate();
            var templateBorder = playAgainButton.Template.FindName("border", playAgainButton) as Border;
            if (templateBorder != null)
            {
                templateBorder.CornerRadius = new CornerRadius(27.5);
                templateBorder.Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromRgb(139, 129, 210), 0),
                        new GradientStop(Color.FromRgb(91, 77, 188), 1)
                    }
                };
                templateBorder.Effect = new DropShadowEffect
                {
                    ShadowDepth = 5,
                    Direction = 315,
                    Color = Colors.Black,
                    Opacity = 0.4,
                    BlurRadius = 10
                };
                
                var contentPresenter = new ContentPresenter
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                templateBorder.Child = contentPresenter;
            }

            // Animation pour le bouton
            var buttonAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = TimeSpan.FromMilliseconds(800)
            };

            // Gestion de l'événement du bouton avec animation de sortie
            playAgainButton.Click += async (s, e) =>
            {
                // Animation de sortie des confettis
                foreach (var confetti in confettiCanvas.Children.OfType<Border>())
                {
                    var fallAnimation = new DoubleAnimation
                    {
                        To = 400,
                        Duration = TimeSpan.FromMilliseconds(500)
                    };
                    confetti.BeginAnimation(Canvas.TopProperty, fallAnimation);
                }
                
                // Animation de sortie de la fenêtre
                var opacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));
                var scaleTransform = new ScaleTransform(1, 1);
                mainGrid.RenderTransform = scaleTransform;
                mainGrid.RenderTransformOrigin = new Point(0.5, 0.5);
                
                var scaleAnimation = new DoubleAnimation(1, 0.8, TimeSpan.FromMilliseconds(500));
                
                mainGrid.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
                
                await Task.Delay(550);
                victoryWindow.Close();
                InitGame();
            };

            // Ajout des éléments au panel
            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(messageBlock);
            stackPanel.Children.Add(playAgainButton);

            // Montage des conteneurs
            Grid mainGrid = new Grid();
            mainGrid.Children.Add(mainBorder);
            mainGrid.Children.Add(patternOverlay);
            mainGrid.Children.Add(stackPanel);
            mainGrid.Children.Add(confettiCanvas);

            victoryWindow.Content = mainGrid;

            // Démarrage des animations après chargement
            victoryWindow.Loaded += (s, e) =>
            {
                titleTransform.BeginAnimation(TranslateTransform.YProperty, titleAnimation);
                messageBlock.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
                playAgainButton.BeginAnimation(UIElement.OpacityProperty, buttonAnimation);
                
                // Création et animation des confettis
                Random random = new Random();
                for (int i = 0; i < 50; i++)
                {
                    var confetti = new Border
                    {
                        Width = random.Next(5, 12),
                        Height = random.Next(5, 12),
                        CornerRadius = new CornerRadius(random.Next(0, 2) == 0 ? 0 : 6),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new RotateTransform(random.Next(0, 360))
                    };

                    // Couleurs variées pour les confettis
                    Color[] confettiColors = new[]
                    {
                        Color.FromRgb(229, 184, 232), // #E5B8E8
                        Color.FromRgb(139, 129, 210), // #8B81D2
                        Color.FromRgb(91, 77, 188),   // #5B4DBC
                        Color.FromRgb(255, 215, 0),   // Gold
                        Color.FromRgb(135, 206, 235)  // SkyBlue
                    };

                    confetti.Background = new SolidColorBrush(confettiColors[random.Next(0, confettiColors.Length)]);
                    Canvas.SetLeft(confetti, random.Next(0, 450));
                    Canvas.SetTop(confetti, -random.Next(10, 50));
                    confettiCanvas.Children.Add(confetti);

                    // Animation de chute
                    var fallAnimation = new DoubleAnimation
                    {
                        From = Canvas.GetTop(confetti),
                        To = 400,
                        Duration = TimeSpan.FromSeconds(random.Next(3, 6)),
                        BeginTime = TimeSpan.FromMilliseconds(random.Next(0, 1500))
                    };

                    // Animation de rotation
                    var rotateTransform = confetti.RenderTransform as RotateTransform;
                    var rotateAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 360 * random.Next(1, 4) * (random.Next(0, 2) == 0 ? 1 : -1),
                        Duration = TimeSpan.FromSeconds(random.Next(3, 6)),
                        BeginTime = TimeSpan.FromMilliseconds(random.Next(0, 500))
                    };

                    confetti.BeginAnimation(Canvas.TopProperty, fallAnimation);
                    rotateTransform?.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
                }
            };

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
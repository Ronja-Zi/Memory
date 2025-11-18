using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Memory
{
    public partial class MainWindow : Window
    {
        // --------------- FELDER ----------------
        private List<MemoryCard> cards = new List<MemoryCard>();
        private Random rnd = new Random();
        private int moves = 0;

        private int currentPlayer = 1;
        private int[] scores = new int[2];

        private DispatcherTimer gameTimer = new DispatcherTimer();
        private TimeSpan elapsedTime = TimeSpan.Zero;

        private MemoryCard? firstCard;
        private MemoryCard? secondCard;

        private bool isBusy = false;

        // Rahmenfarben für Spieler
        private readonly Brush player1Brush = Brushes.DeepPink;
        private readonly Brush player2Brush = Brushes.SkyBlue;

        public MainWindow()
        {
            InitializeComponent();

            // Timer EINMAL konfigurieren
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += GameTimer_Tick;

            InitializeGame();
            StartTimer();
        }

        // --------------- NEUES SPIEL BUTTON ----------------
        private void BtnNewGame_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Stop();
            InitializeGame();
            StartTimer();
        }

        // ---------------- TIMER TICK EVENT ----------------
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
            TxtTime.Text = elapsedTime.ToString(@"mm\:ss");
        }



        // ---------------- SPIEL INITIALISIEREN ----------------
        private void InitializeGame()
        {
            // Basisordner (bin\Debug\...\)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Images-Ordner dort drin
            string imageFolder = System.IO.Path.Combine(baseDir, "Images");

            // Alle Bilddateien holen
            var rawImages = System.IO.Directory.GetFiles(imageFolder, "*.png")
                               .Where(f => !f.ToLower().Contains("cover"))
                               .ToList();

            // Gruppieren nach Motiv (Dateiname ohne letzte Zahl)
            var allFrontImages = rawImages
                .GroupBy(path =>
                {
                    string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                    // alles außer den letzten char nehmen, falls es eine Zahl ist
                    return char.IsDigit(filename.Last()) ? filename[..^1] : filename;
                })
                .Select(g => g.First())  // nur EIN Motivbild auswählen
                .ToList();


            if (allFrontImages.Count < 8)
            {
                MessageBox.Show("Du brauchst mindestens 8 Kartenbilder (ohne 'cover' im Namen) im Ordner 'Images'.");
                return;
            }

            // Cover-Bilder (mind. eins mit 'cover' im Dateinamen)
            var coverImages = System.IO.Directory.GetFiles(imageFolder, "*.png")
                                 .Where(f => f.ToLower().Contains("cover"))
                                 .ToList();

            if (coverImages.Count == 0)
            {
                MessageBox.Show("Kein Cover-Bild gefunden. Lege ein Bild mit 'cover' im Dateinamen in den Images-Ordner.");
                return;
            }

            string coverPath = coverImages[0];

            // 8 verschiedene Motive zufällig auswählen
            var selectedFronts = allFrontImages
                                 .OrderBy(x => rnd.Next())
                                 .Take(8)
                                 .ToList();

            // von jedem Motiv zwei Karten → 16 Einträge
            var cardImages = new List<string>();
            foreach (var img in selectedFronts)
            {
                cardImages.Add(img);
                cardImages.Add(img);
            }

            // alle 16 Karten mischen
            cardImages = cardImages.OrderBy(x => rnd.Next()).ToList();

            // alte Karten löschen
            cards.Clear();
            CardGrid.Children.Clear();

            // 4x4 Grid befüllen
            for (int i = 0; i < cardImages.Count; i++)
            {
                var card = new MemoryCard
                {
                    ImagePath = cardImages[i],   // voller Pfad
                    CoverPath = coverPath,       // voller Pfad
                    Button = new Button
                    {
                        Tag = i,
                        Background = Brushes.Transparent,          // graue Umrandung weg
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(4),
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Stretch,
                       
                    }


                };

                SetButtonCover(card);
                card.Button.Click += Card_Click;

                int row = i / 4;
                int col = i % 4;

                Grid.SetRow(card.Button, row);
                Grid.SetColumn(card.Button, col);

                CardGrid.Children.Add(card.Button);
                cards.Add(card);
            }

            // Variablen zurücksetzen
            moves = 0;
            TxtMoves.Text = "0";

            elapsedTime = TimeSpan.Zero;
            TxtTime.Text = "00:00";

            currentPlayer = 1;
            scores[0] = 0;
            scores[1] = 0;
            UpdatePlayerDisplay();
            UpdateScoreDisplay();


            firstCard = null;
            secondCard = null;
            isBusy = false;
        }

        // ---------------- TIMER STARTEN ----------------
        private void StartTimer()
        {
            elapsedTime = TimeSpan.Zero;
            TxtTime.Text = "00:00";
            gameTimer.Start();
        }

        // ---------------- KLICK EVENT ----------------
        private async void Card_Click(object sender, RoutedEventArgs e)
        {
            if (isBusy) return;

            if (sender is not Button btn) return;
            int index = (int)btn.Tag;
            var card = cards[index];

            if (card.IsFlipped || card.IsMatched) return;

            ShowCard(card);

            if (firstCard == null)
            {
                firstCard = card;
                return;
            }

            // zweite Karte
            secondCard = card;
            moves++;
            TxtMoves.Text = moves.ToString();
            isBusy = true;

            if (firstCard.ImagePath == secondCard.ImagePath)
            {
                // Treffer
                firstCard.IsMatched = true;
                secondCard.IsMatched = true;

                // Karten mit Spielerfarbe markieren
                MarkCardOwner(firstCard, currentPlayer);
                MarkCardOwner(secondCard, currentPlayer);

                
                // Punkt hinzufügen
                scores[currentPlayer - 1]++;
                UpdateScoreDisplay();

                UpdatePlayerDisplay();
                ResetSelection();
                CheckForGameEnd();
            }


            else
            {
                // Kein Treffer → 0,8s warten, dann umdrehen
                await Task.Delay(800);

                HideCard(firstCard);
                HideCard(secondCard);

                // Spielerwechsel (1 ↔ 2)
                currentPlayer = 3 - currentPlayer;
                UpdatePlayerDisplay();

                ResetSelection();

            }
        }

        private void ResetSelection()
        {
            firstCard = null;
            secondCard = null;
            isBusy = false;
        }

        // ---------------- KARTEN ANZEIGEN ----------------
        private void ShowCard(MemoryCard card)
        {
            var img = new Image
            {
                Source = new BitmapImage(new Uri(card.ImagePath, UriKind.Absolute)),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(4)
            };

            card.Button.Content = img;
            card.IsFlipped = true;
        }

        private void HideCard(MemoryCard card)
        {
            SetButtonCover(card);
            card.IsFlipped = false;
        }

        private void SetButtonCover(MemoryCard card)
        {
            var img = new Image
            {
                Source = new BitmapImage(new Uri(card.CoverPath, UriKind.Absolute)),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(4)
            };

            card.Button.Content = img;

        }
        // Karte mit Spielerfarbe umranden
        private void MarkCardOwner(MemoryCard card, int player)
        {
            Brush brush = player == 1 ? player1Brush : player2Brush;
            card.Button.BorderBrush = brush;
            card.Button.BorderThickness = new Thickness(4);
        }
        private void UpdatePlayerDisplay()
        {
            TxtPlayer.Text = $"Spieler {currentPlayer}";
            TxtPlayer.Foreground = currentPlayer == 1 ? player1Brush : player2Brush;
        }
        private void UpdateScoreDisplay()
        {
            TxtScoreP1.Text = $"P1: {scores[0]}";
            TxtScoreP2.Text = $"P2: {scores[1]}";
        }
        private void CheckForGameEnd()
        {
            // Sind alle Karten gefunden?
            if (cards.All(c => c.IsMatched))
            {
                gameTimer.Stop(); // Zeit anhalten

                string message;

                if (scores[0] > scores[1])
                {
                    message = $"Spieler 1 hat gewonnen!\n\nP1: {scores[0]} Paare\nP2: {scores[1]} Paare";
                }
                else if (scores[1] > scores[0])
                {
                    message = $"Spieler 2 hat gewonnen!\n\nP1: {scores[0]} Paare\nP2: {scores[1]} Paare";
                }
                else
                {
                    message = $"Unentschieden!\n\nP1: {scores[0]} Paare\nP2: {scores[1]} Paare";
                }

                MessageBox.Show(message, "Spiel beendet",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                // Danach bleibt das Board so – Spieler kann oben auf "Neues Spiel" klicken
            }
        }


    }
}

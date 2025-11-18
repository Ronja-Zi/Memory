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

        private MemoryCard firstCard = null;
        private MemoryCard secondCard = null;
        private bool isBusy = false;

        public MainWindow()
        {
            InitializeComponent();
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

        // ---------------- SPIEL INITIALISIEREN ----------------
        private void InitializeGame()
        {
            // Vorderseiten: alle Memory-Bilder (nicht Cover)
            var frontImages = System.IO.Directory.GetFiles("Images", "*.png")
                                 .Where(f => !f.ToLower().Contains("cover"))
                                 .ToList();

            // Rückseiten: Cover-Bilder
            var coverImages = System.IO.Directory.GetFiles("Images", "*.png")
                                 .Where(f => f.ToLower().Contains("cover"))
                                 .ToList();

            // Zufällig mischen
            frontImages = frontImages.OrderBy(x => rnd.Next()).ToList();
            coverImages = coverImages.OrderBy(x => rnd.Next()).ToList(); // optional mischen

            // Alte Karten löschen
            cards.Clear();
            CardGrid.Children.Clear();

            for (int i = 0; i < frontImages.Count; i++)
            {
                var card = new MemoryCard
                {
                    ImagePath = frontImages[i],
                    CoverPath = coverImages[i],
                    Button = new Button { Tag = i, Background = Brushes.LightGray }
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
            scores[0] = scores[1] = 0;
            TxtPlayer.Text = $"Am Zug: Spieler {currentPlayer}";

            firstCard = null;
            secondCard = null;
            isBusy = false;
        }

        // ---------------- TIMER ----------------
        private void StartTimer()
        {
            gameTimer.Interval = TimeSpan.FromSeconds(1);
            gameTimer.Tick += (s, e) =>
            {
                elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
                TxtTime.Text = elapsedTime.ToString(@"mm\:ss");
            };
            gameTimer.Start();
        }

        // ---------------- KLICK EVENT ----------------
        private async void Card_Click(object sender, RoutedEventArgs e)
        {
            if (isBusy) return;

            var btn = sender as Button;
            int index = (int)btn.Tag;
            var card = cards[index];

            if (card.IsFlipped || card.IsMatched) return;

            ShowCard(card);

            if (firstCard == null)
            {
                firstCard = card;
                return;
            }

            secondCard = card;
            moves++;
            TxtMoves.Text = moves.ToString();
            isBusy = true;

            if (firstCard.ImagePath == secondCard.ImagePath)
            {
                // Treffer
                firstCard.IsMatched = true;
                secondCard.IsMatched = true;

                scores[currentPlayer - 1]++;

                // Spieler darf nochmal
                TxtPlayer.Text = $"Am Zug: Spieler {currentPlayer}";
                ResetSelection();
            }
            else
            {
                // Kein Treffer → 0,8s warten, dann umdrehen
                await Task.Delay(800);
                HideCard(firstCard);
                HideCard(secondCard);

                currentPlayer = 3 - currentPlayer; // Spielerwechsel
                TxtPlayer.Text = $"Am Zug: Spieler {currentPlayer}";
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
            var img = new Image();
            img.Source = new BitmapImage(new Uri(card.ImagePath, UriKind.Relative));
            img.Stretch = Stretch.UniformToFill;
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
            var img = new Image();
            img.Source = new BitmapImage(new Uri(card.CoverPath, UriKind.Relative));
            img.Stretch = Stretch.UniformToFill;
            card.Button.Content = img;
        }
    }
}

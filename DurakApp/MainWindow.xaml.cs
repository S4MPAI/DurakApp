using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Durak;

namespace DurakApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DurakGame durakGame;
        private Window endingScreen;

        public MainWindow()
        {
            durakGame = new DurakGame();
            InitializeComponent();
            UpdateForm();
        }

        private void UpdateForm()
        {
            PlayingTable.Children.Clear();
            PlayerCards.Children.Clear();

            UpdatePlayerCardsImage();
            SetTrumpSuitImage();

            var text = $"{durakGame.GetCardsInDeckCount()} карт";
            DeckCount.Content = text;
            BotCards.Content = $"{durakGame.bot.cards.Count} карт у соперника";
        }

        private void SetTrumpSuitImage()
        {
            var trump = durakGame.Trump;
            var source = new BitmapImage(new Uri($"Images/suit-{trump}.png", UriKind.Relative));

            Image image = new Image()
            {
                Height = 50,
                Width = 50,
                Source = source,
                VerticalAlignment = VerticalAlignment.Center, 
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Trump.Content = image;
        }

        private void UpdatePlayerCardsImage()
        {
            PlayerCards.Children.Clear();

            var playerCards = durakGame.human.cards;

            for (int i = 0; i < playerCards.Count; i++)
            {
                var card = playerCards[i];
                var source = new BitmapImage(new Uri($"Images/{card.Suit}_{card.Rate}.png", UriKind.Relative));

                Image image = new Image() { 
                                            Height = 100,
                                            Width = 73,
                                            Source = source
                                          };

                image.MouseEnter += Image_MouseEnter;
                image.MouseLeave += Image_MouseLeave;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;

                PlayerCards.Children.Add(image);
                Grid.SetColumn(image, i);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) => Close();

        private void TakeCardsAndBitoButton_Click(object sender, RoutedEventArgs e)
        {
            durakGame.MakeCardReset();

            if (durakGame.human.cards.Count == 0)
            {
                endingScreen = new GameWinWindow();
                endingScreen.Show();
            }
            else if (durakGame.bot.cards.Count == 0)
            {
                endingScreen = new GameLoseWindow();
                endingScreen.Show();
            }

            UpdatePlayerCardsImage();

            PlayingTable.Children.Clear();

            DeckCount.Content = $"{durakGame.GetCardsInDeckCount()} карт";

            ResetBotPlayingTable();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (durakGame.GetAllCardsOnTable().Count == 0) return;
            durakGame.StartGame();
            UpdateForm();
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;

            var cardNumberInHand = PlayerCards.Children.IndexOf(image);

            var card = durakGame.human.cards[cardNumberInHand];

            if (!durakGame.human.IsCardCanPlays(card, durakGame.bot, durakGame.playingTable)) return;

            IncreaseImage(image);
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {

            var image = (Image)sender;

            var cardNumberInHand = PlayerCards.Children.IndexOf(image);

            var card = durakGame.human.cards[cardNumberInHand];

            if (!durakGame.human.IsCardCanPlays(card, durakGame.bot, durakGame.playingTable)) return;

            DecreaseImage(image);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;

            var cardNumberInHand = PlayerCards.Children.IndexOf(image);

            var card = durakGame.human.cards[cardNumberInHand];

            if (!durakGame.human.IsCardCanPlays(card, durakGame.bot, durakGame.playingTable)) return;

            DecreaseImage(image);

            PlayerCards.Children.Remove(image);
            PlayingTable.Children.Add(image);

            Grid.SetRow(image, 1);
            Grid.SetColumn(image, durakGame.GetPlayerCardsOnTable(durakGame.human).Count);

            image.MouseEnter -= Image_MouseEnter;
            image.MouseLeave -= Image_MouseLeave;
            image.MouseLeftButtonUp -= Image_MouseLeftButtonUp;

            durakGame.DoOneMoveOfPlayers(cardNumberInHand);

            ResetBotPlayingTable();
            UpdatePlayerCardsImage();
        }

        private void ResetBotPlayingTable()
        {
            BotCards.Content = $"{durakGame.bot.cards.Count} карт у соперника";
            var botTableCards = durakGame.GetPlayerCardsOnTable(durakGame.bot);

            for (int i = 0; i < botTableCards.Count; i++)
            {
                var card = botTableCards[i];
                var source = new BitmapImage(new Uri($"Images/{card.Suit}_{card.Rate}.png", UriKind.Relative));

                Image image = new Image()
                {
                    Height = 100,
                    Width = 73,
                    Source = source
                };

                PlayingTable.Children.Add(image);
                Grid.SetColumn(image, i);
                Grid.SetRow(image, 0);

            }
        }

        private static void IncreaseImage(Image image)
        {
            image.Width = (int)(image.Width * 1.2);
            image.Height = (int)(image.Height * 1.2);
        }

        private static void DecreaseImage(Image image)
        {
            image.Width = (int)(image.Width / 1.2);
            image.Height = (int)(image.Height / 1.2);
        }
    }
}

using DurakApp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Durak
{
    public class DurakGame
    {
        public Suit Trump { get => playingTable.Trump; }

        private Queue<Card> deckOfCards;

        public PlayingTable playingTable;

        public Player human;

        public Player bot;

        public DurakGame()
        {
            human = new Player( PlayerStatus.Attack, PlayerType.Human);
            bot = new Player(PlayerStatus.Defense, PlayerType.Bot);

            playingTable = new PlayingTable(human, bot);

            StartGame();
        }

        public void StartGame()
        {
            human.cards.Clear();
            bot.cards.Clear();

            deckOfCards = CreateDeckOfCards();

            deckOfCards = RandomCards();

            TryDialCardsToSix();

            playingTable.Trump = GetTrumpSuit();
        }

        

        public void DoOneMoveOfPlayers(int numberCardInHand)
        {
            human.ThrowCard(numberCardInHand, bot, playingTable);

            var card = GenerateCard(bot);
            if (card != null)
                bot.ThrowCard(card, human, playingTable);
        }

        

        public List<Card> GetPlayerCardsOnTable(Player player) => playingTable.GetPlayerCards(player);

        public List<Card> GetAllCardsOnTable() => playingTable.GetAllCards();

        public int GetCardsInDeckCount() => deckOfCards.Count;

        public int GetCardDifferenceOnTable() => playingTable.CardDifference;

        public void MakeCardReset()
        {
            if (GetCardDifferenceOnTable() != 0)
            {
                var defensePlayerCards = (human.Status == PlayerStatus.Attack) ? bot.cards : human.cards;
                var allCardsOnTable = playingTable.ClearAndGetAllCards();
                
                defensePlayerCards.AddRange(allCardsOnTable);
                TryDialCardsToSix();
            }
            else
            {
                TryDialCardsToSix();
                playingTable.Clear();
                bot.Status = ChangeStatus(bot);
                human.Status = ChangeStatus(human);
            }

            if (bot.Status == PlayerStatus.Attack)
            {
                var card = GenerateCard(bot);
                bot.ThrowCard(card, bot, playingTable);
            }
        }

        private Card GenerateCard(Player player)
        {
            Card card;

            if (player.Status == PlayerStatus.Attack)
                card = GenerateAttackCard(player);
            else
                card = GenerateDefenseCard(player);

            return card;
        }

        private Card GenerateDefenseCard(Player player)
        {
            var enemyCard = GetPlayerCardsOnTable(GetEnemy(player))[^1];

            if (IsPlayerHaveDefense(player, enemyCard) &&
                (GetCardDifferenceOnTable() <= 1))
            {
                var card = player.cards.Where(x => x.Rate > enemyCard.Rate && x.Suit == enemyCard.Suit).OrderBy(x => x.Rate).FirstOrDefault();

                if (card == null)
                    card = player.cards.Where(x => x.Suit == Trump && enemyCard.Suit != Trump).OrderBy(x => x.Rate).FirstOrDefault();

                return card;
            }

            return null;
        }

        public Player GetEnemy(Player player)
        {
            return (player == human) ? bot : human;
        }

        private Card GenerateAttackCard(Player player)
        {
            var isPlayingTableHaveCards = GetAllCardsOnTable().Count == 0;
            var playingTableCards = playingTable.GetAllCards();

            var card = player.cards.Where(x => isPlayingTableHaveCards || playingTableCards.Any(y => x.Rate == y.Rate)).OrderBy(x => x.Rate).FirstOrDefault();

            return card;
        }

        private PlayerStatus ChangeStatus(Player player)
        {
            return (player.Status == PlayerStatus.Attack) ? PlayerStatus.Defense : PlayerStatus.Attack;
        }

        private bool IsPlayerHaveAttack(Player player)
        {
            var playingTableCards = playingTable.GetAllCards();

            return player.cards.Any(x => playingTableCards.Any(y => x.Rate == y.Rate));
        }

        private bool IsPlayerHaveDefense(Player player, Card enemyCard)
        {
            return player.cards.Any(x => (x.Rate > enemyCard.Rate && x.Suit == enemyCard.Suit) ||
                                        (x.Suit == Trump && enemyCard.Suit != Trump));
        }        

        private Suit GetTrumpSuit()
        {
            var card = deckOfCards.Dequeue();

            var trump = card.Suit;
            deckOfCards.Enqueue(card);

            return trump;
        }

        private void TryDialCardsToSix()
        {
            if (bot.Status == PlayerStatus.Attack)
            {
                DialCards(bot.cards);
                DialCards(human.cards);
            }
            else
            {
                DialCards(human.cards);
                DialCards(bot.cards);
            }
        }

        private void DialCards(List<Card> cards)
        {
            for (int i = cards.Count; i < 6; i++)
            {
                if (deckOfCards.Count == 0)
                    break;

                var card = deckOfCards.Dequeue();
                cards.Add(card);
            }
        }

        private Queue<Card> RandomCards()
        {
            Random random = new Random();

            return new Queue<Card>(deckOfCards.OrderBy(x => random.Next()));
        }

        private Queue<Card> CreateDeckOfCards()
        {
            var deckOfCards = new Queue<Card>(){ };

            var cardRates = Enum.GetValues(typeof(CardRate)).Cast<CardRate>().ToList();
            var cardSuits = Enum.GetValues(typeof(Suit)).Cast<Suit>().ToList();

            foreach (var rate in cardRates)
            {
                foreach (var suit in cardSuits)
                {
                    var card = new Card(rate, suit);
                    deckOfCards.Enqueue(card);
                }
            }

            return deckOfCards;
        }
    }
}

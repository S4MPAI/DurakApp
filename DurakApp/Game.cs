using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Durak
{
    public class Card
    {
        public readonly int Rate;

        public readonly Suit Suit;

        public Card(CardRate rate, Suit suit)
        {
            Rate = (int)rate;
            Suit = suit;
        }
    }

    public class PlayingTable
    {
        public int CardDifference { get => Math.Abs(botCardsOnTable.Count - humanCardsOnTable.Count); } 

        private List<Card> botCardsOnTable = new List<Card>();
        private List<Card> humanCardsOnTable = new List<Card>();

        public List<Card> GetPlayerCards(Player player)
        {
            if (player == Player.Bot)
                return botCardsOnTable;

            return new(humanCardsOnTable);
        }

        public void AddCard(Player player,Card card)
        {
            if (player == Player.Bot)
                botCardsOnTable.Add(card);
            else
                humanCardsOnTable.Add(card);
        }

        public void Clear()
        {
            botCardsOnTable.Clear();
            humanCardsOnTable.Clear();
        }

        public List<Card> ClearAndGetAllCards()
        {
            var cards = GetAllCards();

            Clear();

            return cards;
        }

        public List<Card> GetAllCards() => botCardsOnTable.Concat(humanCardsOnTable).ToList();
    }

    public class DurakGame
    {
        private Queue<Card> deckOfCards;

        private PlayingTable playingTable = new PlayingTable();

        private List<Card> humanCards = new List<Card>();
        public List<Card> HumanCards { get => new(humanCards); }

        private List<Card> botCards = new List<Card>();
        public List<Card> BotCards { get => new(botCards); }

        public Suit Trump { get; private set; }

        private Stage stage = Stage.None;

        public Player AttackPlayer { get; private set; } = Player.Human;

        public void StartGame()
        {
            if (stage == Stage.Playing)
                throw new Exception();

            deckOfCards = CreateDeckOfCards();

            stage = Stage.Playing;

            deckOfCards = RandomCards();

            TryDialCardsToSix();

            Trump = GetTrumpSuit();
        }

        public void DoOneMoveOfPlayers(int numberCardInHand)
        {
            MoveHuman(numberCardInHand);
            MoveBot();
        }

        public bool IsCardCanAttack(Card card)
        {
            var cardsOnTable = GetAllCardsOnTable();


            return humanCards.Count != 0 &&
                   botCards.Count != 0 &&
                   GetPlayerCardsOnTable(Player.Human).Count < 6 &&
                   GetPlayerCardsOnTable(Player.Bot).Count < 6 &&
                   (cardsOnTable.Count == 0 || cardsOnTable.Any(x => x.Rate == card.Rate));
        }

        public bool IsCardCanDefense(Card card)
        {
            if (GetCardDifferenceOnTable() == 0) return false;

            var enemyCardsOnTable = GetPlayerCardsOnTable(AttackPlayer);

            if (enemyCardsOnTable.Count == 0) return true;
            var lastEnemyCard = enemyCardsOnTable[^1];

            return ((card.Rate > lastEnemyCard.Rate && card.Suit == lastEnemyCard.Suit) ||
                    (card.Suit == Trump && lastEnemyCard.Suit != Trump));
        }

        private void MoveBot()
        {
            if (AttackPlayer == Player.Bot)
                AttackBot();
            else
                DefenseBot();

        }

        private void MoveHuman(int numberCardInHand)
        {
            var card = humanCards[numberCardInHand];

            var isCardCanPlay = (AttackPlayer == Player.Human) ? IsCardCanAttack(card) : IsCardCanDefense(card);
            if (isCardCanPlay)
            {
                humanCards.RemoveAt(numberCardInHand);
                playingTable.AddCard(Player.Human, card);
            }
            else
                throw new Exception("Нельзя разыграть карту");
        }

        private void DefenseBot()
        {
            var humanCard = GetPlayerCardsOnTable(Player.Human)[^1];

            if (IsPlayerHaveDefense(Player.Bot, humanCard) &&
                (GetCardDifferenceOnTable() <= 1))
            {
                var card = botCards.Where(x => x.Rate > humanCard.Rate && x.Suit == humanCard.Suit).OrderBy(x => x.Rate).FirstOrDefault();

                if (card == null)
                    card = botCards.Where(x => x.Suit == Trump && humanCard.Suit != Trump).OrderBy(x => x.Rate).FirstOrDefault();

                playingTable.AddCard(Player.Bot, card);
                botCards.Remove(card);
            }
        }

        private void AttackBot()
        {
            var isPlayingTableHaveCards = GetAllCardsOnTable().Count == 0;
            var playingTableCards = playingTable.GetAllCards();

            var card = botCards.Where(x => isPlayingTableHaveCards || playingTableCards.Any(y => x.Rate == y.Rate)).OrderBy(x => x.Rate).FirstOrDefault();

            if (card == null) return;

            playingTable.AddCard(Player.Bot, card);
            botCards.Remove(card);
        }

        public List<Card> GetPlayerCardsOnTable(Player player) => playingTable.GetPlayerCards(player);

        public List<Card> GetAllCardsOnTable() => playingTable.GetAllCards();

        public int GetCardsInDeckCount() => deckOfCards.Count;

        public int GetCardDifferenceOnTable() => playingTable.CardDifference;

        public bool IsPlayerHaveAttack(Player player)
        {
            var playerCards = (player == Player.Human) ? humanCards : botCards;

            var playingTableCards = playingTable.GetAllCards();

            return playerCards.Any(x => playingTableCards.Any(y => x.Rate == y.Rate));
        }

        public void MakeCardReset()
        {
            if (GetCardDifferenceOnTable() != 0)
            {
                var defensePlayerCards = (AttackPlayer == Player.Bot) ? humanCards : botCards;
                var allCardsOnTable = playingTable.ClearAndGetAllCards();
                
                defensePlayerCards.AddRange(allCardsOnTable);
                TryDialCardsToSix();
            }
            else
            {
                TryDialCardsToSix();
                playingTable.Clear();
                AttackPlayer = (AttackPlayer == Player.Bot) ? Player.Human : Player.Bot;
            }

            if (AttackPlayer == Player.Bot)
                MoveBot();
        }

        private bool IsPlayerHaveDefense(Player player, Card enemyCard)
        {
            var playerCards = (player == Player.Human) ? humanCards : botCards;

            return playerCards.Any(x => (x.Rate > enemyCard.Rate && x.Suit == enemyCard.Suit) ||
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
            if (AttackPlayer == Player.Bot)
            {
                DialCards(botCards);
                DialCards(humanCards);
            }
            else
            {
                DialCards(humanCards);
                DialCards(botCards);
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

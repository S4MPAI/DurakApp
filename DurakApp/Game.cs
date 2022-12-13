﻿using System;
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

        public bool IsCardCanAttack(DurakGame durakGame)
        {
            var cardsOnTable = durakGame.GetAllCardsOnTable();

            return cardsOnTable.Count == 0 || cardsOnTable.Any(x => x.Rate == Rate);
        }

        public bool IsCardCanDefense(DurakGame durakGame)
        {
            if (durakGame.GetCardDifferenceOnTable() == 0) return false;

            var enemyCardsOnTable = durakGame.GetPlayerCardsOnTable(durakGame.AttackPlayer);

            if (enemyCardsOnTable.Count == 0) return true;
            var lastEnemyCard = enemyCardsOnTable[^1];

            return ((Rate > lastEnemyCard.Rate && Suit == lastEnemyCard.Suit) ||
                    (Suit == durakGame.Trump && lastEnemyCard.Suit != durakGame.Trump));
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
                return new(botCardsOnTable);

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

        public Player AttackPlayer { get; private set; }

        public DurakGame()
        {
            deckOfCards = CreateDeckOfCards();
            AttackPlayer = Player.Human;
        }

        public void StartGame()
        {
            if (stage == Stage.Playing)
                throw new Exception();

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

        public void MoveBot()
        {
            if (AttackPlayer == Player.Bot)
                AttackBot();
            else
            {
                DefenseBot();
            }

        }

        private void DefenseBot()
        {
            var humanCard = GetPlayerCardsOnTable(Player.Human)[^1];

            if (IsPlayerHaveDefense(Player.Bot, humanCard) &&
                (GetPlayerCardsOnTable(Player.Human).Count - GetPlayerCardsOnTable(Player.Bot).Count) <= 1)
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


        public void MoveHuman(int numberCardInHand)
        {
            var card = humanCards[numberCardInHand];

            var isCardCanPlay = (AttackPlayer == Player.Human) ? card.IsCardCanAttack(this) : card.IsCardCanDefense(this);
            if (isCardCanPlay)
            {
                humanCards.RemoveAt(numberCardInHand);
                playingTable.AddCard(Player.Human, card);
            }
            else
                throw new Exception("Нельзя разыграть карту");
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
            if (GetPlayerCardsOnTable(Player.Human).Count != GetPlayerCardsOnTable(Player.Bot).Count)
            {
                var defensePlayerCards = (AttackPlayer == Player.Bot) ? humanCards : botCards;

                var allCardsOnTable = playingTable.ClearAndGetAllCards();
                defensePlayerCards.AddRange(allCardsOnTable);
            }
            else
            {
                playingTable.Clear();
                AttackPlayer = (AttackPlayer == Player.Bot) ? Player.Human : Player.Bot;
            }

            TryDialCardsToSix();

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
            var hands = new List<List<Card>>();

            if (AttackPlayer == Player.Bot)
            {
                hands.Add(botCards);
                hands.Add(humanCards);
            }
            else
            {
                hands.Add(humanCards);
                hands.Add(botCards);
            }

            foreach (var cards in hands)
            {
                for (int i = cards.Count; i < 6; i++)
                {
                    if (deckOfCards.Count == 0)
                        break;

                    var card = deckOfCards.Dequeue();
                    cards.Add(card);
                }
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

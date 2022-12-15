using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durak
{
    public class PlayingTable
    {
        public int CardDifference { get => Math.Abs(botCardsOnTable.Count - humanCardsOnTable.Count); }

        private List<Card> botCardsOnTable = new List<Card>();
        private List<Card> humanCardsOnTable = new List<Card>();

        public Suit Trump { get; internal set; }

        public List<Card> GetPlayerCards(PlayerType player)
        {
            if (player == PlayerType.Bot)
                return new(botCardsOnTable);

            return new(humanCardsOnTable);
        }

        public int GetPlayerCardsCount(PlayerType player) => (player == PlayerType.Bot) ? botCardsOnTable.Count : humanCardsOnTable.Count;

        public void AddCard(PlayerType player, Card card)
        {
            if (player == PlayerType.Bot)
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durak
{
    public class PlayingTable
    {
        public int CardDifference { get => Math.Abs(playersCards[firstPlayer].Count - playersCards[secondPlayer].Count); }

        private Dictionary<Player, List<Card>> playersCards;

        private Player firstPlayer;

        private Player secondPlayer;

        public Suit Trump { get; internal set; }

        public PlayingTable(Player player, Player player1)
        {
            playersCards = new Dictionary<Player, List<Card>>();

            playersCards[player] = new List<Card>();
            playersCards[player1] = new List<Card>();

            firstPlayer = player;
            secondPlayer = player1;
        }

        public List<Card> GetPlayerCards(Player player)
        {
            return new List<Card>(playersCards[player]);
        }

        public int GetPlayerCardsCount(Player player) => playersCards[player].Count;

        public void AddCard(Player player, Card card)
        {
            playersCards[player].Add(card);
        }

        public void Clear()
        {
            playersCards[firstPlayer].Clear();
            playersCards[secondPlayer].Clear();
        }

        public List<Card> ClearAndGetAllCards()
        {
            var cards = GetAllCards();

            Clear();

            return cards;
        }

        public List<Card> GetAllCards() => playersCards[firstPlayer].Concat(playersCards[secondPlayer]).ToList();
    }
}

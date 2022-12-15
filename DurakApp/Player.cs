using Durak;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Durak
{
    public class Player
    {
        public readonly List<Card> cards;

        public readonly PlayerType Type;

        public PlayerStatus Status { get; internal set; }

        public Player(PlayerStatus status, PlayerType type)
        {
            cards = new List<Card>();
            Status = status;
            Type = type;
        }

        public void ThrowCard(int numberCardInHand, Player opponent, PlayingTable playingTable)
        {
            if (numberCardInHand < 0 || numberCardInHand> cards.Count) throw new ArgumentException();

            var card = cards[numberCardInHand];
            
            var isCardCanPlay = IsCardCanPlays(card, opponent, playingTable);
            if (isCardCanPlay)
            {
                cards.RemoveAt(numberCardInHand);
                playingTable.AddCard(Type, card);
            }
        }
        
        public void ThrowCard(Card card, Player opponent, PlayingTable playingTable)
        {
            var numberCardInHand = cards.FindIndex(x => x == card);
            
            if (numberCardInHand == -1) throw new ArgumentException();

            ThrowCard(numberCardInHand, opponent, playingTable);
        }

        public bool IsCardCanPlays(Card card, Player opponent, PlayingTable playingTable)
        {
            return (Status == PlayerStatus.Attack) ? IsCardCanAttack(card, opponent, playingTable) : 
                                                     IsCardCanDefense(card, opponent, playingTable);
        }

        private bool IsCardCanDefense(Card card, Player opponent, PlayingTable playingTable)
        {
            if (playingTable.CardDifference == 0) return false;

            var enemyCardsOnTable = playingTable.GetPlayerCards(opponent.Type);

            if (enemyCardsOnTable.Count == 0) return true;

            var lastEnemyCard = enemyCardsOnTable[^1];
            var trump = playingTable.Trump;

            return ((card.Rate > lastEnemyCard.Rate && card.Suit == lastEnemyCard.Suit) ||
                    (card.Suit == trump && lastEnemyCard.Suit != trump));
        }

        private bool IsCardCanAttack(Card card, Player opponent, PlayingTable playingTable)
        {
            var cardsOnTable = playingTable.GetAllCards();

            return cards.Count != 0 &&
                   opponent.cards.Count != 0 &&
                   playingTable.GetPlayerCards(Type).Count < 6 &&
                   playingTable.GetPlayerCards(opponent.Type).Count < 6 &&
                   (cardsOnTable.Count == 0 || cardsOnTable.Any(x => x.Rate == card.Rate));
        }
    }
}

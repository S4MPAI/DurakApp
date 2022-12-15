using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durak
{
    public enum PlayerType
    {
        Human,
        Bot
    }

    public enum PlayerStatus
    {
        Attack,
        Defense,
    }

    public enum Suit
    {
        Hearts,
        Spades,
        Diamonds,
        Clubs,
    }

    public enum CardRate
    {
        Six = 6,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace,
    }
}

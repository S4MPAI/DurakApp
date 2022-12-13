using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Durak
{
    public enum Stage
    {
        None,
        Playing,
        BotWin,
        PlayerWin
    }

    public enum Player
    {
        Human,
        Bot
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBot
{
    public enum BotState
    {
        MainMenu,
        Ignore,
        FavoriteMenu,
        AddingCryptocurrency,
        RemovingCryptocurrency,
        PriceMenu,
        GenAnalysis,
        // ... other states
    }
}

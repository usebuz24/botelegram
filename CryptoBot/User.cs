using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBot
{
    public class User
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public BotState State { get; set; }
        public List<string> Favorites { get; set; }
    }
}

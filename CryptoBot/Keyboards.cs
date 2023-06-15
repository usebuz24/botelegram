using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoBot
{
    public static class Keyboards
    {
        public static ReplyKeyboardMarkup MainMenu = new(new[]
        {
            new KeyboardButton[] { "Analysis", "Favorites", "Price"},
        })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup FavouritesMenu = new(new[]
       {
            new KeyboardButton[] { "Add", "Remove", "Back"},
        })
        {
            ResizeKeyboard = true
        };
        public static ReplyKeyboardMarkup PriceMenu = new(new[]
       {
            new KeyboardButton[] {"Back"},
        })
        {
            ResizeKeyboard = true
        };

        public static List<ReplyKeyboardMarkup> CreatePagedKeyboards(List<string> items, int itemsPerPage)
        {
            var keyboards = new List<ReplyKeyboardMarkup>();

            for (int i = 0; i < items.Count; i += itemsPerPage)
            {
                var pageItems = items.Skip(i).Take(itemsPerPage).ToList();

                var rows = new List<KeyboardButton[]>();

                // Add the items for this page
                rows.Add(pageItems.Select(item => new KeyboardButton(item)).ToArray());

                // Add the navigation buttons
                var navigationButtons = new List<KeyboardButton>();
                if (i > 0)
                {
                    navigationButtons.Add(new KeyboardButton("Previous"));
                }
                navigationButtons.Add(new KeyboardButton("Back"));
                if (i + itemsPerPage < items.Count)
                {
                    navigationButtons.Add(new KeyboardButton("Next"));
                }
                rows.Add(navigationButtons.ToArray());

                var keyboard = new ReplyKeyboardMarkup(rows.ToArray()) { ResizeKeyboard = true };

                keyboards.Add(keyboard);
            }

            return keyboards;
        }

    }
    public class KeyboardState
    {
        public int CurrentPage { get; set; }
        public List<ReplyKeyboardMarkup> Keyboards { get; set; }
    }
}

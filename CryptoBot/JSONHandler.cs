using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CryptoBot
{
    public class JSONHandler
    {
        private string filePath;
        public JSONHandler(string filePath)
        {
            this.filePath = filePath;
        }
        public List<User> LoadUsers()
        {
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<User>>(jsonData) ?? new List<User>();
            }
            else
            {
                return new List<User>();
            }
        }
        public void SaveUsers(List<User> users)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonData = JsonSerializer.Serialize(users, options);
            File.WriteAllText(filePath, jsonData);
        }
        public void AddNewUser(long userId, string username)
        {
            // Підкачуємо існуючі данні користувачів
            List<User> users = LoadUsers();

            // Додаємо нового користувача
            User newUser = new User
            {
                Id = userId,
                Username = username,
                State = BotState.MainMenu,
                Favorites = new List<string> { }
            };
            users.Add(newUser);

            // Зберегаємо данні
            SaveUsers(users);
            Console.WriteLine($"New user added {newUser.Username} (ID: {newUser.Id})");
        }
        public void AddToFavourites(long userId, string cryptoTicker)
        {
            // Підкачуємо існуючі данні користувачів
            List<User> users = LoadUsers();

            // Знаходимо конкретного користувчача по його ID
            User user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                //Перевіряємо чи має вже користувач цю криптовалюту в улюблених
                if (user.Favorites.Contains(cryptoTicker))
                {
                    Console.WriteLine($"User {user.Username} (ID: {user.Id}) already has {cryptoTicker} in their favorites");
                }
                else
                {
                    // Додаємо тікер до улюбленного
                    user.Favorites.Add(cryptoTicker);
                    // Зберегаємо оновлену інформацію
                    SaveUsers(users);
                    Console.WriteLine($"Added {cryptoTicker} to favorites for user {user.Username} (ID: {user.Id})");
                }
            }
            else
            {
                Console.WriteLine($"User with ID {userId} not found");
            }
        }
        public void RemoveFromFavourites(long userId, string cryptoTicker)
        {
            // Підкачуємо існуючі данні користувачів
            List<User> users = LoadUsers();

            // Знаходимо конкретного користувчача по його ID
            User user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                // Видаляємо тікер з улюбленного
                user.Favorites.Remove(cryptoTicker);
                // Зберегаємо оновлену інформацію
                SaveUsers(users);
                Console.WriteLine($"Removed {cryptoTicker} from favorites for user {user.Username} (ID: {user.Id})");
            }
            else
            {
                Console.WriteLine($"User with ID {userId} not found");
            }
        }
        public BotState GetState(long userId)
        {
            List<User> users = LoadUsers();
            User user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                return user.State;
            }
            else
            {
                Console.WriteLine($"User with ID {userId} not found");
                return BotState.MainMenu; // Return a default state if user is not found
            }
        }
        public void SetState(long userId, BotState state)
        {
            List<User> users = LoadUsers();
            User user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                user.State = state;
                SaveUsers(users);
            }
            else
            {
                Console.WriteLine($"User with ID {userId} not found");
            }
        }
        public string[] GetUserFavorites(long userId)
        {
            // Load existing user data
            List<User> users = LoadUsers();

            // Find the user with the specified userId
            User user = users.Find(u => u.Id == userId);
            if (user != null)
            {
                // Return the favorites as an array
                return user.Favorites.ToArray();
            }
            else
            {
                Console.WriteLine($"User with ID {userId} not found");
                return new string[0]; // Return an empty array if the user is not found
            }
        }
        public bool UserExists(long userId)
        {
            List<User> users = LoadUsers();
            return users.Exists(u => u.Id == userId);
        }
        public bool TickerExists(long userId, string cryptoTicker)
        {
            List<User> users = LoadUsers();
            User user = users.Find(u => u.Id == userId);
            return user.Favorites.Contains(cryptoTicker);
        }

    }
}

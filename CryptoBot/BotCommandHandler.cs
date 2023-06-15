using CryptoBot;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Linq.Expressions;

public class BotCommandHandler
{
    private readonly TelegramBotClient _botClient;
    Configuration config = new Configuration();
    AnalysisHandler analysisHandler = new AnalysisHandler();
    JSONHandler jsonHandler = new JSONHandler("users.json");
    Dictionary<long, KeyboardState> keyboardStates = new Dictionary<long, KeyboardState>();
    public BotCommandHandler()
    {
        this.config = config;
        _botClient = new TelegramBotClient(config.GetTelegramBotToken());
        _botClient.StartReceiving(Update, Error);
    }
    public async Task Update(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var state = jsonHandler.GetState(message.Chat.Id);
        await Console.Out.WriteLineAsync($"{message.Chat.Id} sent the message: {message.Text}");
        if (message.Text != null)
        {
            if (message.Text.StartsWith("/start"))
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Welcome to the Crypto Info Bot! You can check prices, add favorites, and get analysis of cryptocurrencies.",
                    replyMarkup: Keyboards.MainMenu
                    );
                if (!jsonHandler.UserExists(message.Chat.Id)) jsonHandler.AddNewUser(message.Chat.Id, message.Chat.Username);
                jsonHandler.SetState(message.Chat.Id, BotState.MainMenu);
                return;
            }
            if (state == BotState.MainMenu)
            {
                switch (message.Text)
                {
                    case "Analysis":
                        await botClient.SendTextMessageAsync(
                               chatId: message.Chat.Id,
                               text: "Type ticker and the bot will give you an analysis on your project, it can take some time so, please, be patient.",
                               replyMarkup: Keyboards.PriceMenu);
                        jsonHandler.SetState(message.Chat.Id, BotState.GenAnalysis);
                        break;
                    case "Favorites":
                        var userFavourites = jsonHandler.GetUserFavorites(message.Chat.Id);
                        if (userFavourites.Length != 0)
                        {
                            var apiKey = config.GetCmcApiKey();
                            string favoritesString = "";
                            foreach (var favorite in userFavourites)
                            {
                                favoritesString += $"{favorite} - {await PriceHandler.GetCryptocurrencyPrice(favorite, apiKey)}\n";
                            }
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: favoritesString,
                                replyMarkup: Keyboards.FavouritesMenu);
                            jsonHandler.SetState(message.Chat.Id, BotState.FavoriteMenu);
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                               chatId: message.Chat.Id,
                               text: "Choose what you want to do.",
                               replyMarkup: Keyboards.FavouritesMenu);
                            jsonHandler.SetState(message.Chat.Id, BotState.FavoriteMenu);
                        }
                        break;
                    case "Price":
                        await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Type ticker and I will give you price",
                                replyMarkup: Keyboards.PriceMenu);
                        jsonHandler.SetState(message.Chat.Id, BotState.PriceMenu);
                        break;
                    default: break;
                }
                return;
            }
            if (state == BotState.FavoriteMenu)
            {
                switch (message.Text)
                {
                    case "Add":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Send a ticker to add it to the favorites",
                            replyMarkup: Keyboards.PriceMenu
                        );
                        jsonHandler.SetState(message.Chat.Id, BotState.AddingCryptocurrency);
                        break;
                    case "Remove":
                        var favorites = jsonHandler.GetUserFavorites(message.Chat.Id).ToList();

                        // Create paged keyboards
                        var keyboards = Keyboards.CreatePagedKeyboards(favorites, 4);

                        // Store the keyboards in the user's state
                        keyboardStates[message.Chat.Id] = new KeyboardState { CurrentPage = 0, Keyboards = keyboards };
                        Console.WriteLine($"Added user {message.Chat.Id} to keyboardStates");  // Add this line
                                                                                               // Send the first keyboard
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Choose what crypto you want to remove, or just type ticker.\nUse the 'Next', 'Previous' and 'Back' buttons to navigate.",
                            replyMarkup: keyboards[0]
                        );
                        jsonHandler.SetState(message.Chat.Id, BotState.RemovingCryptocurrency);
                        break;
                    case "Back":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "You're back!",
                            replyMarkup: Keyboards.MainMenu
                            );
                        jsonHandler.SetState(message.Chat.Id, BotState.MainMenu);
                        break;
                    default:
                        break;
                }
                return;
            }
            if (state == BotState.PriceMenu)
            {
                if (message.Text == "Back")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "You're back!",
                        replyMarkup: Keyboards.MainMenu);
                    jsonHandler.SetState(message.Chat.Id, BotState.MainMenu);
                    return;
                }
                else
                {
                    try
                    {
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: await PriceHandler.GetCryptocurrencyPrice(message.Text.ToUpper(), config.GetCmcApiKey()));
                        return;
                    }
                    catch (Exception ex)
                    {
                        await Console.Out.WriteLineAsync(ex.Message);
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Invalid Ticker. Try again.");
                        return;
                    }
                }
            }
            if (state == BotState.RemovingCryptocurrency)
            {
                switch (message.Text)
                {
                    case "Next":
                    case "Previous":
                        var keyboardState = keyboardStates[message.Chat.Id];
                        var newPage = message.Text == "Next" ? keyboardState.CurrentPage + 1 : keyboardState.CurrentPage - 1;
                        keyboardState.CurrentPage = newPage;
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "...",
                            replyMarkup: keyboardState.Keyboards[newPage]);
                        break;
                    case "Back":
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Operation canceled",
                            replyMarkup: Keyboards.FavouritesMenu);
                        jsonHandler.SetState(message.Chat.Id, BotState.FavoriteMenu);
                        break;
                    default:
                        if (jsonHandler.TickerExists(message.Chat.Id, message.Text.ToUpper()))
                        {
                            jsonHandler.RemoveFromFavourites(message.Chat.Id, message.Text.ToUpper());
                            var favorites = jsonHandler.GetUserFavorites(message.Chat.Id).ToList();

                            // Create a new paged keyboard with the updated list of favorites
                            var keyboards = Keyboards.CreatePagedKeyboards(favorites, 4);

                            // Update the user's KeyboardState with the new paged keyboard
                            keyboardStates[message.Chat.Id] = new KeyboardState { CurrentPage = 0, Keyboards = keyboards };

                            // Send a message to the user with the updated keyboard
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"{message.Text.ToUpper()} removed.",
                                replyMarkup: keyboards[0]
                            );
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: "Ticker is invalid");
                        }
                        break;
                }
            }
            if (state == BotState.AddingCryptocurrency)
            {
                if (message.Text == "Back")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "You're back!",
                        replyMarkup: Keyboards.FavouritesMenu);
                    jsonHandler.SetState(message.Chat.Id, BotState.FavoriteMenu);
                    return;
                }
                if (jsonHandler.TickerExists(message.Chat.Id, message.Text.ToUpper()))
                {
                    await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"You already have {message.Text} in the favorites"
                );
                }
                else
                {

                    if (await PriceHandler.TickerExists(config.GetCmcApiKey(), message.Text.ToUpper()))
                    {
                        jsonHandler.AddToFavourites(message.Chat.Id, message.Text.ToUpper());
                        await botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"{message.Text.ToUpper()} added.");
                    }
                    else
                    {


                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Invalid Ticker. Try again.");
                        return;
                    }

                }
            }
            if (state == BotState.GenAnalysis)
            {
                if (message.Text == "Back")
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "You're back!",
                        replyMarkup: Keyboards.MainMenu);
                    jsonHandler.SetState(message.Chat.Id, BotState.MainMenu);
                    return;
                }
                else
                {
                    try
                    {
                        var slug = await PriceHandler.GetSlug(config.GetCmcApiKey(), message.Text.ToUpper());
                        await Console.Out.WriteLineAsync(slug.ToString());
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "The analysis has begun to generate. Please, wait."
                        );
                        string analysis = await analysisHandler.GetAnalysisAsync(slug);

                        int messageLength = 4096;

                        for (int i = 0; i < analysis.Length; i += messageLength)
                        {
                            string messageToSend = analysis.Substring(i, Math.Min(messageLength, analysis.Length - i));
                            await botClient.SendTextMessageAsync(message.Chat.Id, messageToSend);
                        }
                        return;
                    }
                    catch (Exception e)
                    {
                        await Console.Out.WriteLineAsync(e.Message);
                        await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "There's no such token. Try again."
                        );
                        return;
                    }
                }
            }
        }
    }
    Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        Console.WriteLine(arg2.Message);
        return Task.CompletedTask;
    }
}

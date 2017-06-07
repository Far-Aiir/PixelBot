using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

public class SecondBot
{
    public static DiscordSocketClient _client;
    public async Task RunBot()
    {
        _client = new DiscordSocketClient();
        await _client.LoginAsync(TokenType.Bot, Program.TokenMap.Discord);
        await _client.StartAsync();
        await Task.Delay(-1);
    }
}

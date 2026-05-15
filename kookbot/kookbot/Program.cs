using Kook;
using Kook.WebSocket;

class Program
{
    private static KookSocketClient? _client;
    private static readonly ManualResetEvent _shutdownEvent = new(false);

    static async Task Main(string[] args)
    {
        // 在这里填入你的机器人 Token
        const string token = "1/NDc3NzU=/gQuifQ3eUPbfXbtH1VHekA==";

        if (token == "1/NDc3NzU=/Wwm3p5rj4R//ix3zk876Vw==")
        {
            Console.WriteLine("错误：请先在 Program.cs 中填入你的机器人 Token！");
            Environment.Exit(1);
        }

        var config = new KookSocketConfig
        {
            LogLevel = LogSeverity.Debug
        };

        _client = new KookSocketClient(config);

        _client.Log += Client_Log;
        _client.MessageReceived += Client_MessageReceived;
        _client.MessageButtonClicked += Client_MessageButtonClicked;
        _client.Ready += Client_Ready;
        _client.Disconnected += Client_Disconnected;

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            _shutdownEvent.Set();
        };

        try
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            Console.WriteLine("机器人已启动，按 Ctrl+C 退出...");
            _shutdownEvent.WaitOne();

            Console.WriteLine("正在关闭机器人...");
            await _client.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误: {ex.Message}");
        }
    }

    private static Task Client_Log(LogMessage arg)
    {
        Console.WriteLine($"[{arg.Severity}] {arg.Message}");
        return Task.CompletedTask;
    }

    private static Task Client_Ready()
    {
        Console.WriteLine($"机器人已就绪！当前用户: {_client?.CurrentUser?.Username}");
        return Task.CompletedTask;
    }

    private static Task Client_Disconnected(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"断开连接: {ex.Message}");
        }
        _shutdownEvent.Set();
        return Task.CompletedTask;
    }

    private static async Task Client_MessageReceived(SocketMessage message, SocketGuildUser? user, SocketTextChannel? channel)
    {
        // 忽略系统消息和机器人自己的消息
        if (message is not SocketUserMessage userMessage)
            return;

        if (userMessage.Author is not SocketUser author)
            return;

        // 忽略机器人消息
        if (author.IsBot == true)
            return;

        // 确保在频道中
        if (channel == null)
            return;

        // 监听 !ping 命令
        if (userMessage.Content.Equals("!ping", StringComparison.OrdinalIgnoreCase))
        {
            await channel.SendTextAsync("Pong! 🏓");
        }

        // 监听 !hello 命令
        if (userMessage.Content.Equals("!hello", StringComparison.OrdinalIgnoreCase))
        {
            await channel.SendTextAsync($"你好, {author.Username}! 👋");
        }

        // 监听 !问答 命令
        if (userMessage.Content.Equals("!问答", StringComparison.OrdinalIgnoreCase))
        {
            // 构建卡片消息（参考官方 SimpleBot 示例）
            // 按钮竖着排列：每个按钮放在单独的 ActionGroupModuleBuilder 中
            CardBuilder builder = new CardBuilder()
                .WithTheme(CardTheme.Invisible)
                .AddModule<SectionModuleBuilder>(s => s.WithText("欢迎各位老板"))
                .AddModule<ActionGroupModuleBuilder>(a => a.AddElement(b => b
                    .WithClick(ButtonClickEventType.ReturnValue)
                    .WithText("答案1")
                    .WithValue("btn_1")
                    .WithTheme(ButtonTheme.Primary)))
                .AddModule<ActionGroupModuleBuilder>(a => a.AddElement(b => b
                    .WithClick(ButtonClickEventType.ReturnValue)
                    .WithText("答案2")
                    .WithValue("btn_2")
                    .WithTheme(ButtonTheme.Primary)))
                .AddModule<ActionGroupModuleBuilder>(a => a.AddElement(b => b
                    .WithClick(ButtonClickEventType.ReturnValue)
                    .WithText("答案3")
                    .WithValue("btn_3")
                    .WithTheme(ButtonTheme.Primary)));

            // 发送卡片消息
            await channel.SendCardAsync(builder.Build());
        }
    }

    private static async Task Client_MessageButtonClicked(string value, Cacheable<SocketGuildUser, ulong> user, Cacheable<IMessage, Guid> message, SocketTextChannel channel)
    {
        Console.WriteLine($"收到按钮点击事件，值为: {value}");

        var messageEntity = await message.GetOrDownloadAsync();
        if (messageEntity is not IUserMessage userMessage)
            return;

        switch (value)
        {
            case "btn_1":
                await userMessage.ReplyTextAsync("你选择了：答案1 🎉");
                break;
            case "btn_2":
                await userMessage.ReplyTextAsync("你选择了：答案2 🎉");
                break;
            case "btn_3":
                await userMessage.ReplyTextAsync("你选择了：答案3 🎉");
                break;
            default:
                Console.WriteLine($"未知的按钮值: {value}");
                break;
        }
    }
}

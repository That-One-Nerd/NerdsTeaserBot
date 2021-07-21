using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NerdsTeaserBot.Misc;
using NerdsTeaserBot.Modules;
using NerdsTeaserBot.Modules.Commands;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static NerdsTeaserBot.Const;

namespace NerdsTeaserBot
{
    public class Internals
    {
        internal static DiscordSocketClient client;
        internal static CommandService commands;
        internal static SocketCommandContext context;
        internal static SocketUserMessage message;
        internal static ServiceProvider services;

        internal static DateTime upTime;

        public static void Main() => RunBotAsync().GetAwaiter().GetResult();

        public static async Task RunBotAsync()
        {
            if (File.Exists(Data.consts.Path)) Data.consts.Load();

            commands = new(new()
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = false,
                LogLevel = LogSeverity.Info,
                SeparatorChar = ' ',
                ThrowOnError = true,
            });
            client = new(new()
            {
                AlwaysAcknowledgeInteractions = true,
                AlwaysDownloadUsers = true,
                ConnectionTimeout = 10000,
                DefaultRetryMode = RetryMode.AlwaysFail,
                ExclusiveBulkDelete = true,
                GuildSubscriptions = true,
                LargeThreshold = 250,
                LogLevel = LogSeverity.Info,
                MaxWaitBetweenGuildAvailablesBeforeReady = 250,
                MessageCacheSize = 50,
                RateLimitPrecision = RateLimitPrecision.Millisecond,
                UseSystemClock = false,
            });
            services = new ServiceCollection().AddSingleton(client).AddSingleton(commands).BuildServiceProvider();

            LoadBackground();
            LoadHandles();

            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);

            await client.LoginAsync(TokenType.Bot, Hidden.token);
            await client.StartAsync();

            upTime = DateTime.Now;

            await Task.Delay(-1);
        }

        internal static void LoadHandles()
        {
            // client.ChannelCreated += LogModule.ChannelCreatedHandler;

            // client.ChannelDestroyed += LogModule.ChannelDeletedHandler;

            // client.ChannelUpdated += LogModule.ChannelModifiedHandler;

            client.InteractionCreated += ButtonModule.ButtonHandler;

            client.JoinedGuild += BotModule.PermittedServerHandler;

            client.Log += LogHandler;

            // client.MessagesBulkDeleted += LogModule.MessagesPurgedHandler;

            // client.MessageDeleted += LogModule.MessageDeletedHandler;

            client.MessageReceived += MessageHandler;
            client.MessageReceived += ModmailModule.ModmailHandler;

            // client.MessageUpdated += LogModule.MessageEditedHandler;

            // client.RoleCreated += LogModule.RoleCreatedHandler;

            // client.RoleDeleted += LogModule.RoleDeletedHandler;

            // client.RoleUpdated += LogModule.RoleModifiedHandler;

            // client.GuildUpdated += LogModule.GuildModifiedHandler;

            // client.UserBanned += LogModule.UserBannedHandler;

            // client.UserJoined += LogModule.UserJoinedHandler;
            client.UserJoined += MessagingModule.UserJoinedHandler;

            // client.UserLeft += LogModule.UserLeftHandler;
            client.UserLeft += MessagingModule.UserLeftHandler;

            // client.UserUnbanned += LogModule.UserUnbannedHandler;

            // client.UserUpdated += LogModule.UserModifiedHandler;

            HandlesModule.OnUserMessageRecieved += LevelModule.LevelHandler;
            HandlesModule.OnUserMessageRecieved += TagModule.TagHandler;
            HandlesModule.OnUserMessageRecieved += VariableModule.AutopublishHandler;
        }

        internal static void LoadBackground()
        {
            _ = BackgroundModule.StatusHandler();
            _ = BackgroundModule.MuteHandler();
        }

        internal static Task LogHandler(LogMessage arg)
        {
            Console.WriteLine(arg);
            if (arg.Severity != LogSeverity.Warning && arg.Severity != LogSeverity.Info) LogModule.LogMessage(arg);
            return Task.CompletedTask;
        }

        public static async Task MessageHandler(SocketMessage arg)
        {
            if (arg is SocketUserMessage msg)
            {
                message = msg;
                context = new SocketCommandContext(client, message);

                if (message.Author.Id == ID || message.Channel.GetType() == typeof(SocketDMChannel)) return;

                Data.TryLoadAll(context.Guild.Id);

                if (!context.Guild.HasAllMembers) await context.Guild.DownloadUsersAsync();

                int argPos = 0;

                if (message.HasStringPrefix(Data.misc.Data.prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))
                {
                    IResult result = await commands.ExecuteAsync(context, argPos, services);

                    if (!result.IsSuccess) await LogHandler(new LogMessage(LogSeverity.Error, "", result.ErrorReason));
                    Log.Write(LogItem(context.User) + " has executed command: '" + message.Content + "'");
                }

                await HandlesModule.OnUserMessageRecieved.Invoke(message);

                Data.SaveAll(context.Guild.Id);
            }
        }
    }
}
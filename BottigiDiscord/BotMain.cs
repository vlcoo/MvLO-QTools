﻿using System.Text;
using Discord;
using Discord.WebSocket;
using ReplayFile;
using ReplayViewer;
using ReplayViewer.ReplayDrawers;

namespace BottigiDiscord;

public static class BotMain
{
    private static readonly Dictionary<byte, string> TeamNames = new()
    {
        {0, "\u001b[0;31m"},
        {1, "\u001b[0;32m"},
        {2, "\u001b[0;34m"},
        {3, "\u001b[0;33m"},
        {4, "\u001b[0;35m"},
    };

    private static readonly Dictionary<byte, string> CharacterNames = new()
    {
        {0, "\u001b[1;31mM\u001b[0;0m"},
        {1, "\u001b[1;32mL\u001b[0;0m"},
    };
    
    private static readonly DiscordSocketConfig Intents = new()
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    };
    private static readonly DiscordSocketClient Bot = new(Intents);

    private static readonly HttpClient Downloader = new();
    
    public static async Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("DiscordTokenBottigi", EnvironmentVariableTarget.User);
        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("Bot token missing. Please set the 'DiscordTokenBottigi' user environment variable.");
        }

        Bot.Log += Log;
        await Bot.LoginAsync(TokenType.Bot, token);
        await Bot.StartAsync();
        
        Bot.MessageReceived += OnMessageReceived;

        await Task.Delay(-1);
    }

    private static async Task OnMessageReceived(SocketMessage msg)
    {
        if (msg.Author.IsBot) return;
        if (msg.Attachments.Count == 0) return;

        foreach (var attachment in msg.Attachments)
        {
            if (!attachment.Filename.EndsWith(".mvlreplay")) continue;
            await SendReplayReply(msg, attachment);
            // await SendReplayVideo(msg, attachment);
        }
    }

    private static async Task SendReplayVideo(SocketMessage msg, Attachment replayAttachment)
    {
        var channel = msg.Channel;
        await using var stream = await Downloader.GetStreamAsync(replayAttachment.Url);
        var replay = new BinaryReplayMatch(stream);
        if (!replay.Valid)
        {
            await msg.AddReactionAsync(Emoji.Parse("❔"));
            return;
        }
        
        await msg.AddReactionAsync(Emoji.Parse("⌛"));
        var replayDrawer = new VideoReplayDrawer(false);
        var task = Task.Run(() => replay.Start(replayDrawer));
        await task;
        await channel.SendFileAsync(replayDrawer.OutputStream, "replay.mp4");
        await msg.RemoveReactionAsync(Emoji.Parse("⌛"), Bot.CurrentUser);
    }

    private static async Task SendReplayReply(SocketMessage msg, Attachment replayAttachment)
    {
        var channel = msg.Channel;
        await using var stream = await Downloader.GetStreamAsync(replayAttachment.Url);
        var replay = new BinaryReplayFile(stream);
        if (!replay.Valid)
        {
            await msg.AddReactionAsync(Emoji.Parse("❔"));
            return;
        }

        var playersString = new StringBuilder();
        foreach (var player in replay.Players)
        {
            playersString.Append($"{CharacterNames[player.Character]} ");
            
            if (replay.Rules.IsTeamsEnabled) playersString.Append($"{TeamNames[player.Team]}");
            playersString.Append(player.Username.PadRight(20));
            if (replay.Rules.IsTeamsEnabled) playersString.Append("\u001b[0;0m");
            
            playersString.Append($"{player.FinalStarCount}\u2605".PadRight(3));
            
            var hasWon = false;
            if (replay.Rules.IsTeamsEnabled) hasWon = player.Team == replay.WinningTeam;
            else if (replay.WinningPlayer != null) hasWon = player == replay.WinningPlayer;
            if (hasWon) playersString.Append(" \ud83c\udfc6");
            
            playersString.AppendLine();
        }

        var embed = new EmbedBuilder
        {
            Title = $"MvLO Match Replay{(string.IsNullOrEmpty(replay.CustomName) ? "" : $" • \"{replay.CustomName}\"")}",
            Color = Color.Purple,
            ThumbnailUrl = StageIconGetter.GetIconUrl(replay.Rules.StageName),
            Footer = new EmbedFooterBuilder
            {
                Text = $"Generated by MvLO-QTools v{BinaryReplayFile.SolutionVersion}",
            },
        };

        embed.AddField("Stage", replay.Rules.StageName, true);
        embed.AddField("Coins/p.up", replay.Rules.CoinsForPowerup, true);
        embed.AddField("Lives", replay.Rules.Lives, true);
        embed.AddField("Stars", replay.Rules.StarsToWin, true);
        embed.AddField("Date", replay.ReplayDate, true);
        embed.AddField("Duration", replay.ReplayLength, true);
        embed.AddField("Participants", $"```ansi\n{playersString}\n```");
        
        await channel.SendMessageAsync(embed: embed.Build());
    }
    
    private static string GlobalNameOrUsername(this IUser user) => user.GlobalName ?? user.Username;

    private static string CleanDateTimeString(this DateTimeOffset timestamp) =>
        $"{timestamp.Year}-{timestamp.Month:D2}-{timestamp.Day:D2} {timestamp.Hour:D2}:{timestamp.Minute:D2}";
    
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine("### " + msg.ToString());
        return Task.CompletedTask;
    }
}
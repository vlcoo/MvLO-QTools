using System.Text;
using System.Text.RegularExpressions;

namespace ReplayFile;

public class BinaryReplayFile
{
    public const int SolutionVersion = 0;

    public readonly ReplayFormat? Format;
    public readonly long FileSize;
    public readonly bool Valid;
    public readonly GameVersion Version;
    private readonly long _unixTimestamp;
    public string ReplayDate => DateTimeOffset.FromUnixTimeSeconds(_unixTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    protected readonly int InitialFrameNumber;
    protected readonly int ReplayLengthInFrames;
    public string ReplayDuration => TimeSpan.FromSeconds(ReplayLengthInFrames / 60f).ToString(@"m\m\ ss\s");
    public readonly string CustomName = "";
    public readonly GameRules Rules;
    public readonly ReplayPlayerInfo[] Players = [];
    public readonly sbyte WinningTeam = -1;
    public ReplayPlayerInfo? WinningPlayer => Rules.IsTeamsEnabled ? null : Players[WinningTeam];
    
    protected readonly byte[]? CompressedRuntimeConfigData;
    protected readonly byte[]? CompressedDeterministicConfigData;
    protected readonly byte[]? CompressedInitialFrameData;
    protected readonly byte[]? CompressedInputData;

    public BinaryReplayFile(Stream input, bool readQData = false)
    {
        var memInput = new MemoryStream();
        if (!input.CanSeek)
        {
            input.CopyTo(memInput);
            input.Dispose();
        }
        using var reader = new BinaryReader(memInput.Length > 0 ? memInput : input, Encoding.ASCII);
        FileSize = reader.BaseStream.Length;

        try
        {
            // the header can be any of the ones in the lookup table. check each one?
            foreach (var validFormat in ReplayModMapper.ValidReplayFormats)
            {
                reader.BaseStream.Position = 0;
                var headerLength = Encoding.ASCII.GetByteCount(validFormat.Header);
                var headerBuffer = new byte[headerLength];
                if (reader.Read(headerBuffer, 0, headerLength) != headerLength) continue;
                if (Encoding.ASCII.GetString(headerBuffer) != validFormat.Header) continue;
                Format = validFormat;
                break;
            }

            if (Format == null) return;

            Version = new GameVersion
            {
                Major = reader.ReadByte(),
                Minor = reader.ReadByte(),
                Patch = reader.ReadByte(),
                Hotfix = reader.ReadByte(),
            };
            _unixTimestamp = reader.ReadInt64();
            InitialFrameNumber = reader.ReadInt32();
            ReplayLengthInFrames = reader.ReadInt32();
            CustomName = reader.ReadString();

            Rules = new GameRules(reader.ReadString());

            Players = new ReplayPlayerInfo[reader.ReadByte()];
            for (var i = 0; i < Players.Length; i++)
            {
                Players[i] = new ReplayPlayerInfo
                {
                    Username = reader.ReadString(),
                    FinalObjectiveCount = reader.ReadInt32(),
                    Team = reader.ReadByte(),
                    Character = reader.ReadByte(),
                };
                reader.ReadInt32(); // Quantum Ref
            }
            
            WinningTeam = reader.ReadSByte();

            if (readQData)
            {
                var runtimeConfigLength = reader.ReadInt32();
                var deterministicConfigLength = reader.ReadInt32();
                var initialFrameLength = reader.ReadInt32();
                var inputDataLength = reader.ReadInt32();
                CompressedRuntimeConfigData = reader.ReadBytes(runtimeConfigLength);
                CompressedDeterministicConfigData = reader.ReadBytes(deterministicConfigLength);
                CompressedInitialFrameData = reader.ReadBytes(initialFrameLength);
                CompressedInputData = reader.ReadBytes(inputDataLength);
            }

            Valid = true;
        }
        catch (Exception e)
        {
            // unexpected data. valid is false
            Console.WriteLine(e.Message);
        }
    }
}

[Serializable]
public struct GameRules
{
    private static readonly Dictionary<long, string> StageNames = new()
    {
        { 438935048561577377, "Grassland" },
        { 422133218138107384, "Bricks" },
        { 472204063534919154, "Fortress" },
        { 329327285755650127, "Pipes" },
        { 372220640476305493, "Ice" },
        { 365460248894597713, "Jungle" },
        { 328495164863943948, "Sky" },
        { 420358832928607506, "Bonus" },
        { 269186410886596865, "Volcano" },
        { 280769179842433036, "Desert" },
        { 503647763446439242, "Ghost House" },
        { 372234066173501830, "Beach" },
    };

    private static readonly Dictionary<long, string> GamemodeNames = new()
    {
        { 394255533064620237, "Coin Runners" },
        { 326486899682662279, "Star Chasers" },
    };
    
    public string StageName;
    public string GamemodeName;
    public int StarsToWin;
    public int CoinsForPowerup;
    public int Lives;
    public int TimerMinutes;
    public bool IsTeamsEnabled;
    public bool IsCustomPowerupsEnabled;
    public bool IsDrawOnTimeUp;

    public GameRules(string json)
    {
        var stageMatch = new Regex(@"""Stage"":{""Id"":{""Value"":(\d{18})", RegexOptions.IgnoreCase).Match(json);
        var gamemodeMatch = new Regex(@"""Gamemode"":{""Id"":{""Value"":(\d{18})", RegexOptions.IgnoreCase).Match(json);
        var starsMatch = new Regex(@"""StarsToWin"":(\d+)", RegexOptions.IgnoreCase).Match(json);
        var coinsMatch = new Regex(@"""CoinsForPowerup"":(\d+)", RegexOptions.IgnoreCase).Match(json);
        var livesMatch = new Regex(@"""Lives"":(\d+)", RegexOptions.IgnoreCase).Match(json);
        var timerMatch = new Regex(@"""TimerMinutes"":(\d+)", RegexOptions.IgnoreCase).Match(json);
        var teamsMatch = new Regex(@"""TeamsEnabled"":{""Value"":(\d)}", RegexOptions.IgnoreCase).Match(json);
        var customPowerupsMatch = new Regex(@"""CustomPowerupsEnabled"":{""Value"":(\d)}", RegexOptions.IgnoreCase).Match(json);
        var drawOnTimeUpMatch = new Regex(@"""DrawOnTimeUp"":{""Value"":(\d)}", RegexOptions.IgnoreCase).Match(json);
        if (!stageMatch.Success || !starsMatch.Success || !coinsMatch.Success || !livesMatch.Success ||
            !timerMatch.Success || !teamsMatch.Success || !customPowerupsMatch.Success ||
            !drawOnTimeUpMatch.Success || !gamemodeMatch.Success) throw new ArgumentException("Unexpected JSON schema!!");

        StageName = StageNames.TryGetValue(long.Parse(stageMatch.Groups[1].Value), out var name) ? name : "Unknown";
        GamemodeName = GamemodeNames.TryGetValue(long.Parse(gamemodeMatch.Groups[1].Value), out var mode) ? mode : "Unknown";
        StarsToWin = int.Parse(starsMatch.Groups[1].Value);
        CoinsForPowerup = int.Parse(coinsMatch.Groups[1].Value);
        Lives = int.Parse(livesMatch.Groups[1].Value);
        TimerMinutes = int.Parse(timerMatch.Groups[1].Value);
        IsTeamsEnabled = int.Parse(teamsMatch.Groups[1].Value) > 0;
        IsCustomPowerupsEnabled = int.Parse(customPowerupsMatch.Groups[1].Value) > 0;
        IsDrawOnTimeUp = int.Parse(drawOnTimeUpMatch.Groups[1].Value) > 0;
    }
    
    public static string PropertyToString(int value, string suffix = "") => value > 0 ? value + suffix : "Off";
}

public record struct ReplayPlayerInfo
{
    public string Username;
    public int FinalObjectiveCount;
    public byte Team, Character;

    public bool Equals(ReplayPlayerInfo other) => Username == other.Username;
    public override int GetHashCode() => Username.GetHashCode();
}

public struct GameVersion
{
    public byte Major, Minor, Patch, Hotfix;
}
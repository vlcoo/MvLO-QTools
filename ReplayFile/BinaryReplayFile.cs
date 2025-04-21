using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ReplayFile;

public class BinaryReplayFile
{
    public const int SolutionVersion = 0;
    private const string MagicHeader = "MvLO-RP";
    private static int MagicHeaderLength => Encoding.ASCII.GetByteCount(MagicHeader);
    private static readonly byte[] HeaderBuffer = new byte[MagicHeaderLength];

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
        using var reader = new BinaryReader(input, Encoding.ASCII);
        FileSize = reader.BaseStream.CanSeek ? reader.BaseStream.Length : -1;

        try
        {
            var read = reader.Read(HeaderBuffer, 0, MagicHeaderLength);
            if (read != MagicHeaderLength) return;
            var readString = Encoding.ASCII.GetString(HeaderBuffer);
            if (readString != MagicHeader) return;

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

#pragma warning disable SYSLIB0011
            var formatter = new BinaryFormatter
            {
                Binder = new QuantumBinder()
            };
            Rules = (GameRules)formatter.Deserialize(input);
#pragma warning restore SYSLIB0011

            Players = new ReplayPlayerInfo[reader.ReadByte()];
            for (var i = 0; i < Players.Length; i++)
            {
                Players[i] = new ReplayPlayerInfo
                {
                    Username = reader.ReadString(),
                    FinalStarCount = reader.ReadByte(),
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
        catch (Exception)
        {
            // unexpected data. valid is false
        }
    }
}

[Serializable]
public struct GameRules
{
    private static readonly Dictionary<long, string> StageNames = new()
    {
        {438935048561577377, "Grassland"},
        {422133218138107384, "Bricks"},
        {472204063534919154, "Fortress"},
        {329327285755650127, "Pipes"},
        {372220640476305493, "Ice"},
        {365460248894597713, "Jungle"},
        {328495164863943948, "Sky"},
        {420358832928607506, "Bonus"},
        {269186410886596865, "Volcano"},
        {280769179842433036, "Desert"},
        {503647763446439242, "Ghost House"},
        {372234066173501830, "Beach"},
    };
    
    private QAsset Stage;
    public string StageName => StageNames.TryGetValue(Stage.Id.Value, out var name) ? name : "unknown";
    public int StarsToWin;
    public int CoinsForPowerup;
    public int Lives;
    public int TimerSeconds;
    private QBool TeamsEnabled;
    public bool IsTeamsEnabled => TeamsEnabled.Value > 0;
    private QBool CustomPowerupsEnabled;
    public bool IsCustomPowerupsEnabled => CustomPowerupsEnabled.Value > 0;
    private QBool DrawOnTimeUp;
    public bool IsDrawOnTimeUp => DrawOnTimeUp.Value > 0;

    [Serializable]
    public struct QBool
    {
        public int Value;
    }
    
    [Serializable]
    public struct QAsset
    {
        public QGuid Id;
    }
    
    [Serializable]
    public struct QGuid
    {
        public long Value;
    }
    
    public static string PropertyToString(int value) => value > 0 ? value.ToString() : "Off";
}

public class QuantumBinder : SerializationBinder
{
    public override Type? BindToType(string assemblyName, string typeName)
    {
        if (typeName.StartsWith("Quantum.AssetRef"))
            return typeof(GameRules.QAsset);

        return typeName switch
        {
            "Quantum.Prototypes.GameRulesPrototype" => typeof(GameRules),
            "Quantum.AssetGuid" => typeof(GameRules.QGuid),
            "Quantum.QBoolean" => typeof(GameRules.QBool),
            _ => Type.GetType($"{typeName}, {assemblyName}")
        };
    }
}

public struct ReplayPlayerInfo
{
    public string Username;
    public byte FinalStarCount;
    public byte Team;
    public byte Character;

    public static bool operator ==(ReplayPlayerInfo left, ReplayPlayerInfo right) => left.Username.Equals(right.Username);
    public static bool operator !=(ReplayPlayerInfo left, ReplayPlayerInfo right) => !(left == right);
}

public struct GameVersion
{
    public byte Major, Minor, Patch, Hotfix;
}
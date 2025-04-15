using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ReplayFile;

public class BinaryReplayFile
{
    private const string MagicHeader = "MvLO-RP";
    private static int MagicHeaderLength => Encoding.ASCII.GetByteCount(MagicHeader);
    private static readonly byte[] HeaderBuffer = new byte[MagicHeaderLength];

    public long FileSize;
    public bool Valid;
    public GameVersion Version;
    public long UnixTimestamp;
    public string ReplayDate => DateTimeOffset.FromUnixTimeSeconds(UnixTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
    public int InitialFrameNumber;
    public int ReplayLengthInFrames;
    public string ReplayLength => TimeSpan.FromSeconds(ReplayLengthInFrames / 60f).ToString(@"mm\m\ ss\s");
    public string CustomName = "";
    public GameRules Rules;
    // public byte PlayerCount;
    // public byte[] PlayerStars = new byte[10];
    // public byte[] PlayerTeams = new byte[10];
    // public string[] PlayerNames = new string[10];
    public ReplayPlayerInfo[] Players = [];
    public sbyte WinningTeam = -1;

    public BinaryReplayFile(Stream input)
    {
        using var reader = new BinaryReader(input, Encoding.ASCII);
        FileSize = reader.BaseStream.Length;

        try
        {
            reader.Read(HeaderBuffer, 0, MagicHeaderLength);
            var readString = Encoding.ASCII.GetString(HeaderBuffer);
            if (readString != MagicHeader)
            {
                Valid = false;
                return;
            }

            // Version = reader.ReadByte();
            Version = new GameVersion
            {
                Major = reader.ReadByte(),
                Minor = reader.ReadByte(),
                Patch = reader.ReadByte(),
                Hotfix = reader.ReadByte(),
            };
            UnixTimestamp = reader.ReadInt64();
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
            }
                
            WinningTeam = reader.ReadSByte();

            Valid = true;
        }
        catch (Exception)
        {
            Valid = false;
        }
    }
}

[Serializable]
public struct GameRules
{
    private static readonly Dictionary<long, string> StageNames = new Dictionary<long, string>
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
    public string StageName => StageNames.GetValueOrDefault(Stage.Id.Value, "unknown");
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
}

public struct GameVersion
{
    public byte Major, Minor, Patch, Hotfix;
}
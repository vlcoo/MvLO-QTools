using Photon.Deterministic;
using Quantum;
using ReplayFile;
using System.Text;

namespace ReplayViewer;

public class BinaryReplayMatch(Stream input) : BinaryReplayFile(input, true)
{
    private byte[] RuntimeConfigData => ByteUtils.GZipDecompressBytes(CompressedRuntimeConfigData);
    private byte[] DeterministicConfigData => ByteUtils.GZipDecompressBytes(CompressedDeterministicConfigData);
    private byte[] InitialFrameData => ByteUtils.GZipDecompressBytes(CompressedInitialFrameData);
    private byte[] InputData => ByteUtils.GZipDecompressBytes(CompressedInputData);

    public unsafe void Start()
    {
        if (!Valid) return;
        var maxFrame = InitialFrameNumber + ReplayLengthInFrames;
        FPLut.Init(AppDomain.CurrentDomain.BaseDirectory + "resources/LUT");
        
        var serializer = new QuantumJsonSerializer();
        var runtimeConfig = serializer.ConfigFromByteArray<RuntimeConfig>(RuntimeConfigData, compressed: true); // will change
        var deterministicConfig = DeterministicSessionConfig.FromByteArray(DeterministicConfigData);
        var inputStream = new BitStream(InputData);
        var replayInputProvider =
            new BitStreamReplayInputProvider(inputStream, maxFrame);
        var resourceManager = new ResourceManagerStatic(serializer.AssetsFromByteArray(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "resources/db.json")), DotNetRunnerFactory.CreateNativeAllocator());
        var callbackDispatcher = new CallbackDispatcher();
        var eventDispatcher = new EventDispatcher();
        // eventDispatcher.Subscribe<EventGameStarted>(this, _ => E("Game started"));
        // eventDispatcher.Subscribe<EventGameEnded>(this, _ => E("Game ended"));
        // eventDispatcher.Subscribe<EventMarioPlayerDied>(this, _ => E("Player died"));
        // eventDispatcher.Subscribe<EventMarioPlayerRespawned>(this, _ => E("Player respawned"));
        // eventDispatcher.Subscribe<EventMarioPlayerJumped>(this, _ => E("Player jumped"));
        // eventDispatcher.Subscribe<EventGameStateChanged>(this, _ => E("Game state changed"));
        // eventDispatcher.Subscribe<EventMarioPlayerTookDamage>(this, _ => E("Player took damage"));
        callbackDispatcher.Subscribe<CallbackSimulateFinished>(this, e =>
        {
            /*
            var f = e.Game.Frames.Predicted;
            var marios = f.Filter<Transform2D, MarioPlayer>();
            marios.UseCulling = false;
            E($"Tick #{f.Number}");
            while (marios.NextUnsafe(out var entity, out var transform, out var mario))
            {
                Console.Write($"{mario->PlayerRef} is at {transform->Position}\t".PadRight(46));
            }
            Console.WriteLine();
            */
            DrawGame(e.Frame);
        });
        
        var arguments = new SessionRunner.Arguments
        {
            RunnerFactory = new DotNetRunnerFactory(),
            AssetSerializer = serializer,
            CallbackDispatcher = callbackDispatcher,
            EventDispatcher = eventDispatcher,
            ResourceManager = resourceManager,
            GameFlags = QuantumGameFlags.EnableTaskProfiler,
            ReplayProvider = replayInputProvider,
            GameMode = DeterministicGameMode.Replay,
            RuntimeConfig = runtimeConfig,
            SessionConfig = deterministicConfig,
            RunnerId = "LOCALREPLAY",
            PlayerCount = deterministicConfig.PlayerCount,
            InitialTick = InitialFrameNumber,
            FrameData = InitialFrameData,
            DeltaTimeType = SimulationUpdateTime.EngineDeltaTime,
        };
        deterministicConfig.ChecksumInterval = 0;
        var runner = SessionRunner.Start(arguments);

        while (runner.Session.FramePredicted == null || runner.Session.FramePredicted.Number < maxFrame)
        {
            Thread.Sleep(10);
            runner.Service();
        }
        
        Console.WriteLine("finished!");

        runner.Shutdown();
        resourceManager.Dispose();
    }

    private static readonly StringBuilder builder = new();
    private static unsafe void DrawGame(Frame f) {
        var tiles = f.StageTiles;

        builder.Clear();
        VersusStageData stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);
        if (tiles != null) {
            char[,] test = new char[stage.TileDimensions.x, stage.TileDimensions.y];
            for (int x = 0; x < test.GetLength(0); x++) {
                for (int y = 0; y < test.GetLength(1); y++) {
                    test[x, y] = tiles[x + y * stage.TileDimensions.x].HasWorldPolygons(f) ? '#' : ' ';
                }
            }

            var all = f.Filter<Transform2D>();
            all.UseCulling = false;
            while (all.NextUnsafe(out _, out Transform2D* transform)) {
                Vector2Int pos = QuantumUtils.WorldToRelativeTile(f, transform->Position);
                if (pos.x >= 0 && pos.x < test.GetLength(0)
                    && pos.y >= 0 && pos.y < test.GetLength(1)) {
                    test[pos.x, pos.y] = 'E';
                }
            }

            for (int y = test.GetLength(1) - 1; y >= 0; y--) {
                for (int x = 0; x < test.GetLength(0); x++) {
                    builder.Append(test[x, y]);
                }
                builder.AppendLine();
            }
            builder.Append("Frame #").AppendLine(f.Number.ToString());
            
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(builder.ToString());
        }
    }

    private static void E(string msg)
    {
        Console.WriteLine("## Event: " + msg);
    }
}
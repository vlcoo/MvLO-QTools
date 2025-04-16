using Photon.Deterministic;
using Quantum;
using ReplayFile;

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
            var f = e.Game.Frames.Predicted;
            var marios = f.Filter<Transform2D, MarioPlayer>();
            marios.UseCulling = false;
            E($"Tick #{f.Number}");
            while (marios.NextUnsafe(out var entity, out var transform, out var mario))
            {
                Console.Write($"{mario->PlayerRef} is at {transform->Position}\t".PadRight(46));
            }
            Console.WriteLine();
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
            runner.Service(0.1f);
        }
        
        Console.WriteLine("finished!");

        runner.Shutdown();
        resourceManager.Dispose();
    }

    private static void E(string msg)
    {
        Console.WriteLine("## Event: " + msg);
    }
}
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
    
    public ResourceManagerStatic ResourceManager { get; private set; }
    public SimulationConfig SimulationConfig { get; private set; }

    private ReplayDrawer Drawer;

    public void Start(ReplayDrawer drawer)
    {
        if (!Valid) return;
        var maxFrame = InitialFrameNumber + ReplayLengthInFrames;
        FPLut.Init(AppDomain.CurrentDomain.BaseDirectory + "resources/LUT");
        Drawer = drawer;
        
        var serializer = new QuantumJsonSerializer();
        var runtimeConfig = serializer.ConfigFromByteArray<RuntimeConfig>(RuntimeConfigData, compressed: true); // will change
        var deterministicConfig = DeterministicSessionConfig.FromByteArray(DeterministicConfigData);
        var inputStream = new BitStream(InputData);
        var replayInputProvider =
            new BitStreamReplayInputProvider(inputStream, maxFrame);
        ResourceManager =
            new ResourceManagerStatic(
                serializer.AssetsFromByteArray(
                    File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "resources/db.json")),
                DotNetRunnerFactory.CreateNativeAllocator());
        var callbackDispatcher = new CallbackDispatcher();
        var eventDispatcher = new EventDispatcher();
        drawer.EventDispatcher = eventDispatcher;
        drawer.Replay = this;
        callbackDispatcher.Subscribe<CallbackSimulateFinished>(this,
            e => Drawer.DrawFrame(e.Game.Frames.Predicted));
        
        var arguments = new SessionRunner.Arguments
        {
            RunnerFactory = new DotNetRunnerFactory(),
            AssetSerializer = serializer,
            CallbackDispatcher = callbackDispatcher,
            EventDispatcher = eventDispatcher,
            ResourceManager = ResourceManager,
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
        SimulationConfig = (SimulationConfig) ResourceManager.GetAsset(runtimeConfig.SimulationConfig.Id);
        
        while (runner.Session.FramePredicted == null || runner.Session.FramePredicted.Number < maxFrame)
        {
            Thread.Sleep((int) Math.Max(1, Drawer.Speed));
            runner.Service(Drawer.Speed > 0 ? 1.0f / Drawer.Speed : null);
        }
        
        Console.WriteLine("Simulation ended!!");

        runner.Shutdown();
        ResourceManager.Dispose();
        drawer.Render();
    }
}
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
    private SessionRunner Runner;
    private int _maxFrame;

    public void Start(ReplayDrawer drawer)
    {
        if (!Valid) return;
        Drawer = drawer;
        Init();
        
        while (Runner.Session.FramePredicted == null || Runner.Session.FramePredicted.Number < _maxFrame)
        {
            Thread.Sleep((int) Math.Max(1, Drawer.Speed));
            Runner.Service(Drawer.Speed > 0 ? 1.0f / Drawer.Speed : null);
        }
        
        Console.WriteLine("Simulation ended!!");

        Runner.Shutdown();
        ResourceManager.Dispose();
        drawer.Render();
    }

    public void StartManual(ReplayDrawer drawer)
    {
        if (!Valid) return;
        Drawer = drawer;
        Init();
    }

    public void Step(double? delta = null)
    {
        if (Runner.Session.FramePredicted == null || Runner.Session.FramePredicted.Number < _maxFrame)
        {
            Runner.Service(delta);
        }
        else
        {
            Console.WriteLine("Simulation ended!!");

            Runner.Shutdown();
            ResourceManager.Dispose();
            Drawer.Render();
        }
    }

    private void Init()
    {
        _maxFrame = InitialFrameNumber + ReplayLengthInFrames;
        FPLut.Init(AppDomain.CurrentDomain.BaseDirectory + "resources/LUT");
        
        var serializer = new QuantumJsonSerializer();
        var runtimeConfig = serializer.ConfigFromByteArray<RuntimeConfig>(RuntimeConfigData, compressed: false);
        var deterministicConfig = DeterministicSessionConfig.FromByteArray(DeterministicConfigData);
        var inputStream = new BitStream(InputData);
        var replayInputProvider =
            new BitStreamReplayInputProvider(inputStream, _maxFrame);
        ResourceManager =
            new ResourceManagerStatic(
                serializer.AssetsFromByteArray(
                    File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "resources/db.json")),
                DotNetRunnerFactory.CreateNativeAllocator());
        var callbackDispatcher = new CallbackDispatcher();
        var eventDispatcher = new EventDispatcher();
        Drawer.EventDispatcher = eventDispatcher;
        Drawer.Replay = this;
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
        
        Runner = SessionRunner.Start(arguments);
        SimulationConfig = (SimulationConfig) ResourceManager.GetAsset(runtimeConfig.SimulationConfig.Id);
    }
}
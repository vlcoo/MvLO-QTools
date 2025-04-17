using Quantum;

namespace ReplayViewer;

public abstract class ReplayDrawer
{
    public EventDispatcher EventDispatcher { get; set; }
    public BinaryReplayMatch Replay { get; set; }

    public abstract void DrawFrame(Frame f);
    public abstract void Render();
}
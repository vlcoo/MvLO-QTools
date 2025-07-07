using Quantum;

namespace ReplayViewer.ReplayDrawers;

public class InputReplayDrawer : ReplayDrawer
{
    public override float Speed => 0f;
    public override unsafe void DrawFrame(Frame f)
    {
        var marios = f.Filter<MarioPlayer>();
        marios.UseCulling = false;
        marios.NextUnsafe(out var entity, out var mario);
        var input = f.GetPlayerInput(mario->PlayerRef);
        var b = new[]
        {
            ButtonToString(input->Up),
            ButtonToString(input->Down),
            ButtonToString(input->Left),
            ButtonToString(input->Right),
            ButtonToString(input->Jump),
            ButtonToString(input->Sprint),
        };
        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"""
                            {b[0]}    
                           {b[2]} {b[3]}  {b[5]}
                            {b[1]}    {b[4]}
                           """);
    }

    private string ButtonToString(Button button)
    {
        return button.IsDown ? "O" : "·";
    }

    public override void Render()
    {
        
    }
}
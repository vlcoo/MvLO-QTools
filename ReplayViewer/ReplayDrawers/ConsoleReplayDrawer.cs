using System.Text;
using Quantum;

namespace ReplayViewer.ReplayDrawers;

public class ConsoleReplayDrawer : ReplayDrawer
{
    private static readonly StringBuilder Builder = new();

    public override float Speed => 0f;

    public override void OnBeforeFirstFrame()
    {
        
    }

    public override unsafe void DrawFrame(Frame f)
    {
        var tiles = f.StageTiles;

        Builder.Clear();
        var stage = f.FindAsset<VersusStageData>(f.Map.UserAsset);
        if (tiles == null) return;
        var test = new char[stage.TileDimensions.X, stage.TileDimensions.Y];
        for (var x = 0; x < test.GetLength(0); x++) {
            for (var y = 0; y < test.GetLength(1); y++) {
                test[x, y] = tiles[x + y * stage.TileDimensions.X].HasWorldPolygons(f) ? '#' : ' ';
            }
        }

        var all = f.Filter<Transform2D>();
        all.UseCulling = false;
        while (all.NextUnsafe(out _, out var transform)) {
            var pos = QuantumUtils.WorldToRelativeTile(f, transform->Position);
            if (pos.X >= 0 && pos.X < test.GetLength(0)
                           && pos.Y >= 0 && pos.Y < test.GetLength(1)) {
                test[pos.X, pos.Y] = 'E';
            }
        }

        for (var y = test.GetLength(1) - 1; y >= 0; y--) {
            for (var x = 0; x < test.GetLength(0); x++) {
                Builder.Append(test[x, y]);
            }
            Builder.AppendLine();
        }
        Builder.Append("Frame #").AppendLine(f.Number.ToString());
            
        Console.SetCursorPosition(0, 0);
        Console.WriteLine(Builder.ToString());
    }

    public override void Render()
    {
        
    }
}
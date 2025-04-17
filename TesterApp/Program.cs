using ReplayViewer;
using ReplayViewer.ReplayDrawers;

const string replayPath = @"C:\Users\Victor\Desktop\mvlo stuff\Replay-1744507719.mvlreplay";
var replay = new BinaryReplayMatch(File.OpenRead(replayPath));
if (replay.Valid) replay.Start(new VideoReplayDrawer(true));
else Console.WriteLine("Invalid replay file.");

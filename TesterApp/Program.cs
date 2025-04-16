using ReplayViewer;

const string replayPath = @"C:\Users\Danny\Downloads\Replay-1744507719.mvlreplay";
var replay = new BinaryReplayMatch(File.OpenRead(replayPath));
if (replay.Valid) replay.Start();
else Console.WriteLine("Invalid replay file.");

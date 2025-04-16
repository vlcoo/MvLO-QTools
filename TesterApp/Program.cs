using ReplayViewer;

const string replayPath = @"C:\Users\Victor\Desktop\mvlo stuff\6-SpielerRundeaufHimmel.mvlreplay";
var replay = new BinaryReplayMatch(File.OpenRead(replayPath));
if (replay.Valid) replay.Start();
else Console.WriteLine("Invalid replay file.");

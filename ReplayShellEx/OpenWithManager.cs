using Microsoft.Win32;
using ReplayFile;

namespace ReplayShellEx;

// registering file types (3 goals):
// * - file type description
// ** - file icon in explorer
// *** - open with a specific executable
// 1. add to HKEY_CLASSES_ROOT a key with the extension, with value a meaningful name A (ProgID)
// 2. add to HKEY_CLASSES_ROOT a key with the name A, with the file type description as value *
// 3. add to HKEY_CLASSES_ROOT a key with the name A\DefaultIcon, with icon path as value **
// 4. add to HKEY_CLASSES_ROOT a key with the name A\shell\open\command, with the executable path as value (followed by something like \"%1\") ***

public static class OpenWithManager
{
    public const string ReplayProgId = "MvLO-QTools.Replay";
    
    public static void Main(string[] args)
    {
        if (args.Contains("-reg")) RegisterFileType();
        else if (args.Contains("-dereg")) DeregisterFileType();
        else if (args.Contains("-read") && args.Length > 1) ReadSampleReplayFile(args[1]);
        else Console.WriteLine("Please specify argument: '-reg' to add file type associations, '-dereg' to remove.");
    }

    private static void RegisterFileType()
    {
        Registry.SetValue(@"HKEY_CLASSES_ROOT\.mvlreplay", null, ReplayProgId);
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{ReplayProgId}", null, "MvLO Match Replay");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{ReplayProgId}\DefaultIcon", null, @"C:\ShellExtensions\mvlo1.ico,0");
        Registry.SetValue($@"HKEY_CLASSES_ROOT\{ReplayProgId}\shell\open\command", null, """
            C:\Users\Victor\Projects\CS\MvLO-QTools\ReplayShellEx\bin\Release\net8.0-windows\ReplayShellEx.exe -read "%1"
            """);
        Console.WriteLine("Installation attempt finished...!!");
    }

    private static void DeregisterFileType()
    {
        Registry.ClassesRoot.DeleteSubKey(".mvlreplay");
        Registry.ClassesRoot.DeleteSubKeyTree(ReplayProgId);
        Console.WriteLine("Uninstallation attempt finished...!!");
    }

    private static void ReadSampleReplayFile(string path)
    {
        var replay = new BinaryReplayFile(File.OpenRead(path));
        Console.WriteLine(
            replay.Valid
                ? $"{replay.Players.Length}-player match in {replay.Rules.StageName} with {replay.WinningPlayer?.Username} winning."
                : "Invalid replay file.");
        Console.ReadKey();
    }
}
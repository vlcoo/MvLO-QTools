using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using ReplayFile;
using SharpShell.Attributes;
using SharpShell.Interop;
using SharpShell.SharpIconOverlayHandler;

namespace ReplayShellEx;

[ComVisible(true)]
[DisplayName("MvLO Replay Icon Overlay Handler")]
public class ExtensionIconOverlay : SharpIconOverlayHandler
{
    protected override int GetPriority()
    {
        return 50;
    }

    protected override bool CanShowOverlay(string path, FILE_ATTRIBUTE attributes)
    {
        if (attributes.HasFlag(FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY)) return false;
        if (Path.GetExtension(path) != ".mvlreplay") return false;
        var replay = new BinaryReplayFile(File.OpenRead(path));
        return !replay.Valid;
    }

    protected override Icon GetOverlayIcon()
    {
        return new Icon(typeof(ExtensionIconOverlay).Assembly.GetManifestResourceStream("ReplayShellEx.resources.cross.ico")!);
    }
}
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ReplayFile;
using SharpShell.Attributes;
using SharpShell.SharpInfoTipHandler;

namespace ReplayShellEx;

[ComVisible(true)]
[DisplayName("MvLO Replay InfoTip Handler")]
[COMServerAssociation(AssociationType.ClassOfExtension, ".mvlreplay")]
public class ExtensionInfoTip : SharpInfoTipHandler
{
    protected override string GetInfo(RequestedInfoType infoType, bool singleLine)
    {
        var replay = new BinaryReplayFile(File.OpenRead(SelectedItemPath));
        var description = new StringBuilder();
        if (replay.Valid)
        {
            description.Append($"{replay.Players.Length}-player game on {replay.Rules.StageName}\n");
            description.Append("Date: ").Append(replay.ReplayDate).Append('\n');
            description.Append("Duration: ").Append(replay.ReplayDuration).Append('\n');
        }
        else
        {
            description.Append("Incompatible replay file!");
        }

        return infoType switch
        {
            RequestedInfoType.InfoTip => description.ToString(),
            RequestedInfoType.Name => "MvLO Match Replay",
            _ => string.Empty
        };
    }
}
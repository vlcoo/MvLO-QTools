using System.Drawing;

namespace ReplayShellEx;

public static class StageIconGetter
{
    public static Bitmap GetIconBitmap(string stageName)
    {
        var resourceName = stageName switch
        {
            "Grassland" => "ReplayShellEx.resources.stage_bitmaps.stage-grassland.bmp",
            "Bricks" => "ReplayShellEx.resources.stage_bitmaps.stage-brick.bmp",
            "Fortress" => "ReplayShellEx.resources.stage_bitmaps.stage-fortress.bmp",
            "Pipes" => "ReplayShellEx.resources.stage_bitmaps.stage-pipes.bmp",
            "Ice" => "ReplayShellEx.resources.stage_bitmaps.stage-ice.bmp",
            "Jungle" => "ReplayShellEx.resources.stage_bitmaps.stage-jungle.bmp",
            "Sky" => "ReplayShellEx.resources.stage_bitmaps.stage-sky.bmp",
            "Bonus" => "ReplayShellEx.resources.stage_bitmaps.stage-bonus.bmp",
            "Volcano" => "ReplayShellEx.resources.stage_bitmaps.stage-volcano.bmp",
            "Desert" => "ReplayShellEx.resources.stage_bitmaps.stage-desert.bmp",
            "Ghost House" => "ReplayShellEx.resources.stage_bitmaps.stage-ghosthouse.bmp",
            "Beach" => "ReplayShellEx.resources.stage_bitmaps.stage-beach.bmp",
            _ => "",
        };
        
        if (string.IsNullOrEmpty(resourceName)) return new Bitmap(1, 1);
        using var stream = typeof(StageIconGetter).Assembly.GetManifestResourceStream(resourceName);
        return stream == null ? new Bitmap(1, 1) : new Bitmap(stream);
    }
}
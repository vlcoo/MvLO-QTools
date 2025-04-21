namespace BottigiDiscord;

public static class StageIconGetter
{
    private static readonly Dictionary<string, string> IconPaths = new()
    {
        {"Grassland", "./resources/stage-icons/stage-grassland.png"},
        {"Bricks", "./resources/stage-icons/stage-brick.png"},
        {"Fortress", "./resources/stage-icons/stage-fortress.png"},
        {"Pipes", "./resources/stage-icons/stage-pipes.png"},
        {"Ice", "./resources/stage-icons/stage-ice.png"},
        {"Jungle", "./resources/stage-icons/stage-jungle.png"},
        {"Sky", "./resources/stage-icons/stage-sky.png"},
        {"Bonus", "./resources/stage-icons/stage-bonus.png"},
        {"Volcano", "./resources/stage-icons/stage-volcano.png"},
        {"Desert", "./resources/stage-icons/stage-desert.png"},
        {"Ghost House", "./resources/stage-icons/stage-ghosthouse.png"},
        {"Beach", "./resources/stage-icons/stage-beach.png"},
    };
    
    public static string GetIconPath(string stageName)
    {
        return IconPaths.GetValueOrDefault(stageName, "./resources/stage-icons/unknown.png");
    }
}
namespace ReplayFile;

public static class StageIconGetter
{
    private static readonly Dictionary<string, string> IconUrls = new()
    {
        {"Grassland", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-grassland.png"},
        {"Bricks", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-brick.png"},
        {"Fortress", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-fortress.png"},
        {"Pipes", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-pipes.png"},
        {"Ice", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-ice.png"},
        {"Jungle", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-jungle.png"},
        {"Sky", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-sky.png"},
        {"Bonus", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-bonus.png"},
        {"Volcano", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-volcano.png"},
        {"Desert", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-desert.png"},
        {"Ghost House", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-ghosthouse.png"},
        {"Beach", "https://raw.githubusercontent.com/vlcoo/MvLO-QTools/refs/heads/main/ReplayFile/resources/stage-icons/stage-beach.png"},
    };
    
    public static string GetIconUrl(string stageName)
    {
        return IconUrls.GetValueOrDefault(stageName, "");
    }
}
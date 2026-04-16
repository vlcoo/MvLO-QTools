namespace ReplayFile;

public static class ReplayModMapper
{
    public static readonly ReplayFormat[] ValidReplayFormats =
    [
        new()
        {
            Header = "MvLO-RP",
            Extension = ".mvlreplay",
            ModName = "Vanilla",
            IsVanilla = true,
        },
        new()
        {
            Header = "vcmi-RP",
            Extension = ".mvlreplay",
            ModName = "vic's Custom Match-inator",
        },
        new()
        {
            Header = "two-RP",
            Extension = ".mvlreplay",
            ModName = "20 Players",
        }
    ];
}

public record struct ReplayFormat
{
    public string Header, Extension, ModName;
    public bool IsVanilla;

    public bool Equals(ReplayFormat? other) => Header == other?.Header;
    public override int GetHashCode() => Header.GetHashCode();
}
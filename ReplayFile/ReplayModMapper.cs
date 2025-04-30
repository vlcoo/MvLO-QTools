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
        },
        new()
        {
            Header = "vcmi-RP",
            Extension = ".mvlreplay",
            ModName = "vic's Custom Match-inator",
        }
    ];
}

public record struct ReplayFormat
{
    public string Header, Extension, ModName;

    public bool Equals(ReplayFormat? other) => Header == other?.Header;
    public override int GetHashCode() => Header.GetHashCode();
}
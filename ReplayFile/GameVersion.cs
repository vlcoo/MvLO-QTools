namespace ReplayFile;

public struct GameVersion(byte major, byte minor, byte patch = 0, byte hotfix = 0)
    : IEquatable<GameVersion>, IComparable<GameVersion>
{
    public byte Major = major, Minor = minor, Patch = patch, Hotfix = hotfix;

    public readonly bool Equals(GameVersion other) {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch && Hotfix == other.Hotfix;
    }

    public readonly bool EqualsIgnoreHotfix(GameVersion other) {
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }

    public readonly int CompareTo(GameVersion other) {
        if (Major != other.Major) {
            return Major - other.Major;
        }
        if (Minor != other.Minor) {
            return Minor - other.Minor;
        }
        if (Patch != other.Patch) {
            return Patch - other.Patch;
        }
        return Hotfix - other.Hotfix;
    }

    public static bool operator >(GameVersion x, GameVersion y) {
        return x.CompareTo(y) > 0;
    }

    public static bool operator <(GameVersion x, GameVersion y) {
        return x.CompareTo(y) < 0;
    }

    public static bool operator >=(GameVersion x, GameVersion y) {
        return x.CompareTo(y) >= 0;
    }

    public static bool operator <=(GameVersion x, GameVersion y) {
        return x.CompareTo(y) <= 0;
    }

    public readonly override string ToString() {
        return $"{Major}.{Minor}.{Patch}.{Hotfix}";
    }
}
namespace Toast;

public record Location(int Line = 1, int Column = 1)
{
    public static readonly Location Unknown = new(1, 1);

    public override string ToString() => $"Line {Line}, Column {Column}";
}

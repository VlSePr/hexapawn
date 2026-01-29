namespace Hexapawns.Core;

/// <summary>
/// Represents a position on the 3x3 board
/// </summary>
public readonly struct Position
{
    public int Row { get; }
    public int Column { get; }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public bool IsValid() => Row >= 0 && Row < 3 && Column >= 0 && Column < 3;

    public override string ToString() => $"({Row},{Column})";

    public override bool Equals(object? obj) => 
        obj is Position other && Row == other.Row && Column == other.Column;

    public override int GetHashCode() => HashCode.Combine(Row, Column);

    public static bool operator ==(Position left, Position right) => left.Equals(right);
    public static bool operator !=(Position left, Position right) => !left.Equals(right);
}

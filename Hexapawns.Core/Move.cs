namespace Hexapawns.Core;

/// <summary>
/// Represents a move in the game
/// </summary>
public readonly struct Move
{
    public Position From { get; }
    public Position To { get; }
    public bool IsCapture { get; }

    public Move(Position from, Position to, bool isCapture = false)
    {
        From = from;
        To = to;
        IsCapture = isCapture;
    }

    public override string ToString() => 
        IsCapture ? $"{From}x{To}" : $"{From}->{To}";

    public override bool Equals(object? obj) =>
        obj is Move other && From == other.From && To == other.To;

    public override int GetHashCode() => HashCode.Combine(From, To);

    public static bool operator ==(Move left, Move right) => left.Equals(right);
    public static bool operator !=(Move left, Move right) => !left.Equals(right);
}

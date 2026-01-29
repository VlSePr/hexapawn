namespace Hexapawns.Core;

/// <summary>
/// Represents the state of a Hexapawn game
/// </summary>
public class GameState
{
    private readonly Player[,] _board;
    
    public Player CurrentPlayer { get; private set; }
    public GameResult Result { get; private set; }
    public bool IsGameOver => Result != GameResult.InProgress;

    public GameState()
    {
        _board = new Player[3, 3];
        InitializeBoard();
        CurrentPlayer = Player.White;
        Result = GameResult.InProgress;
    }

    // Copy constructor for making moves
    private GameState(GameState other)
    {
        _board = (Player[,])other._board.Clone();
        CurrentPlayer = other.CurrentPlayer;
        Result = other.Result;
    }

    private void InitializeBoard()
    {
        // White pawns on row 0
        for (int col = 0; col < 3; col++)
            _board[0, col] = Player.White;

        // Empty middle row
        for (int col = 0; col < 3; col++)
            _board[1, col] = Player.None;

        // Black pawns on row 2
        for (int col = 0; col < 3; col++)
            _board[2, col] = Player.Black;
    }

    public Player GetPiece(Position pos)
    {
        if (!pos.IsValid()) return Player.None;
        return _board[pos.Row, pos.Column];
    }

    public Player GetPiece(int row, int col)
    {
        if (row < 0 || row >= 3 || col < 0 || col >= 3) return Player.None;
        return _board[row, col];
    }

    /// <summary>
    /// Get all legal moves for the current player
    /// </summary>
    public List<Move> GetLegalMoves()
    {
        var moves = new List<Move>();
        
        if (IsGameOver) return moves;

        int direction = CurrentPlayer == Player.White ? 1 : -1;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                var pos = new Position(row, col);
                if (GetPiece(pos) != CurrentPlayer) continue;

                // Forward move
                var forward = new Position(row + direction, col);
                if (forward.IsValid() && GetPiece(forward) == Player.None)
                {
                    moves.Add(new Move(pos, forward, false));
                }

                // Capture left diagonal
                var captureLeft = new Position(row + direction, col - 1);
                if (captureLeft.IsValid() && GetPiece(captureLeft) == GetOpponent(CurrentPlayer))
                {
                    moves.Add(new Move(pos, captureLeft, true));
                }

                // Capture right diagonal
                var captureRight = new Position(row + direction, col + 1);
                if (captureRight.IsValid() && GetPiece(captureRight) == GetOpponent(CurrentPlayer))
                {
                    moves.Add(new Move(pos, captureRight, true));
                }
            }
        }

        return moves;
    }

    /// <summary>
    /// Make a move and return a new game state
    /// </summary>
    public GameState MakeMove(Move move)
    {
        var newState = new GameState(this);
        
        // Move the piece
        newState._board[move.To.Row, move.To.Column] = CurrentPlayer;
        newState._board[move.From.Row, move.From.Column] = Player.None;

        // Switch player
        newState.CurrentPlayer = GetOpponent(CurrentPlayer);

        // Check for game over
        newState.UpdateGameResult();

        return newState;
    }

    private void UpdateGameResult()
    {
        // Check if current player reached the opposite end
        if (CurrentPlayer == Player.White)
        {
            for (int col = 0; col < 3; col++)
            {
                if (_board[0, col] == Player.Black)
                {
                    Result = GameResult.BlackWins;
                    return;
                }
            }
        }
        else
        {
            for (int col = 0; col < 3; col++)
            {
                if (_board[2, col] == Player.White)
                {
                    Result = GameResult.WhiteWins;
                    return;
                }
            }
        }

        // Check if current player has no legal moves
        if (GetLegalMoves().Count == 0)
        {
            Result = CurrentPlayer == Player.White ? GameResult.BlackWins : GameResult.WhiteWins;
        }
    }

    private static Player GetOpponent(Player player) =>
        player == Player.White ? Player.Black : Player.White;

    /// <summary>
    /// Get a unique string representation of the board state
    /// </summary>
    public string GetBoardHash()
    {
        var chars = new char[9];
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                chars[row * 3 + col] = _board[row, col] switch
                {
                    Player.White => 'W',
                    Player.Black => 'B',
                    _ => '.'
                };
            }
        }
        return new string(chars);
    }

    public override string ToString()
    {
        var lines = new string[3];
        for (int row = 0; row < 3; row++)
        {
            var chars = new char[3];
            for (int col = 0; col < 3; col++)
            {
                chars[col] = _board[row, col] switch
                {
                    Player.White => 'W',
                    Player.Black => 'B',
                    _ => '.'
                };
            }
            lines[row] = new string(chars);
        }
        return string.Join("\n", lines);
    }
}

public enum GameResult
{
    InProgress,
    WhiteWins,
    BlackWins,
    Draw
}

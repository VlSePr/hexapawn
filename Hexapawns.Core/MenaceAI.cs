namespace Hexapawns.Core;

/// <summary>
/// MENACE (Machine Educable Noughts And Crosses Engine) implementation for Hexapawn
/// This AI learns from experience by adjusting move probabilities based on outcomes
/// </summary>
public class MenaceAI
{
    // For each board state, stores available moves and their "bead" counts
    private readonly Dictionary<string, Dictionary<Move, int>> _moveWeights;
    
    // History of states and moves for the current game
    private readonly List<(string state, Move move)> _gameHistory;
    
    private readonly Random _random;
    
    public int InitialBeadsPerMove { get; set; } = 3;
    public int RewardForWin { get; set; } = 3;
    public int PenaltyForLoss { get; set; } = 1;
    public int RewardForDraw { get; set; } = 1;

    // Statistics
    public int TotalGamesPlayed { get; private set; }
    public int GamesWon { get; private set; }
    public int GamesLost { get; private set; }
    public int GamesDrawn { get; private set; }

    public MenaceAI(int? seed = null)
    {
        _moveWeights = new Dictionary<string, Dictionary<Move, int>>();
        _gameHistory = new List<(string, Move)>();
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Select a move for the given game state
    /// </summary>
    public Move SelectMove(GameState state)
    {
        var boardHash = state.GetBoardHash();
        var legalMoves = state.GetLegalMoves();

        if (legalMoves.Count == 0)
            throw new InvalidOperationException("No legal moves available");

        // Initialize move weights for this state if not seen before
        if (!_moveWeights.ContainsKey(boardHash))
        {
            _moveWeights[boardHash] = new Dictionary<Move, int>();
            foreach (var move in legalMoves)
            {
                _moveWeights[boardHash][move] = InitialBeadsPerMove;
            }
        }

        // Get weights for legal moves
        var weights = _moveWeights[boardHash];
        
        // Remove any illegal moves that might exist (shouldn't happen, but safety first)
        var validWeights = weights.Where(kv => legalMoves.Contains(kv.Key)).ToList();
        
        // If all moves have been eliminated (all weights are 0), reinitialize with 1 bead each
        if (validWeights.All(kv => kv.Value <= 0))
        {
            foreach (var move in legalMoves)
            {
                weights[move] = 1;
            }
            validWeights = weights.Where(kv => legalMoves.Contains(kv.Key)).ToList();
        }

        // Select a move based on weighted random selection
        int totalWeight = validWeights.Sum(kv => Math.Max(0, kv.Value));
        int choice = _random.Next(totalWeight);
        
        int cumulative = 0;
        Move selectedMove = legalMoves[0]; // fallback
        
        foreach (var (move, weight) in validWeights)
        {
            if (weight <= 0) continue;
            cumulative += weight;
            if (choice < cumulative)
            {
                selectedMove = move;
                break;
            }
        }

        // Record this decision
        _gameHistory.Add((boardHash, selectedMove));

        return selectedMove;
    }

    /// <summary>
    /// Update weights based on game outcome
    /// </summary>
    public void LearnFromGame(GameResult result, Player aiPlayer)
    {
        TotalGamesPlayed++;

        int adjustment = 0;
        
        if (result == GameResult.Draw)
        {
            adjustment = RewardForDraw;
            GamesDrawn++;
        }
        else if ((result == GameResult.WhiteWins && aiPlayer == Player.White) ||
                 (result == GameResult.BlackWins && aiPlayer == Player.Black))
        {
            adjustment = RewardForWin;
            GamesWon++;
        }
        else
        {
            adjustment = -PenaltyForLoss;
            GamesLost++;
        }

        // Apply adjustment to all moves made in this game
        foreach (var (state, move) in _gameHistory)
        {
            if (_moveWeights.ContainsKey(state) && _moveWeights[state].ContainsKey(move))
            {
                _moveWeights[state][move] = Math.Max(0, _moveWeights[state][move] + adjustment);
            }
        }

        // Clear history for next game
        _gameHistory.Clear();
    }

    /// <summary>
    /// Reset learning (clear game history, keep statistics)
    /// </summary>
    public void StartNewGame()
    {
        _gameHistory.Clear();
    }

    /// <summary>
    /// Get statistics about learned positions
    /// </summary>
    public (int statesLearned, int totalMoveOptions) GetLearningStats()
    {
        int statesLearned = _moveWeights.Count;
        int totalMoveOptions = _moveWeights.Sum(kv => kv.Value.Count);
        return (statesLearned, totalMoveOptions);
    }

    /// <summary>
    /// Get win rate as a percentage
    /// </summary>
    public double GetWinRate()
    {
        if (TotalGamesPlayed == 0) return 0;
        return (double)GamesWon / TotalGamesPlayed * 100;
    }

    /// <summary>
    /// Get the move weights for a specific board state (for debugging/visualization)
    /// </summary>
    public Dictionary<Move, int>? GetMoveWeights(string boardHash)
    {
        return _moveWeights.TryGetValue(boardHash, out var weights) ? weights : null;
    }

    /// <summary>
    /// Get all positions the AI has learned about
    /// </summary>
    public IEnumerable<string> GetAllPositions()
    {
        return _moveWeights.Keys;
    }

    /// <summary>
    /// Get the game history for the current game
    /// </summary>
    public List<(string state, Move move)> GetGameHistory()
    {
        return _gameHistory.ToList();
    }

    /// <summary>
    /// Reset all learning data and statistics
    /// </summary>
    public void ResetLearning()
    {
        _moveWeights.Clear();
        _gameHistory.Clear();
        TotalGamesPlayed = 0;
        GamesWon = 0;
        GamesLost = 0;
        GamesDrawn = 0;
    }
}

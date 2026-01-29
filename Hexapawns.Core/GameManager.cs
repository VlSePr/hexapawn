namespace Hexapawns.Core;

/// <summary>
/// Manages game execution and training sessions
/// </summary>
public class GameManager
{
    public GameState CurrentState { get; private set; }
    public MenaceAI? WhiteAI { get; set; }
    public MenaceAI? BlackAI { get; set; }
    
    public event Action<GameState>? StateChanged;
    public event Action<Move, Player>? MoveMade;
    public event Action<GameResult>? GameEnded;

    public GameManager()
    {
        CurrentState = new GameState();
    }

    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartNewGame()
    {
        CurrentState = new GameState();
        WhiteAI?.StartNewGame();
        BlackAI?.StartNewGame();
        StateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// Make a move in the current game
    /// </summary>
    public bool MakeMove(Move move)
    {
        if (CurrentState.IsGameOver)
            return false;

        var legalMoves = CurrentState.GetLegalMoves();
        if (!legalMoves.Contains(move))
            return false;

        var previousPlayer = CurrentState.CurrentPlayer;
        CurrentState = CurrentState.MakeMove(move);
        
        MoveMade?.Invoke(move, previousPlayer);
        StateChanged?.Invoke(CurrentState);

        if (CurrentState.IsGameOver)
        {
            // Teach AIs based on result
            if (WhiteAI != null)
                WhiteAI.LearnFromGame(CurrentState.Result, Player.White);
            if (BlackAI != null)
                BlackAI.LearnFromGame(CurrentState.Result, Player.Black);

            GameEnded?.Invoke(CurrentState.Result);
        }

        return true;
    }

    /// <summary>
    /// Let the AI make a move for the current player
    /// </summary>
    public bool MakeAIMove()
    {
        if (CurrentState.IsGameOver)
            return false;

        var ai = CurrentState.CurrentPlayer == Player.White ? WhiteAI : BlackAI;
        if (ai == null)
            return false;

        var move = ai.SelectMove(CurrentState);
        return MakeMove(move);
    }

    /// <summary>
    /// Play a complete game with AIs
    /// </summary>
    public GameResult PlayAutomatedGame()
    {
        StartNewGame();
        
        while (!CurrentState.IsGameOver)
        {
            if (!MakeAIMove())
                break;
        }

        return CurrentState.Result;
    }

    /// <summary>
    /// Train AIs by playing multiple games
    /// </summary>
    public List<TrainingResult> TrainAIs(int numberOfGames, Action<int, GameResult>? progressCallback = null)
    {
        var results = new List<TrainingResult>();

        for (int i = 0; i < numberOfGames; i++)
        {
            var result = PlayAutomatedGame();
            
            var trainingResult = new TrainingResult
            {
                GameNumber = i + 1,
                Result = result,
                WhiteWinRate = WhiteAI?.GetWinRate() ?? 0,
                BlackWinRate = BlackAI?.GetWinRate() ?? 0,
                WhiteGamesWon = WhiteAI?.GamesWon ?? 0,
                BlackGamesWon = BlackAI?.GamesWon ?? 0,
                TotalGamesPlayed = i + 1
            };
            
            results.Add(trainingResult);
            progressCallback?.Invoke(i + 1, result);
        }

        return results;
    }
}

/// <summary>
/// Represents the result of a training game
/// </summary>
public class TrainingResult
{
    public int GameNumber { get; set; }
    public GameResult Result { get; set; }
    public double WhiteWinRate { get; set; }
    public double BlackWinRate { get; set; }
    public int WhiteGamesWon { get; set; }
    public int BlackGamesWon { get; set; }
    public int TotalGamesPlayed { get; set; }
}

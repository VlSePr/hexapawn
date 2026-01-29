using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hexapawns.Core;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Hexapawns.WPF;

public partial class MainViewModel : ObservableObject
{
    private readonly GameManager _gameManager;
    private readonly DispatcherTimer _autoPlayTimer;

    [ObservableProperty]
    private GameState _currentState;

    [ObservableProperty]
    private string _statusMessage = "Ready to play!";

    [ObservableProperty]
    private int _gamesPlayed;

    [ObservableProperty]
    private bool _isAutoPlaying;

    [ObservableProperty]
    private int _humanScore;

    [ObservableProperty]
    private int _aiScore;

    [ObservableProperty]
    private string _currentStateHash = "";

    [ObservableProperty]
    private ObservableCollection<MoveWeight> _currentMoveWeights = new();

    [ObservableProperty]
    private ObservableCollection<MatchboxInfo> _allMatchboxes = new();

    [ObservableProperty]
    private string _lastUpdatedPosition = "";

    [ObservableProperty]
    private Dictionary<string, Dictionary<string, int>> _previousBeadCounts = new();

    [ObservableProperty]
    private PlotModel _learningCurvePlot = null!;

    private List<int> _aiWinsByGame = new();

    public ObservableCollection<TrainingResult> TrainingHistory { get; set; }

    public MainViewModel()
    {
        _gameManager = new GameManager();
        _gameManager.WhiteAI = null; // Human plays White
        _gameManager.BlackAI = new MenaceAI(seed: 123);
        
        _currentState = _gameManager.CurrentState;
        
        _gameManager.StateChanged += OnStateChanged;
        _gameManager.GameEnded += OnGameEnded;

        TrainingHistory = new ObservableCollection<TrainingResult>();

        _autoPlayTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _autoPlayTimer.Tick += AutoPlayTick;
        
        // Don't pre-train - start fresh to demonstrate learning
        // InitializeAIKnowledge();
        
        InitializeLearningCurve();
        UpdateMatchboxVisualization();
    }

    private void InitializeLearningCurve()
    {
        LearningCurvePlot = new PlotModel { Title = "AI Learning Progress" };
        
        LearningCurvePlot.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Games Played",
            Minimum = 0,
            MinimumPadding = 0.1,
            MaximumPadding = 0.1
        });
        
        LearningCurvePlot.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "AI Victories",
            Minimum = 0,
            MinimumPadding = 0.1,
            MaximumPadding = 0.1
        });
        
        var lineSeries = new LineSeries
        {
            Title = "AI Wins",
            Color = OxyColors.Red,
            StrokeThickness = 2,
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerFill = OxyColors.Red
        };
        
        LearningCurvePlot.Series.Add(lineSeries);
    }

    private void InitializeAIKnowledge()
    {
        // Run initial training games so matchboxes are visible from start
        var whiteTrainer = new MenaceAI(seed: 456);
        var blackAI = _gameManager.BlackAI;
        
        for (int i = 0; i < 20; i++)
        {
            var tempManager = new GameManager();
            tempManager.WhiteAI = whiteTrainer;
            tempManager.BlackAI = blackAI;
            
            while (!tempManager.CurrentState.IsGameOver)
            {
                tempManager.MakeAIMove();
            }
        }
    }

    private void UpdateMatchboxVisualization()
    {
        var ai = _gameManager.BlackAI;
        if (ai == null) return;

        var currentBoardHash = _gameManager.CurrentState.GetBoardHash();
        
        // Capture current state as "previous" for next update
        var newPreviousBeadCounts = new Dictionary<string, Dictionary<string, int>>();
        
        // Get all positions the AI knows about
        var allPositions = ai.GetAllPositions();
        
        AllMatchboxes.Clear();
        
        foreach (var position in allPositions.OrderBy(p => p))
        {
            var weights = ai.GetMoveWeights(position);
            if (weights == null || !weights.Any()) continue;
            
            var matchbox = new MatchboxInfo
            {
                PositionHash = position,
                IsCurrentPosition = position == currentBoardHash,
                JustUpdated = position == LastUpdatedPosition,
                Moves = new ObservableCollection<MoveWeight>()
            };
            
            var positionBeads = new Dictionary<string, int>();
            
            foreach (var (move, weight) in weights.OrderByDescending(kv => kv.Value))
            {
                var moveKey = move.ToString();
                var previousWeight = 0;
                
                if (PreviousBeadCounts.TryGetValue(position, out var prevMoves))
                {
                    prevMoves.TryGetValue(moveKey, out previousWeight);
                }
                
                var moveWeight = new MoveWeight
                {
                    MoveDescription = moveKey,
                    BeadCount = Math.Max(0, weight),
                    PreviousBeadCount = previousWeight,
                    IsAvailable = weight > 0,
                    JustUpdated = matchbox.JustUpdated && previousWeight != weight,
                    BeadItems = Enumerable.Range(0, Math.Min(Math.Max(0, weight), 20)).ToList()
                };
                
                matchbox.Moves.Add(moveWeight);
                positionBeads[moveKey] = Math.Max(0, weight);
            }
            
            newPreviousBeadCounts[position] = positionBeads;
            AllMatchboxes.Add(matchbox);
        }
        
        // Update previous counts for next comparison
        PreviousBeadCounts = newPreviousBeadCounts;
        
        // Update current position info
        if (_gameManager.CurrentState.IsGameOver)
        {
            CurrentStateHash = "Game Over";
        }
        else if (_gameManager.CurrentState.CurrentPlayer == Player.White)
        {
            CurrentStateHash = "Your turn!";
        }
        else
        {
            CurrentStateHash = $"AI's turn - Position: {currentBoardHash}";
        }
    }

    private void OnStateChanged(GameState state)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentState = state;
            UpdateMatchboxVisualization();
        });
    }

    private void OnGameEnded(GameResult result)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Capture positions that were played in this game for highlighting
            var ai = _gameManager.BlackAI;
            if (ai != null)
            {
                var gamePositions = ai.GetGameHistory();
                if (gamePositions.Any())
                {
                    LastUpdatedPosition = gamePositions.Last().state;
                }
            }
            
            if (result == GameResult.WhiteWins)
            {
                HumanScore++;
                StatusMessage = "You Win! 🎉";
            }
            else if (result == GameResult.BlackWins)
            {
                AiScore++;
                StatusMessage = "AI Wins! 🤖";
            }
            else
            {
                StatusMessage = "Draw!";
            }
            
            GamesPlayed++;
            
            // Track cumulative AI wins for learning curve
            _aiWinsByGame.Add(AiScore);
            UpdateLearningCurve();
            
            // Update to show learned changes
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateMatchboxVisualization();
            }), System.Windows.Threading.DispatcherPriority.Background);
        });
    }

    private void UpdateLearningCurve()
    {
        var lineSeries = LearningCurvePlot.Series[0] as LineSeries;
        if (lineSeries != null)
        {
            lineSeries.Points.Clear();
            for (int i = 0; i < _aiWinsByGame.Count; i++)
            {
                lineSeries.Points.Add(new DataPoint(i + 1, _aiWinsByGame[i]));
            }
            LearningCurvePlot.InvalidatePlot(true);
        }
    }

    [RelayCommand]
    private void NewGame()
    {
        _gameManager.StartNewGame();
        StatusMessage = "Your turn!";
        // Don't clear LastUpdatedPosition - keep highlight visible
        // Update visualization to refresh current position highlight
        UpdateMatchboxVisualization();
    }

    [RelayCommand]
    private void MakeAIMove()
    {
        if (_gameManager.MakeAIMove())
        {
            StatusMessage = $"{_gameManager.CurrentState.CurrentPlayer}'s turn";
            UpdateMatchboxVisualization();
        }
    }

    public void OnHumanMove(Move move)
    {
        if (_gameManager.MakeMove(move))
        {
            if (_gameManager.CurrentState.IsGameOver)
            {
                StatusMessage = "Game Over!";
                UpdateMatchboxVisualization();
            }
            else
            {
                StatusMessage = "AI is thinking...";
                UpdateMatchboxVisualization();
                
                // Auto-play AI response after a short delay
                if (!IsAutoPlaying)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(800); // Delay to see matchbox
                        if (!_gameManager.CurrentState.IsGameOver && !IsAutoPlaying)
                        {
                            _gameManager.MakeAIMove();
                            StatusMessage = _gameManager.CurrentState.IsGameOver 
                                ? "Game Over!" 
                                : "Your turn!";
                            UpdateMatchboxVisualization();
                        }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }
    }

    [RelayCommand]
    private void ToggleAutoPlay()
    {
        IsAutoPlaying = !IsAutoPlaying;
        if (IsAutoPlaying)
        {
            StatusMessage = "Auto-playing...";
            _autoPlayTimer.Start();
        }
        else
        {
            StatusMessage = "Auto-play stopped";
            _autoPlayTimer.Stop();
        }
    }

    private void AutoPlayTick(object? sender, EventArgs e)
    {
        if (_gameManager.CurrentState.IsGameOver)
        {
            _gameManager.StartNewGame();
        }
        else
        {
            _gameManager.MakeAIMove();
        }
        UpdateMatchboxVisualization();
    }

    [RelayCommand]
    private void ResetScore()
    {
        HumanScore = 0;
        AiScore = 0;
        GamesPlayed = 0;
        _aiWinsByGame.Clear();
        UpdateLearningCurve();
        StatusMessage = "Score reset!";
    }

    [RelayCommand]
    private void ResetLearning()
    {
        _gameManager.BlackAI?.ResetLearning();
        LastUpdatedPosition = ""; // Clear highlight when resetting
        PreviousBeadCounts.Clear();
        StatusMessage = "AI learning reset - starting fresh!";
        UpdateMatchboxVisualization();
    }
}

public class MoveWeight
{
    public string MoveDescription { get; set; } = "";
    public int BeadCount { get; set; }
    public int PreviousBeadCount { get; set; }
    public bool IsAvailable { get; set; }
    public bool JustUpdated { get; set; }
    public List<int> BeadItems { get; set; } = new(); // For visual bead rendering
}

public class MatchboxInfo
{
    public string PositionHash { get; set; } = "";
    public bool IsCurrentPosition { get; set; }
    public bool JustUpdated { get; set; }
    public ObservableCollection<MoveWeight> Moves { get; set; } = new();
}

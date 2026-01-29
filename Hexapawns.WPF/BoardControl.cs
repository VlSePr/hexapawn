using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Hexapawns.Core;

namespace Hexapawns.WPF;

public class BoardControl : Canvas
{
    private const int CellSize = 100;
    private const int BoardSize = 3;
    private readonly Rectangle[,] _cells = new Rectangle[BoardSize, BoardSize];
    private readonly Ellipse[,] _pieces = new Ellipse[BoardSize, BoardSize];
    private Position? _selectedPosition;
    private List<Move> _validMoves = new List<Move>();
    private readonly List<Rectangle> _highlightRectangles = new List<Rectangle>();

    public static readonly DependencyProperty GameStateProperty =
        DependencyProperty.Register(nameof(GameState), typeof(GameState), typeof(BoardControl),
            new PropertyMetadata(null, OnGameStateChanged));

    public GameState? GameState
    {
        get => (GameState?)GetValue(GameStateProperty);
        set => SetValue(GameStateProperty, value);
    }

    public event Action<Move>? MoveRequested;

    public BoardControl()
    {
        Width = CellSize * BoardSize;
        Height = CellSize * BoardSize;
        InitializeBoard();
        MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    private void InitializeBoard()
    {
        // Create board cells
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                var cell = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Fill = (row + col) % 2 == 0 ? Brushes.WhiteSmoke : Brushes.LightGray
                };

                Canvas.SetLeft(cell, col * CellSize);
                Canvas.SetTop(cell, row * CellSize);
                Children.Add(cell);
                _cells[row, col] = cell;

                // Create piece placeholder
                var piece = new Ellipse
                {
                    Width = CellSize * 0.7,
                    Height = CellSize * 0.7,
                    Visibility = Visibility.Collapsed
                };

                Canvas.SetLeft(piece, col * CellSize + CellSize * 0.15);
                Canvas.SetTop(piece, row * CellSize + CellSize * 0.15);
                Children.Add(piece);
                _pieces[row, col] = piece;
            }
        }
    }

    private static void OnGameStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BoardControl control && e.NewValue is GameState state)
        {
            control.UpdateBoard(state);
        }
    }

    private void UpdateBoard(GameState state)
    {
        _selectedPosition = null;
        _validMoves.Clear();
        ClearHighlights();

        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                var piece = state.GetPiece(row, col);
                var ellipse = _pieces[row, col];

                if (piece == Player.None)
                {
                    ellipse.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ellipse.Visibility = Visibility.Visible;
                    
                    if (piece == Player.White)
                    {
                        ellipse.Fill = new RadialGradientBrush(
                            Colors.White,
                            Colors.LightBlue
                        );
                        ellipse.Stroke = Brushes.DodgerBlue;
                        ellipse.StrokeThickness = 3;
                    }
                    else // Black
                    {
                        ellipse.Fill = new RadialGradientBrush(
                            Colors.DarkRed,
                            Colors.Black
                        );
                        ellipse.Stroke = Brushes.Red;
                        ellipse.StrokeThickness = 3;
                    }
                }
            }
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (GameState == null || GameState.IsGameOver) return;

        var point = e.GetPosition(this);
        int col = (int)(point.X / CellSize);
        int row = (int)(point.Y / CellSize);

        if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize) return;

        var clickedPos = new Position(row, col);

        // If a piece is already selected, try to move it
        if (_selectedPosition.HasValue)
        {
            var move = _validMoves.FirstOrDefault(m => m.To == clickedPos);
            if (move != default(Move))
            {
                MoveRequested?.Invoke(move);
                _selectedPosition = null;
                _validMoves.Clear();
                ClearHighlights();
                return;
            }
        }

        // Select a new piece if it belongs to the current player
        var piece = GameState.GetPiece(clickedPos);
        if (piece == GameState.CurrentPlayer)
        {
            _selectedPosition = clickedPos;
            _validMoves = GameState.GetLegalMoves()
                .Where(m => m.From == clickedPos)
                .ToList();
            
            HighlightSelectedPiece(clickedPos);
            HighlightValidMoves(_validMoves);
        }
        else
        {
            _selectedPosition = null;
            _validMoves.Clear();
            ClearHighlights();
        }
    }

    private void HighlightSelectedPiece(Position pos)
    {
        var highlight = new Rectangle
        {
            Width = CellSize,
            Height = CellSize,
            Stroke = Brushes.Yellow,
            StrokeThickness = 4,
            Fill = Brushes.Transparent
        };
        Canvas.SetLeft(highlight, pos.Column * CellSize);
        Canvas.SetTop(highlight, pos.Row * CellSize);
        Children.Add(highlight);
        _highlightRectangles.Add(highlight);
    }

    private void HighlightValidMoves(List<Move> moves)
    {
        foreach (var move in moves)
        {
            var highlight = new Rectangle
            {
                Width = CellSize,
                Height = CellSize,
                Fill = new SolidColorBrush(Color.FromArgb(80, 0, 255, 0)),
                Stroke = Brushes.Green,
                StrokeThickness = 2
            };
            Canvas.SetLeft(highlight, move.To.Column * CellSize);
            Canvas.SetTop(highlight, move.To.Row * CellSize);
            Children.Add(highlight);
            _highlightRectangles.Add(highlight);
        }
    }

    private void ClearHighlights()
    {
        foreach (var rect in _highlightRectangles)
        {
            Children.Remove(rect);
        }
        _highlightRectangles.Clear();
    }
}

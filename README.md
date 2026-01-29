# Hexapawn - MENACE Learning AI

A C# WPF implementation of the classic Hexapawn game with a MENACE (Machine Educable Noughts And Crosses Engine) learning algorithm that learns to play through experience.

## What is Hexapawn?

Hexapawn is a simplified chess-like game played on a 3x3 board with 3 pawns per side:
- **White pawns** start at the bottom row (moving up)
- **Black pawns** start at the top row (moving down)

### Rules:
1. Pawns move one square forward if empty
2. Pawns capture diagonally (like in chess)
3. Win conditions:
   - Reach the opposite end of the board
   - Capture all opponent's pawns
   - Block opponent (no legal moves)

## What is MENACE?

MENACE is a learning algorithm that uses reinforcement learning:
- Each game position has a "matchbox" of possible moves
- Each move starts with equal "beads" (probability weights)
- After each game:
  - **Winning moves**: Get more beads (increased probability)
  - **Losing moves**: Lose beads (decreased probability)
- Over time, the AI learns which moves lead to victory!

## Project Structure

### Hexapawns.Core
Core game logic and AI implementation:
- `GameState.cs`: Board representation and game rules
- `MenaceAI.cs`: MENACE learning algorithm
- `GameManager.cs`: Game orchestration and training
- `Player.cs`, `Position.cs`, `Move.cs`: Basic types

### Hexapawns.WPF
WPF visualization application:
- `MainWindow.xaml`: UI layout
- `MainViewModel.cs`: MVVM pattern view model
- `BoardControl.cs`: Custom game board visualization
- `Converters.cs`: UI data converters

## How to Use

### Running the Application

```bash
cd Hexapawns.WPF
dotnet run
```

Or run the executable directly:
```
Hexapawns.WPF\bin\Debug\net9.0-windows\Hexapawns.WPF.exe
```

### Controls

#### Game Controls
- **New Game**: Start a fresh game
- **AI Move**: Let the current AI make one move
- **Start/Stop Auto-Play**: Continuously play games and watch in real-time

#### Training Controls
- **Number of Games**: Set how many games to train (default: 100)
- **Train AI**: Run multiple games automatically to train both AIs
- **Reset Learning**: Clear all learned data and start fresh

### What to Observe

1. **Game Board**: 
   - White pieces (blue with white center)
   - Black pieces (red with black center)
   - Watch pieces move and capture

2. **Statistics Panel**:
   - Win counts for each AI
   - Win rates (percentage)
   - Total games played

3. **Learning Curve Chart**:
   - Blue line: White AI win rate over time
   - Red line: Black AI win rate over time
   - Watch how the AIs improve as they learn!

### Experiment Ideas

1. **Train from scratch**: 
   - Click "Reset Learning"
   - Click "Train AI" with 500 games
   - Watch the learning curve evolve

2. **Watch live learning**:
   - Click "Start Auto-Play"
   - Observe how strategies change over time

3. **Compare learning speeds**:
   - Try different numbers of training games
   - See when the AIs plateau

## Technical Details

- **Language**: C# / .NET 9.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **MVVM Pattern**: CommunityToolkit.Mvvm
- **Charting**: OxyPlot for visualizations
- **Architecture**: Clean separation of core logic and UI

## Key Features

✅ Complete Hexapawn rules implementation
✅ MENACE reinforcement learning algorithm  
✅ Real-time game visualization
✅ Interactive learning curve charts
✅ Batch training mode
✅ Live statistics tracking
✅ Beautiful modern UI with animations

## Learning Outcomes

This project demonstrates:
- **Reinforcement Learning**: How AI can learn from experience
- **Game Theory**: Strategic gameplay evolution
- **MVVM Architecture**: Clean separation of concerns
- **WPF Custom Controls**: Custom board rendering
- **Data Visualization**: Real-time charting

## Credits

Based on Martin Gardner's Hexapawn game and Donald Michie's MENACE algorithm (1961).

---

**Enjoy watching the AI learn to play!** 🎮🤖

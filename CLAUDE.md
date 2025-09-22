# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Blockbot is a Unity 2D puzzle game built with Unity 2022 LTS. It features a grid-based movement system with puzzle mechanics including blocks, boxing gloves, crossbows, and bombots (bomb robots).

## Unity-Specific Commands

### Building the Project
```bash
# Windows build (from Unity Editor or Unity command line)
Unity.exe -batchmode -quit -projectPath . -buildTarget StandaloneWindows64 -buildWindows64Player build/Blockbot.exe

# Or use Unity Hub to open and build through the Editor
```

### Running the Game
- Open the project in Unity Editor (2022.3 or later)
- Press Play button in Editor to test
- Build starts at Level1 scene (enforced by StartupLoader.cs)

## Architecture and Core Systems

### Game Architecture
The game uses a component-based architecture with several key systems:

1. **Movement System**: Grid-based movement with undo/redo functionality
   - `PlayerController.cs`: Handles player input, movement, and maintains move history stack
   - `MoveUnit`/`MoveRecord` classes in `Global.cs`: Track move history for undo functionality
   - Movement types: Face, Move, Push, Punch, Kill, Burn, Explode

2. **GameObject Controllers**:
   - `BlockController.cs`: Pushable blocks
   - `BoxingGloveController.cs`: Punchable item pickup
   - `CrossbowController.cs`: Projectile shooting mechanics
   - `BombotController.cs`: Bomb robot enemies with explosion mechanics
   - `ExitController.cs`: Level completion trigger

3. **Interface System**:
   - `IPickupable`: Items that can be picked up (e.g., boxing gloves)
   - `IPulsable`: Objects that respond to turn-based pulses
   - `IBurnable`: Objects that can be destroyed by fire
   - `IUndoable`: Objects that support undo operations

### Scene Management
- Levels are stored in `Assets/Scenes/` (Level1-9, SampleScene, Claire)
- Build settings in `ProjectSettings/EditorBuildSettings.asset` define scene order
- `StartupLoader.cs` ensures builds start at Level1
- Scene reload on death: Press 'R' key (handled in PlayerController)

### Input System
- Arrow keys or WASD for movement
- Z for undo
- R to restart level (when dead)
- Space for item use (punch with boxing glove)

## Key Implementation Details

- **Grid System**: Uses Vector3 positions with unit movements (1 unit = 1 grid cell)
- **Layers**: Uses LayerMask `whatStopsMovement` for collision detection
- **Turn-based Logic**: Enemies move after player actions via pulse system
- **Undo System**: Maintains a stack of `MoveRecord` objects to reverse player and affected enemy moves

## Development Notes

When modifying gameplay:
- Test undo functionality after any movement changes
- Ensure new GameObjects implement appropriate interfaces (IPickupable, IBurnable, etc.)
- Add new levels to EditorBuildSettings for inclusion in builds
- Maintain grid-based positioning (integer positions)
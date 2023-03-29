### Recreation of the board game Tak in Unity3D.

### Demo video: https://youtu.be/-y6P3AOcgVI

# Code guide
## Overview
All of my code can be found in Assets/Scripts/.

4 Key Files
* Tak.cs
  * The pure logic of the game. Holds the board state and has 5 public functions for updating and retrieving information about the game state.
* UI.cs
  * The UI implementation for game actions. Generates the board and carries out moves in the game engine.
* GameManager.cs
  * Combines the Tak and UI classes to be the central control module for the game.
* PlayerControl.cs
  * The "player controller" script that allows the player to navigate the board with ease and translates user input into game actions.

4 Support Files
* Types.cs
  * Contains all of the lightweight classes, structures, and enums for implementing game logic.
* PieceUI.cs
  * Script attached to each piece that governs its behavior and hold characteristics.
* Utils.cs
  * Static class that provides common functions used throughout the project.
* Settings.cs 
  * Static class that holds configurable settings with variables that can be fine-tuned to the user's liking.

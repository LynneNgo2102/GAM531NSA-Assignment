----------------------------------------------------------------------------
Name: Lynne Ngo
Student ID: 123456789
Date: October 31, 2025
Section: NSA
----------------------------------------------------------------------------

Midterm Game Challenge: Dual-Light Explorer Room

Project Title and Description

This project is an interactive 3D scene built using C# and OpenTK (OpenGL 4.6). It serves as the submission for the "Mini 3D Explorer Game" midterm challenge, demonstrating core 3D graphics concepts. The scene is a single, explorable room that uses two point lights for balanced illumination and features an interactive light switch.


- Feature List (Implemented):

This game demonstrates all required technical features, including the Bonus challenge for multiple lights:

Geometry (3+ Meshes): The environment is built using three distinct mesh types: a Y-axis Quad (for floor/ceiling), a Z-axis 

Quad (for walls), and a Cube (for the pillar and light indicators).

Texturing: Two textures are implemented (wall.png and floor.png) and correctly mapped to the room's geometry.

Lighting (Phong): Implements the Phong lighting model (Ambient, Diffuse, Specular) with calculated contributions from two separate point light sources.

Multiple Lights (Bonus): Two point lights (light1 and light2) are positioned diagonally across the room to provide uniform illumination and eliminate dark spots.

Camera Control: A fully functional first-person camera with decoupled keyboard movement and mouse look.

Basic Interaction: Pressing the E key toggles both main room lights ON/OFF.

Code Structure: Modular and organized codebase with dedicated classes for Camera, Shader, Mesh, and Texture.

-Gameplay Instructions
The camera controls are designed to mimic standard first-person games, with mouse look active only when the Right Mouse Button (RMB) is held.

Key/Mouse
Action W/A/S/D to Move forward, left, backward, and right through the room.

Hold RMB Look Around: Activates mouse look (locks the cursor).

Release RMB: Frees the mouse cursor.
Press E to TOGGLE LIGHT: Turns both point lights ON or OFF.
Tab to Toggles the mouse cursor between Grabbed (locked) and Normal (released).
Esc to Quit the game.

- How to Build and Run the Project:
Dependencies
Framework: .NET 6 SDK (or higher) is required.
OS: Windows, macOS, or Linux (requires the appropriate OpenTK native libraries).

NuGet Packages
The project relies on the following OpenTK libraries, which should be restored automatically:
OpenTK.Windowing.DesktopOpenTK.
Graphics.OpenGL4
OpenTK.Mathematics
System.Drawing.Common (Used for texture loading)

Building and Running
Via Visual Studio: Open the .sln file and build/run the project directly.

Via .NET CLI: Navigate to the root directory containing the project folder and execute:
Bashdotnet run --project MidtermGame/MidtermGame.csproj

CreditsFramework: 
OpenTK (C# wrapper for OpenGL).
Textures: The included texture files (wall.png, floor.png) are from https://www.poliigon.com/ free texture.
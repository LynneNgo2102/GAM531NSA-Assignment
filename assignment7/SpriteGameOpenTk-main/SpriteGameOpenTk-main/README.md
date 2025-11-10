# Character Movement & State Machine Additions

What new movements I implemented:

This project extends the original character controller by adding several new movement features, including a running state, jumping, and an idle animation that transitions smoothly when the player is not moving. The character can walk normally 
using directional input, sprint when the run shift key is held, and automatically switch to an idle stance when no input is detected. Jumping was added with basic gravity logic to give the player vertical mobility and more dynamic interaction with the environmen by hit space.

How your state machine works:

The movement logic is managed by a simple state machine that switches between **Idle**, **Walk**, **Run**, and **Jump** states. Each state controls which animation should play and how the player should respond to input. Transitions occur based on input conditions—such as velocity, key presses, or grounded checks. When these conditions change, the state machine updates accordingly, ensuring smooth and responsive animation changes. Walk by the right and left arrow. Run by hold shift and walk and Jump is space or up arrow.

Any challenges faced and how you solved them:

One of the main challenges was ensuring the idle animation played correctly. Initially, the orthographic projection function caused issues due to API differences inside OpenTK, resulting in unexpected behavior. This was resolved by matching the sample code provided and removing named parameters that were incompatible across versions. After applying this fix, the idle animation displayed as intended and all states transitioned smoothly.

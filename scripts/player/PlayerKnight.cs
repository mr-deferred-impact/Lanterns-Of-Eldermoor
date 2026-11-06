using Godot;
using System;

public partial class PlayerKnight : CharacterBody3D {
    [Export] public float speed = 6.5f;
    [Export] private float mouseSensitivity = .25f;
    [Export] private float cameraYTopLimit = 5f;
    [Export] private float cameraYBottomLimit = -60f;

    public PlayerKnightStateHandler.StateMachine stateMachine = new();

    #region DeclareNode
    private SpringArm3D springArm3D;
    #endregion

    public override void _Ready() {
        InitializeNodes();
        SetupStateMachine();
    }

    public override void _UnhandledInput(InputEvent @event) {
        bool mouseCaptured = (
            @event is InputEventMouseButton mouseButtonEvent &&
            mouseButtonEvent.ButtonIndex == MouseButton.Left &&
            mouseButtonEvent.IsPressed() &&
            Input.MouseMode == Input.MouseModeEnum.Visible
        );

        bool mouseReleased = (
            @event.IsActionPressed("ui_cancel") &&
            Input.MouseMode == Input.MouseModeEnum.Captured
        );

        bool mouseMoved = (
            Input.MouseMode == Input.MouseModeEnum.Captured &&
            @event is InputEventMouseMotion
        );

        if (mouseCaptured) {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
        else if (mouseReleased) {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        if (mouseMoved) {
            Vector3 cameraVector = springArm3D.RotationDegrees;
            InputEventMouseMotion mouseMotionEvent = (InputEventMouseMotion)@event;

            cameraVector.X -= mouseSensitivity * mouseMotionEvent.ScreenRelative.Y;
            cameraVector.Y -= mouseSensitivity * mouseMotionEvent.ScreenRelative.X;

            cameraVector.X = Mathf.Clamp(cameraVector.X, cameraYBottomLimit, cameraYTopLimit);

            springArm3D.RotationDegrees = cameraVector;
        }
    }

    private void SetupStateMachine() {
        PlayerKnightStateHandler.StateIdle idle = new(this);
        PlayerKnightStateHandler.StateWalk walk = new(this);

        AddChild(stateMachine);

        stateMachine.Name = "StateMachine";

        stateMachine.transitions = new() {
            { idle, new() { { PlayerKnightStateHandler.Event.Walk, walk }, } },
            { walk, new() { { PlayerKnightStateHandler.Event.Idle, idle }, } },
        };

        stateMachine.Activate(idle);
    }

    private void InitializeNodes() {
        springArm3D = GetNode<SpringArm3D>("%SpringArm3D");
    }

    public Vector2 GetInputVectorNormalized() {
        return Input.GetVector("move_left", "move_right", "move_up", "move_down").Rotated(-springArm3D.Rotation.Y);
    }
}

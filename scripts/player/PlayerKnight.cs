using Godot;
using Godot.Collections;
using System;

public partial class PlayerKnight : CharacterBody3D {
    public event EventHandler OneOffStateFinished;

    public const float NORMAL_SPEED = 6.5f;
    public const float SPRINT_SPEED = 15.5f;

    [Export] public float speed = NORMAL_SPEED;
    [Export] private float mouseSensitivity = .25f;
    [Export] private float cameraYTopLimit = 5f;
    [Export] private float cameraYBottomLimit = -60f;
    [Export] private PlayerCharacter playerCharacter;


    [Export] private Dictionary<PlayerInventorySlot.Item, NodePath> itemMap;

    public PlayerStateHandler.StateMachine stateMachine = new();

    private PlayerInventorySlot.Item playerItem = PlayerInventorySlot.Item.Lantern;

    #region DeclareNodes
    private SpringArm3D springArm3D;
    private GridContainer playerInventory;
    #endregion

    public override void _Ready() {
        InitializeNodes();
        SetupStateMachine();

        foreach (PlayerInventorySlot inventorySlot in playerInventory.GetChildren()) {
            inventorySlot.OnItemSelected += PlayerInventorySlot_OnItemSelected;
        }

        playerCharacter.OneOffStateAnimationFinished += PlayerCharacter_OneOffStateAnimationFinished;
    }

    public override void _UnhandledInput(InputEvent @event) {
        bool mouseReleased = (
            @event.IsActionPressed("ui_cancel") &&
            Input.MouseMode == Input.MouseModeEnum.Captured
        );

        bool mouseMoved = (
            Input.MouseMode == Input.MouseModeEnum.Captured &&
            @event is InputEventMouseMotion
        );

        if (GlobalData.IsMouseClicked(@event) && Input.MouseMode == Input.MouseModeEnum.Visible) {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GetViewport().SetInputAsHandled();
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
        PlayerStateHandler.StateIdle idle = new(this);
        PlayerStateHandler.StateWalk walk = new(this);
        PlayerStateHandler.StateAttack attack = new(this);

        AddChild(stateMachine);
        MoveChild(stateMachine, 0);

        stateMachine.Name = "StateMachine";

        stateMachine.transitions = new() {
            { idle, new() { { PlayerStateHandler.Event.Walk, walk }, { PlayerStateHandler.Event.Attack, attack }, } },
            { walk, new() { { PlayerStateHandler.Event.Idle, idle }, { PlayerStateHandler.Event.Attack, attack }, } },
            { attack, new() { { PlayerStateHandler.Event.Finish, idle } } },
        };

        stateMachine.Activate(idle);
    }

    private void InitializeNodes() {
        springArm3D = GetNode<SpringArm3D>("%SpringArm3D");
        playerInventory = GetNode<GridContainer>("%PlayerInventory");
    }

    public Vector2 GetInputVectorNormalized() {
        return Input.GetVector("move_left", "move_right", "move_up", "move_down").Rotated(-springArm3D.Rotation.Y);
    }

    private void PlayerInventorySlot_OnItemSelected(object sender, PlayerInventorySlot.OnItemSelectedEventArgs e) {
        foreach (PlayerInventorySlot.Item item in itemMap.Keys) {
            if (item == e.item) {
                ((Node3D)GetNode(itemMap[item])).Show();
            }
            else {
                ((Node3D)GetNode(itemMap[item])).Hide();
            }
        }
    }

    public bool GetAttack() {
        return playerItem == PlayerInventorySlot.Item.Sword;
    }

    private void PlayerCharacter_OneOffStateAnimationFinished(object sender, EventArgs e) {
        OneOffStateFinished?.Invoke(this, EventArgs.Empty);
    }
}

using Godot;
using Godot.Collections;

public partial class PlayerKnight : CharacterBody3D {
    [Export] public float speed = 6.5f;
    [Export] private float mouseSensitivity = .25f;
    [Export] private float cameraYTopLimit = 5f;
    [Export] private float cameraYBottomLimit = -60f;

    [Export] private Dictionary<PlayerInventorySlot.Item, NodePath> itemMap;

    public PlayerStateHandler.StateMachine stateMachine = new();

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
        const float MELEE_HORIZONTAL_ATTACK_DURATION = 1f;

        PlayerStateHandler.StateIdle idle = new(this);
        PlayerStateHandler.StateWalk walk = new(this);
        PlayerStateHandler.StateAttack attack = new(this, MELEE_HORIZONTAL_ATTACK_DURATION);

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
}

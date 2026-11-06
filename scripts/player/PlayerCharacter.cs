using Godot;

public partial class PlayerCharacter : Node3D {
    private const string IDLE = "general/Idle_B";
    private const string WALK = "movement_basic/Walking_B";

    [Export] private PlayerKnight player;
    [Export] private float rotationSpeed = 7.5f;

    private AnimationPlayer animationPlayer;

    public override void _Ready() {
        animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");

        player.stateMachine.OnEventEmitted += PlayerStateMachine_OnEventEmitted;
    }

    private void PlayerStateMachine_OnEventEmitted(object sender, PlayerKnightStateHandler.StateMachine.OnEventEmitteEdventArgs e) {
        switch (e.@event) {
            case PlayerKnightStateHandler.Event.Idle:
                animationPlayer.Play(IDLE);
                break;

            case PlayerKnightStateHandler.Event.Walk:
                animationPlayer.Play(WALK);
                break;
        }
    }

    public override void _PhysicsProcess(double delta) {
        Vector2 inputVector = player.GetInputVectorNormalized();
        float fDelta = (float)delta;

        if (!Mathf.IsZeroApprox(inputVector.Length())) {
            Vector3 direction = new(inputVector.X, 0f, inputVector.Y);
            Vector3 targetRotationVector = Rotation;
            float targetRotation = Vector3.Back.SignedAngleTo(direction, Vector3.Up);

            targetRotationVector.Y = Mathf.LerpAngle(targetRotationVector.Y, targetRotation, rotationSpeed * fDelta);
            Rotation = targetRotationVector;
        }
    }
}

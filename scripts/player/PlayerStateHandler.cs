using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class PlayerStateHandler : Resource {

    public enum Event {
        Attack,
        Finish,
        Idle,
        Interact,
        None,
        Sprint,
        Walk,
    }

    public abstract partial class State : RefCounted {
        public event EventHandler OnFinished;

        protected PlayerKnight player;

        public State(PlayerKnight player) {
            this.player = player;
        }

        public virtual void Enter() {
        }

        public virtual Event Update(float delta) {
            return Event.None;
        }

        public virtual Event UnhandledInput(InputEvent @event) {
            return Event.None;
        }

        public virtual void Exit() {
        }

        protected void EmitOnFinished() {
            OnFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    public partial class StateMachine : Node {
        public event EventHandler<OnEventEmitteEdventArgs> OnEventEmitted;

        public class OnEventEmitteEdventArgs : EventArgs {
            public Event @event;
        }

        public State currentState;
        public Dictionary<State, Dictionary<Event, State>> transitions;

        public override void _Ready() {
            SetPhysicsProcess(false);
            SetProcessUnhandledInput(false);
        }

        public void Activate(State initialState = null) {
            if (initialState != null) {
                currentState = initialState;
            }

            Debug.Assert(currentState != null, "Cannot activate the current state from a null state.");

            currentState.OnFinished += OnCurrentState_Finished;
            currentState.Enter();
            SetPhysicsProcess(true);
            SetProcessUnhandledInput(true);
        }

        public override void _UnhandledInput(InputEvent @event) {
            Event currentEvent = currentState.UnhandledInput(@event);

            if (currentEvent != Event.None) {
                Transition(transitions[currentState][currentEvent]);
                OnEventEmitted?.Invoke(this, new OnEventEmitteEdventArgs { @event = currentEvent });
            }
        }

        public override void _PhysicsProcess(double delta) {
            Event @event = currentState.Update((float)delta);

            if (@event != Event.None) {
                Transition(transitions[currentState][@event]);
                OnEventEmitted?.Invoke(this, new OnEventEmitteEdventArgs { @event = @event });
            }
        }

        private void Transition(State newState) {
            currentState.Exit();
            currentState.OnFinished -= OnCurrentState_Finished;

            currentState = newState;

            currentState.OnFinished += OnCurrentState_Finished;
            currentState.Enter();
        }

        private void OnCurrentState_Finished(object sender, EventArgs e) {
            Transition(transitions[currentState][Event.Finish]);
        }
    }

    public partial class StateIdle : State {
        public StateIdle(PlayerKnight player) : base(player) { }

        public override Event UnhandledInput(InputEvent @event) {
            if (GlobalData.IsMouseClicked(@event) && Input.MouseMode == Input.MouseModeEnum.Captured) {
                return Event.Attack;
            }

            return Event.None;
        }

        public override Event Update(float delta) {
            Vector2 inputVector = player.GetInputVectorNormalized();

            if (inputVector.Length() > 0f) {
                return Event.Walk;
            }

            return Event.None;
        }
    }

    public partial class StateWalk : State {
        public StateWalk(PlayerKnight player) : base(player) { }

        public override void Enter() {
            player.speed = PlayerKnight.NORMAL_SPEED;
        }

        public override Event UnhandledInput(InputEvent @event) {
            if (GlobalData.IsMouseClicked(@event) && Input.MouseMode == Input.MouseModeEnum.Captured) {
                return Event.Attack;
            }

            return Event.None;
        }

        public override Event Update(float delta) {
            Vector2 inputVector = player.GetInputVectorNormalized();

            if (Mathf.IsZeroApprox(inputVector.Length())) {
                return Event.Idle;
            }

            Vector3 direction = new(inputVector.X, 0f, inputVector.Y);

            player.Velocity = direction * player.speed;

            player.MoveAndSlide();

            return Event.None;
        }
    }

    public partial class StateAttack : State {
        public StateAttack(PlayerKnight player) : base(player) { }

        public override void Enter() {
            player.OneOffStateFinished += (object sender, EventArgs e) => {
                EmitOnFinished();
            };
        }
    }

    public partial class StateSprint : StateWalk {
        public StateSprint(PlayerKnight player) : base(player) { }

        public override void Enter() {
            player.speed = PlayerKnight.SPRINT_SPEED;
        }

        public override Event UnhandledInput(InputEvent @event) {
            return base.UnhandledInput(@event);
        }

        public override Event Update(float delta) {
            return base.Update(delta);
        }
    }
}

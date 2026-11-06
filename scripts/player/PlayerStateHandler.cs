using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;


public partial class PlayerKnightStateHandler : Resource {

    public enum Event {
        Finish,
        Idle,
        Interact,
        None,
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

        public virtual void Exit() {
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
        }

        public void Activate(State initialState = null) {
            if (initialState != null) {
                currentState = initialState;
            }

            Debug.Assert(currentState != null, "Cannot activate the current state from a null state.");

            currentState.OnFinished += OnCurrentState_Finished;
            currentState.Enter();
            SetPhysicsProcess(true);
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
}

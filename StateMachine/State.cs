using System;

namespace UnityDemo
{
    public abstract class State<T> where T : Enum
    {
        public T StateKey { get; protected set; }

        public T Transitions { get; protected set; }

        public State(T state, T transitions)
        {
            StateKey = state;
            Transitions = transitions;
        }

        public abstract void Enter();
        public abstract void CheckTransition();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void LateUpdateState();
        public abstract void Exit();

        public bool IsValidTransition(T state) => Transitions.HasFlag(state);
    }
}

using System;
using System.Collections.Generic;

namespace UnityDemo
{
    /// <summary>
    /// Finite state machine.
    /// </summary>
    /// <typeparam name="T"> State enum type. </typeparam>
    public class FiniteStateMachine<T> where T : Enum
    {
        private State<T> _currentState = null;
        private Dictionary<T, State<T>> _states;

        public State<T> CurrentState => _currentState;

        #region Public Methods

        /// <summary>
        /// Creates a new finite state machine.
        /// </summary>
        /// <param name="states"> States to initialize this state machine with. </param>
        /// <param name="initialState"> The initial state of this state machine. </param>
        /// <exception cref="ArgumentException"> If no states are provided. </exception>
        /// <exception cref="ArgumentNullException"> If the initial state is null. </exception>
        public FiniteStateMachine(State<T>[] states, T initialState)
        {
            if (states.Length == 0)
            {
                throw new ArgumentException("[FiniteStateMachine]: Must have at least 1 state!");
            }

            if (initialState == null)
            {
                throw new ArgumentNullException("[PushdownAutomaton]; Initial state cannot be null!");
            }

            _states = CreateStateLookUpTable(states);
            _currentState = GetState(initialState);
            _currentState.Enter();
        }

        /// <summary>
        /// Change current state into a new state.
        /// </summary>
        /// <param name="newState"> The state to change into. </param>
        /// <returns> True if transition was successful, false otherwise. </returns>
        /// <exception cref="ArgumentNullException"> If new state is null. </exception>
        /// <exception cref="ArgumentException"> If transition is not allowed. </exception>
        public bool ChangeState(T newState)
        {
            if (newState == null)
            {
                throw new ArgumentNullException("[FiniteStateMachine]: Cannot change state to null!");
            }

            if (_currentState.Equals(newState)) return false;

            if (!_currentState.IsValidTransition(newState))
            {
                throw new ArgumentException($"[FiniteStateMachine]: Transition from {_currentState} to {newState} is not allowed!");
            }

            _currentState.Exit();
            _currentState = GetState(newState);
            _currentState.Enter();
            return true;
        }

        /// <summary>
        /// Get a state class by key.
        /// </summary>
        /// <param name="state"> The state to get. </param>
        /// <returns> State class. </returns>
        public State<T> GetState(T state) => _states[state];

        #endregion

        #region State Events

        public void CheckTransition()
        {
            _currentState.CheckTransition();
        }

        public void UpdateState()
        {
            _currentState.UpdateState();
        }

        public void FixedUpdateState()
        {
            _currentState.FixedUpdateState();
        }

        public void LateUpdateState()
        {
            _currentState.LateUpdateState();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a lookup table for the states of this state machine.
        /// </summary>
        /// <param name="states"> States this state machine has. </param>
        /// <returns> A dictionary of state keys and state classes. </returns>
        /// <exception cref="ArgumentException"> If state array contains the same state more than once. </exception>
        private Dictionary<T, State<T>> CreateStateLookUpTable(State<T>[] states)
        {
            Dictionary<T, State<T>> stateLookUpTable = new Dictionary<T, State<T>>();

            for (int i = 0; i < states.Length; i++)
            {
                T key = states[i].StateKey;
                State<T> state = states[i];
                if (stateLookUpTable.ContainsKey(key))
                {
                    throw new ArgumentException("[FiniteStateMachine]: Duplicate states are not allowed!");
                }
                stateLookUpTable.Add(key, state);
            }

            return stateLookUpTable;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityDemo
{
    /// <summary>
    /// Pushdown automaton. Uses a stack to keep track of state history.
    /// </summary>
    /// <typeparam name="T" State enum type. </typeparam>
    public class PushdownAutomaton<T> where T : Enum
    {
        private Stack<T> _stack;
        private State<T> _currentState = null;
        private Dictionary<T, State<T>> _states;

        public T CurrentState => _currentState.StateKey;

        #region Public Methods

        /// <summary>
        /// Creates a new pushdown automaton.
        /// </summary>
        /// <param name="states"> States to create this pushdown automaton with. </param>
        /// <param name="initialState"> The initial state of this pushdown automaton. </param>
        /// <exception cref="ArgumentException"> If no states are provided. </exception>
        /// <exception cref="ArgumentNullException"> If initialState is null. </exception>
        public PushdownAutomaton(State<T>[] states, T initialState)
        {
            if (states.Length == 0)
            {
                throw new ArgumentException("[PushdownAutomaton]: Must have at least 1 state!");
            }

            if (initialState == null)
            {
                throw new ArgumentNullException("[PushdownAutomaton]; Initial state cannot be null!");
            }

            _stack = new Stack<T>();
            _stack.Push(initialState);
            _states = CreateStateLookUpTable(states);
            _currentState = GetState(initialState);
        }

        /// <summary>
        /// Peeks at the stack.
        /// </summary>
        /// <returns> The state at the top of the stack. </returns>
        public T Peek() => _stack.Peek();

        /// <summary>
        /// Pushes a new state on top of the stack and changes state to the new state.
        /// </summary>
        /// <param name="state"> State to push to stack and change into. </param>
        /// <returns> True if changing state was successful, false otherwise. </returns>
        public bool Push(T state)
        {
            bool flag = ChangeState(state);
            if (flag) _stack.Push(state);
            return flag;
        }

        /// <summary>
        /// Pops a state from top of the stack or the next valid state.
        /// </summary>
        /// <returns> True if a state transition was successful, false otherwise. </returns>
        /// <exception cref="InvalidOperationException"> If no valid state to transition into was found. </exception>
        public bool Pop()
        {
            if (_stack.Count == 1) return false;

            while (_stack.Count > 1)
            {
                T state = _stack.Pop();
                bool flag = ChangeState(state);
                if (flag) return true;
            }

            if (ChangeState(_stack.Peek())) return true;

            throw new InvalidOperationException("[PushdownAutomaton]: No valid state to pop back to was found!");
        }

        /// <summary>
        /// Empties the stack untill the bottom and changes state to the one at the bottom of the stack.
        /// </summary>
        /// <exception cref="InvalidOperationException"> If no valid state to transition to was found. </exception>
        public void EmptyStackUntillBottom()
        {
            if (_stack.Count == 1) return;

            while (_stack.Count > 1)
            {
                _stack.Pop();
            }
            
            if (ChangeState(_stack.Peek())) return;

            throw new InvalidOperationException("[PushdownAutomaton]: Transition to bottom of stack was not allowed!");
        }

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
        /// Change from current state to a new state.
        /// </summary>
        /// <param name="newState"> State to change into. </param>
        /// <returns> True if transition was successful, false otherwise. </returns>
        /// <exception cref="ArgumentNullException"> If new state is null. </exception>
        /// <exception cref="ArgumentException"> If transition is not allowed. </exception>
        private bool ChangeState(T newState)
        {
            if (newState == null)
            {
                throw new ArgumentNullException("[PushdownAutomaton]: Cannot change state to null!");
            }

            if (newState.Equals(CurrentState)) return false;

            if (!_currentState.IsValidTransition(newState))
            {
                Debug.LogWarning($"[PushdownAutomaton]: Transition from {_currentState} to {newState} is not allowed!");
                return false;
            }

            _currentState.Exit();
            _currentState = GetState(newState);
            _currentState.Enter();
            return true;
        }

        /// <summary>
        /// Get a state class by key.
        /// </summary>
        /// <param name="state"> State to get. </param>
        /// <returns> State class. </returns>
        private State<T> GetState(T state) => _states[state];

        /// <summary>
        /// Create a lookup table for states of this pushdown automaton.
        /// </summary>
        /// <param name="states"> States this pushdown automaton has. </param>
        /// <returns>A dictionary of state keys and state classes. </returns>
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
                    throw new ArgumentException("[PushdownAutomaton]: Duplicate states are not allowed!");
                }
                stateLookUpTable.Add(key, state);
            }

            return stateLookUpTable;
        }

        #endregion
    }
}

using UnityEngine;

namespace CatFish.Enemies
{
	public interface IEnemyControl
	{
		/// <summary>
		/// The event is fired when enemy dies and is returned to pool.
		/// </summary>
		event System.Action<EnemyControl> ReturnToPool;

		/// <summary>
		/// The type of the enemy.	
		/// </summary>
		EnemyType EnemyType { get; set; }

		/// <summary>
		/// Returns and sets the current state.
		/// </summary>
		EnemyStateBase CurrentState { get; set; }

		/// <summary>
		/// Returns and sets the previous state.
		/// </summary>
		EnemyStateBase PreviousState { get; set; }

		/// <summary>
		/// Adds all usable states to a list.
		/// </summary>
		void SetStates();

		/// <summary>
		/// Get enemy state by type.
		/// </summary>
		/// <param name="stateType">The type of the state to get.</param>
		/// <returns>The enemy state matching the given type.
		/// Null if matching state is not found in state list.</returns>
		EnemyStateBase GetEnemyState(EnemyStateType stateType);

		/// <summary>
		/// Change current state to target state. Method is called in each state that
		/// checks which state to transition into.
		/// </summary>
		/// <param name="targetStateType">The state to transition into.</param>
		/// <returns>True if transition was successful. False if not.</returns>
		bool ChangeState(EnemyStateType targetStateType);

		/// <summary>
		/// Triggers ReturnToPool event.
		/// </summary>
		void TriggerReturn();

		/// <summary>
		/// Reset hp to max hp and state to initial state when returning enemy to pool.
		/// </summary>
		void ResetValues();
	}
}

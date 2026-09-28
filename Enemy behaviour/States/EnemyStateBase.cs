using UnityEngine;

namespace CatFish.Enemies
{
	public class EnemyStateBase
	{
		public virtual EnemyStateType EnemyStateType { get; }

		protected EnemyContext _context;
		protected bool _playerInAggroRange = false;
		protected bool _canSeePlayer = false;
		protected float _distanceToPlayer = 0f;
		protected float _dot = 0f;
		protected Collider[] _colliders = new Collider[1];

		protected EnemyStateBase(EnemyContext context)
		{
			_context = context;
		}

		public virtual void OnEnter()
		{
			// Implementation in states
		}

		public virtual void OnExit()
		{
			// Implementation in states
		}

		/// <summary>
		/// Includes player detection logic. Should be called in every state that needs to check for player.
		/// </summary>
		public virtual void OnFixedUpdate()
		{
			if (!_playerInAggroRange)
			{
				FindPlayerInRange();
			}
			else
			{
				LookForPlayer();
			}
			// If player is dead, reset player detection values.
			if (GameManager.Instance.PlayerDead)
			{
				_playerInAggroRange = false;
			}
		}

		/// <summary>
		/// Checks if the enemy can transition to another state.
		/// </summary>
		public virtual void CheckTransition()
		{
			// Implementation in states
		}

		#region Player detection

		/// <summary>
		/// Checks if the player is within the aggro range of the enemy.
		/// </summary>
		public void FindPlayerInRange()
		{
			_colliders = Physics.OverlapSphere(_context.Transform.position, _context.SphereCastRadius,
				 _context.SphereCastLayers);
			if (_colliders.Length > 0)
			{
				_playerInAggroRange = true;
				_context.Player = _colliders[0].gameObject.transform;
				LookForPlayer();
			}
			else
			{
				_playerInAggroRange = false;
				_canSeePlayer = false;
			}
		}

		/// <summary>
		/// Checks if the enemy can see the player based on the field of vision and distance.
		/// </summary>
		public void LookForPlayer()
		{
			Vector3 directionToPlayer = Vector3.Normalize(_context.Player.position - _context.Transform.position);
			_dot = Vector3.Dot(_context.Transform.forward, directionToPlayer);
			_distanceToPlayer = Vector3.Distance(_context.Player.position, _context.Transform.position);

			if (_distanceToPlayer <= _context.VisionDistance)
			{
				_canSeePlayer = _dot >= _context.FieldOfVision;
			}
			else
			{
				_canSeePlayer = false;
				_playerInAggroRange = false;
			}
		}
		#endregion
	}

}
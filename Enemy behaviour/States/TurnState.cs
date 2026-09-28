using UnityEngine;

namespace CatFish.Enemies
{
	public class TurnState : EnemyMovementState
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Turn;

		private float _timer = 0f;
		private float _turnTime = 1f;

		public TurnState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			if (_context.EnemyType == EnemyType.Pig)
			{
				_context.Animator.SetBool(AnimationVariables.TURN, true);
			}
		}

		public override void OnExit()
		{
			_timer = 0f;
			if (_context.EnemyType == EnemyType.Pig)
			{
				_context.Animator.SetBool(AnimationVariables.TURN, false);
			}
			else if (_context.EnemyType == EnemyType.Plant)
			{
				_context.Animator.SetBool(AnimationVariables.TURNLEFT, false);
				_context.Animator.SetBool(AnimationVariables.TURNRIGHT, false);
			}
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
			_timer += Time.deltaTime;
			FaceTowardsPlayer();
			CheckTransition();
		}

		public override void CheckTransition()
		{
			if (_context.EnemyType == EnemyType.Pig)
			{
				if (_timer >= _turnTime)
				{
					if (_playerInAggroRange && _canSeePlayer)
					{
						_context.EnemyControl.ChangeState(EnemyStateType.Charge);
					}
					else if (_playerInAggroRange)
					{
						_context.EnemyControl.ChangeState(EnemyStateType.Aggro);
					}
					else
					{
						_context.EnemyControl.ChangeState(EnemyStateType.Idle);
					}
				}
			}
			else if (_context.EnemyType == EnemyType.Plant)
			{
				if (_playerInAggroRange)
				{
					if (_timer >= _context.MaxTurnTime)
					{
						_context.EnemyControl.ChangeState(EnemyStateType.Exhausted);
					}
					else if (_dot >= _context.TopAttackField && _distanceToPlayer <= _context.TopAttackDistance)
					{
						_context.EnemyControl.ChangeState(EnemyStateType.TopAttack);
					}
					else if (_dot >= _context.SwipeAttackField && _distanceToPlayer <= _context.SwipeAttackDistance)
					{
						_context.EnemyControl.ChangeState(EnemyStateType.SwipeAttack);
					}
					else if (_distanceToPlayer > _context.SwipeAttackDistance)
					{
						_context.EnemyControl.ChangeState(EnemyStateType.Aggro);
					}
				}
				else
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Idle);
				}
			}
		}

		private void FaceTowardsPlayer()
		{
			// If the player is dead, stop turning and return to idle state.
			if (GameManager.Instance.PlayerDead)
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Idle);
				return;
			}

			// Calculate the direction to the player and normalize it.
			Vector3 targetLookDirection = new Vector3(_context.Player.position.x, _context.Transform.position.y,
														_context.Player.position.z)
													- _context.Transform.position;
			targetLookDirection.Normalize();

			// Set the rotation towards the target direction.
			// Use Quaternion.Slerp to smoothly rotate towards the target direction.
			// The rotation speed is limited by the turn speed of the enemy.
			Quaternion targetRotation = Quaternion.LookRotation(targetLookDirection, Vector3.up);
			Quaternion limitedRotation = Quaternion.Slerp(_context.Transform.rotation, targetRotation,
				_context.TurnSpeed * Time.deltaTime);
			_context.Transform.rotation = limitedRotation;

			// If the enemy is a plant, determine the turn direction based on the cross product of the forward vector
			// and the target look direction.
			if (_context.EnemyType == EnemyType.Plant)
			{
				Vector3 cross = Vector3.Cross(_context.Transform.forward, targetLookDirection);

				if (cross.y < 0.2f)
				{
					_context.Animator.SetBool(AnimationVariables.TURNRIGHT, true);
					_context.Animator.SetBool(AnimationVariables.TURNLEFT, false);
				}
				else
				{
					_context.Animator.SetBool(AnimationVariables.TURNLEFT, true);
					_context.Animator.SetBool(AnimationVariables.TURNRIGHT, false);
				}
			}
		}
	}
}
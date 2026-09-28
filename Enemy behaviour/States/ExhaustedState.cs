using UnityEngine;

namespace CatFish.Enemies
{
	public class ExhaustedState : EnemyMovementState
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Exhausted;

		private float _timer = 0f;

		public ExhaustedState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_context.Animator.SetBool(AnimationVariables.EXHAUSTED, true);
		}

		public override void OnExit()
		{
			_timer = 0f;
			_context.Animator.SetBool(AnimationVariables.EXHAUSTED, false);
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
			_timer += Time.deltaTime;
			CheckTransition();
		}

		public override void CheckTransition()
		{
			if (_playerInAggroRange && _timer >= _context.ExhaustedTime)
			{
				if (_canSeePlayer && _context.EnemyType == EnemyType.Pig)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Charge);
				}
				else
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Turn);
				}
			}
			else if (!_playerInAggroRange && _timer >= _context.ExhaustedTime)
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Idle);
			}
		}
	}
}
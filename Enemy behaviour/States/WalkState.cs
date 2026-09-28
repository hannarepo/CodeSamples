using UnityEngine;

namespace CatFish.Enemies
{
	public class WalkState : EnemyMovementState
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Walk;


		public WalkState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_context.Animator.SetBool(AnimationVariables.WALKING, true);
			_speed = _context.WalkSpeed;
			if (_path.Type == WaypointPath.PathType.DelayLoop || _path.Type == WaypointPath.PathType.DelayPingPong)
			{
				_isStopped = false;
			}
		}

		public override void OnExit()
		{
			_context.Animator.SetBool(AnimationVariables.WALKING, false);
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
			CheckTransition();
		}

		public override void CheckTransition()
		{
			if (_canSeePlayer)
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Aggro);
				//_context.AggroEffect.SetActive(true);
			}
			else if (_isStopped)
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Idle);
			}
		}
	}
}

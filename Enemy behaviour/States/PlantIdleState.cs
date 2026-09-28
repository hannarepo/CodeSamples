using UnityEngine;

namespace CatFish.Enemies
{
	public class PlantIdleState : EnemyStateBase
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Idle;

		public PlantIdleState(EnemyContext context) : base(context)
		{
		}

		public override void OnExit()
		{
			_context.Animator.SetTrigger(AnimationVariables.ENDIDLE);
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
			}
		}
	}
}

namespace CatFish.Enemies
{
	public class DeadState : EnemyStateBase
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Dead;

		public DeadState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_context.Animator.SetTrigger(AnimationVariables.DEAD);
		}
	}
}
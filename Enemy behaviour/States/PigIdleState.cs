using UnityEngine;

namespace CatFish.Enemies
{
	public class PigIdleState : EnemyMovementState
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Idle;
		private float _stopTime = 2f;
		private float _timer = 0f;

		public PigIdleState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			base.OnEnter();
			_isStopped = true;
			_context.AudioSource.clip = GameManager.Instance.AudioManager.PigBreathing;
			if (_context.AudioSource.isActiveAndEnabled)
			{
				_context.AudioSource.Play();
			}
		}

		public override void OnExit()
		{
			_isStopped = false;
			_timer = 0f;
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			_timer += Time.deltaTime;
			CheckTransition();
		}

		public override void CheckTransition()
		{
			if (_timer >= _stopTime)
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Walk);
			}
			else if (_canSeePlayer)
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Aggro);
			}
		}
	}
}

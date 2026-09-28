using UnityEngine;

namespace CatFish.Enemies
{
	public class TopAttackState : EnemyStateBase
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.TopAttack;

		private float _timer = 0f;
		private float _attackTime = 2.11f;

		public TopAttackState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_context.Animator.SetBool(AnimationVariables.TOPATTACK, true);

			// Set can deal damage to true for top attack weapons.
			foreach (EnemyWeapon weapon in _context.Weapons)
			{
				if (weapon.Type == EnemyWeapon.WeaponType.Top)
				{
					weapon.CanDealDamage = true;
				}
			}
		}

		public override void OnExit()
		{
			_context.Animator.SetBool(AnimationVariables.TOPATTACK, false);
			_timer = 0f;

			// Set can deal damage to false for top attack weapons.
			foreach (EnemyWeapon weapon in _context.Weapons)
			{
				if (weapon.Type == EnemyWeapon.WeaponType.Top)
				{
					weapon.CanDealDamage = false;
				}
			}
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
			_timer += Time.deltaTime;
			CheckTransition();
		}

		public override void CheckTransition()
		{
			if (_timer > _attackTime)
			{
				if (_dot < _context.SwipeAttackField)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Turn);
				}
				else if (_dot >= _context.SwipeAttackField && _distanceToPlayer > _context.TopAttackDistance)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.SwipeAttack);
				}
				else if (_distanceToPlayer > _context.SwipeAttackDistance && _playerInAggroRange)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Aggro);
				}
				else if (!_playerInAggroRange)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Idle);
				}
				else
				{
					_timer = 0f;
				}
			}
		}
	}
}

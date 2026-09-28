using UnityEngine;

namespace CatFish.Enemies
{
	public class SwipeAttackState : EnemyStateBase
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.SwipeAttack;

		private float _timer = 0f;

		public SwipeAttackState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_context.Animator.SetBool(AnimationVariables.SWIPEATTACK, true);

			// Set can deal damage to true for swipe attack weapons.
			foreach (EnemyWeapon weapon in _context.Weapons)
			{
				if (weapon.Type == EnemyWeapon.WeaponType.Swipe)
				{
					weapon.CanDealDamage = true;
				}
			}
		}

		public override void OnExit()
		{
			_context.Animator.SetBool(AnimationVariables.SWIPEATTACK, false);
			_timer = 0f;

			// Set can deal damage to false for swipe attack weapons.
			foreach (EnemyWeapon weapon in _context.Weapons)
			{
				if (weapon.Type == EnemyWeapon.WeaponType.Swipe)
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
			if (_timer >= _context.SwipeAttackTime)
			{
				if (_dot < _context.SwipeAttackField)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.Turn);
				}
				else if (_dot >= _context.TopAttackField && _distanceToPlayer <= _context.TopAttackDistance)
				{
					_context.EnemyControl.ChangeState(EnemyStateType.TopAttack);
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
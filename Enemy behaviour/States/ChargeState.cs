using Unity.VisualScripting;
using UnityEngine;

namespace CatFish.Enemies
{
	public class ChargeState : EnemyMovementState
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Charge;

		private Vector3 _movementDirection = Vector3.zero;
		private float _timer = 0f;

		public ChargeState(EnemyContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_context.Animator.SetBool(AnimationVariables.CHARGE, true);
			Vector3 targetDirection = new Vector3(_context.Player.position.x, _context.Transform.position.y, _context.Player.position.z);
			_movementDirection = Vector3.Normalize(targetDirection - _context.Transform.position);
			_context.Transform.forward = _movementDirection;
			foreach (EnemyWeapon weapon in _context.Weapons)
			{
				if (weapon.Type == EnemyWeapon.WeaponType.Charge)
				{
					weapon.CanDealDamage = true;
				}
			}
		}

		public override void OnExit()
		{
			_timer = 0f;
			_context.Animator.SetBool(AnimationVariables.CHARGE, false);
			foreach (EnemyWeapon weapon in _context.Weapons)
			{
				if (weapon.Type == EnemyWeapon.WeaponType.Charge)
				{
					weapon.CanDealDamage = false;
				}
			}
		}

		public override void OnFixedUpdate()
		{
			_timer += Time.deltaTime;
			CheckTransition();
		}

		public override void CheckTransition()
		{
			if (_timer < _context.ChargeTime)
			{
				Move(_movementDirection, _context.ChargeSpeed);
			}
			else
			{
				_context.EnemyControl.ChangeState(EnemyStateType.Exhausted);
			}
		}
	}
}

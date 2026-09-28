using UnityEngine;

namespace CatFish.Enemies
{
    public class AggroState : EnemyMovementState
    {
        public override EnemyStateType EnemyStateType => EnemyStateType.Aggro;

        private float _timer = 0f;

        public AggroState(EnemyContext context) : base(context)
        {
        }

        public override void OnEnter()
        {
            _context.Animator.SetBool(AnimationVariables.AGGRO, true);
            _context.AggroEffect.PlayAggroEffect();
        }

        public override void OnExit()
        {
            _context.Animator.SetBool(AnimationVariables.AGGRO, false);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            _timer += Time.fixedDeltaTime;
            CheckTransition();
        }

        public override void CheckTransition()
        {
            if (_context.EnemyType == EnemyType.Pig)
            {
                if (_playerInAggroRange && !_canSeePlayer)
                {
                    _context.EnemyControl.ChangeState(EnemyStateType.Turn);
                }
                else
                {
                    if (_playerInAggroRange && _distanceToPlayer < _context.MaxChargeDistance && _canSeePlayer)
                    {
                        _context.EnemyControl.ChangeState(EnemyStateType.Charge);
                    }
                }
            }
            else if (_context.EnemyType == EnemyType.Plant)
            {
                if (_playerInAggroRange)
                {
                    if (_dot >= _context.TopAttackField && _distanceToPlayer <= _context.TopAttackDistance)
                    {
                        _context.EnemyControl.ChangeState(EnemyStateType.TopAttack);
                    }
                    else if (_dot >= _context.SwipeAttackField && _distanceToPlayer <= _context.SwipeAttackDistance)
                    {
                        _context.EnemyControl.ChangeState(EnemyStateType.SwipeAttack);
                    }
                    else if (_dot < _context.SwipeAttackField)
                    {
                        _context.EnemyControl.ChangeState(EnemyStateType.Turn);
                    }
                }
                else
                {
                    _context.EnemyControl.ChangeState(EnemyStateType.Idle);
                }
            }
        }
    }
}

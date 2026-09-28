using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Serialization;

namespace CatFish.Enemies
{
	public class EnemyControl : MonoBehaviour, IEnemyControl
	{
		[field: SerializeField]
		public EnemyType EnemyType { get; set; }

		private EnemyContext _enemyContext = null;

		// State related
		private List<EnemyStateBase> _states = new List<EnemyStateBase>();
		public EnemyStateBase CurrentState { get; set; }
		public EnemyStateBase PreviousState { get; set; }
		private bool _statesInitialized = false;

		// Pool related
		public event Action<EnemyControl> ReturnToPool;

		// Movement related
		[SerializeField] private WaypointPath.Direction _direction = WaypointPath.Direction.None;
		[SerializeField] private float _walkSpeed = 3f;
		[SerializeField] private float _chargeSpeed = 10f;
		[SerializeField] private float _turnSpeed = 4f;
		[SerializeField] private float _stopInterval = 5f;
		public WaypointPath Path { get; set; }
		
		// Player detection
		[SerializeField] private float _sphereCastRadius = 5f;
		[SerializeField] private LayerMask _sphereCastLayers;
		[SerializeField] private AggroEffect _aggroEffect = null;
		
		// Combat related
		// Common
		[SerializeField] private float _fielOfVision = 0.7f;
		[SerializeField] private float _visionDistance = 14f;
		[SerializeField] private InventoryItem _droppedItem;
		[SerializeField] private EnemyWeapon[] _weapons;
		private EnemyHealth _health;
		// Pig enemy
		[SerializeField] private float _chargeTime = 0.1f;
		[SerializeField] private float _maxChargeDistance = 13f;
		[SerializeField] private float _exhaustedTime = 2f;
		// Plant enemy
		[SerializeField] private float _topAttackDistance = 5f;
		[SerializeField] private float _topAttackField = 0.8f;
		[SerializeField] private float _swipeAttackDistance = 10f;
		[SerializeField] private float _swipeAttackField = 0.6f;
		[SerializeField] private float _swipeAttackTime = 1.5f;
		[SerializeField] private float _maxTurnTime = 0.5f;
		public InventoryItem DroppedItem => _droppedItem;

		#region Unity Messages
		private void Start()
		{
			_health = GetComponent<EnemyHealth>();
			
			_enemyContext = new EnemyContext();

			// Pass all needed references to EnemyContext so that they can be used from states.
			// Controls
			_enemyContext.Animator = GetComponentInChildren<Animator>();
			_enemyContext.Rigidbody = GetComponent<Rigidbody>();
			_enemyContext.EnemyControl = this;
			_enemyContext.EnemyType = EnemyType;
			_enemyContext.Transform = transform;
			// Movement
			_enemyContext.WaypointPath = Path;
			_enemyContext.Direction = _direction;
			_enemyContext.WalkSpeed = _walkSpeed;
			_enemyContext.TurnSpeed = _turnSpeed;
			// Combat
			_enemyContext.ChargeSpeed = _chargeSpeed;
			_enemyContext.ChargeTime = _chargeTime;
			_enemyContext.MaxChargeDistance = _maxChargeDistance;
			_enemyContext.ExhaustedTime = _exhaustedTime;
			_enemyContext.TopAttackDistance = _topAttackDistance;
			_enemyContext.TopAttackField = _topAttackField;
			_enemyContext.SwipeAttackDistance = _swipeAttackDistance;
			_enemyContext.SwipeAttackField = _swipeAttackField;
			_enemyContext.SwipeAttackTime = _swipeAttackTime;
			_enemyContext.MaxTurnTime = _maxTurnTime;
			_enemyContext.AggroEffect = _aggroEffect;
			_enemyContext.Weapons = _weapons;
			// Player detection
			_enemyContext.FieldOfVision = _fielOfVision;
			_enemyContext.VisionDistance = _visionDistance;
			_enemyContext.StopInterval = _stopInterval;
			_enemyContext.SphereCastRadius = _sphereCastRadius;
			_enemyContext.SphereCastLayers = _sphereCastLayers;
			// Other
			_enemyContext.DroppedItem = _droppedItem;
			_enemyContext.AudioSource = GetComponent<AudioSource>();

			SetStates();
		}

		private void FixedUpdate()
		{
			CurrentState.OnFixedUpdate();
		}

		private void OnDrawGizmos()
		{
			Debug.DrawRay(transform.position, transform.forward * _visionDistance, Color.red);
			Gizmos.DrawWireSphere(transform.position, _sphereCastRadius);
		}
		#endregion
		
		#region Initialization
		public void SetStates()
		{
			_statesInitialized = true;
			// Set states for pig enemy
			if (EnemyType == EnemyType.Pig)
			{
				_states.Add(new PigIdleState(_enemyContext));
				_states.Add(new WalkState(_enemyContext));
				_states.Add(new ChargeState(_enemyContext));
				_states.Add(new AggroState(_enemyContext));
				_states.Add(new TurnState(_enemyContext));
				_states.Add(new ExhaustedState(_enemyContext));
				_states.Add(new DeadState(_enemyContext));
			}
			// Set states for plant enemy
			else if (EnemyType == EnemyType.Plant)
			{
				_states.Add(new PlantIdleState(_enemyContext));
				_states.Add(new AggroState(_enemyContext));
				_states.Add(new TopAttackState(_enemyContext));
				_states.Add(new SwipeAttackState(_enemyContext));
				_states.Add(new TurnState(_enemyContext));
				_states.Add(new ExhaustedState(_enemyContext));
				_states.Add(new DeadState(_enemyContext));
			}

			// Set first state in list as current state. Should always be idle state.
			CurrentState = _states[0];
			CurrentState.OnEnter();
		}
		#endregion

		#region Pool related
		public void TriggerReturn()
		{
			if (ReturnToPool != null)
			{
				ReturnToPool.Invoke(this);
				ResetValues();
			}
		}

		public void ResetValues()
		{
			_health.CurrentHP = _health.StartingHP;
			ChangeState(EnemyStateType.Idle);
		}
		#endregion

		#region State related
		public EnemyStateBase GetEnemyState(EnemyStateType stateType)
		{
			foreach (EnemyStateBase state in _states)
			{
				if (state.EnemyStateType == stateType)
				{
					return state;
				}
			}
			return null;
		}

		/// <summary>
		/// Change the current state to the target state.
		/// </summary>
		/// <param name="targetStateType">The type of the target state.</param>
		/// <returns>True if state transition was successful, false if not.</returns>
		public bool ChangeState(EnemyStateType targetStateType)
		{
			// Get the target state by given type.
			EnemyStateBase targetState = GetEnemyState(targetStateType);
			
			if (CurrentState.EnemyStateType == targetStateType)
			{
				Debug.Log($"Current state {CurrentState} is same as target state {targetState}!");
				return false;
			}

			if (targetState == null)
			{
				Debug.Log($"Target state {CurrentState} not found!");
				return false;
			}

			// Store the current state as the previous state before changing the state
			// so that previous state can be referenced if needed.
			PreviousState = CurrentState;
			// Call the current states OnExit so all necessary state end related functionality is performed.
			CurrentState.OnExit();

			// Change the current state to the target state.
			CurrentState = targetState;
			// Call current states OnEnter so all necessary state start related functionality is performed.
			CurrentState.OnEnter();

			return true;
		}
		#endregion
	}
}

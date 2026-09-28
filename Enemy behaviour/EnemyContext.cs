using UnityEngine;

namespace CatFish.Enemies
{
	public class EnemyContext
	{
		// Movement
		public float WalkSpeed { get; set; }
		public float TurnSpeed { get; set; }
		public WaypointPath.Direction Direction { get; set; }
		public float StopInterval { get; set; }
		public Rigidbody Rigidbody { get; set; }
		public Transform Transform { get; set; }
		public WaypointPath WaypointPath { get; set; }
		
		// Combat
		public float ChargeSpeed { get; set; }
		public float ChargeTime { get; set; }
		public float MaxChargeDistance { get; set; }
		public float ExhaustedTime { get; set; }
		public float TopAttackDistance { get; set; }
		public float TopAttackField { get; set; }
		public float SwipeAttackDistance { get; set; }
		public float SwipeAttackField { get; set; }
		public float SwipeAttackTime { get; set; }
		public float MaxTurnTime { get; set; }
		public EnemyWeapon[] Weapons { get; set; }
		
		// Player detection
		public float FieldOfVision { get; set; }
		public float VisionDistance { get; set; }
		public Transform Player { get; set; }
		public AggroEffect AggroEffect { get; set; }
		public float AggroEffectDuration { get; set; }
		public float SphereCastRadius { get; set; }
		public LayerMask SphereCastLayers { get; set; }
		
		// Other
		public Animator Animator { get; set; }
		public EnemyControl EnemyControl { get; set; }
		public EnemyType EnemyType { get; set; }
		public InventoryItem DroppedItem { get; set; }
		public AudioSource AudioSource { get; set; }
	}
}
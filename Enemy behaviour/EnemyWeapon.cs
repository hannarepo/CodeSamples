using UnityEngine;
using CatFish.Player;
using System;
using UnityEngine.Serialization;

namespace CatFish.Enemies
{
	public class EnemyWeapon : MonoBehaviour
	{
		public enum WeaponType
		{
			None = 0,
			Swipe,
			Top,
			Charge
		}

		[SerializeField] private int _damage;
		[SerializeField] private WeaponType weaponType;

		public WeaponType Type => weaponType;
		public bool CanDealDamage { get; set; }

		public static event Action<Vector3> KnockBack = null;

		private void OnTriggerEnter(Collider other)
		{
			// Check if the object is a player and if the weapon can deal damage.
			// If the object is a player, decrease its health and invoke knockback event.
			if (CanDealDamage && other.gameObject.layer == LayerMask.NameToLayer(TagsAndLayers.PLAYER)
				 && other.gameObject.TryGetComponent<PlayerHealth>(out PlayerHealth health))
			{
				if (KnockBack != null)
				{
					KnockBack.Invoke(transform.forward);
				}
				health.DecreaseHealth(_damage);
			}
		}
	}
}
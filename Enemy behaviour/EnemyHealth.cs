using UnityEngine;
using System.Collections;
using TMPro;

namespace CatFish.Enemies
{
	public class EnemyHealth : MonoBehaviour
	{
		[SerializeField] private int _startHp = 50;
		[SerializeField] private int _maxHp = 50;
		[SerializeField] private GameObject _damageEffectPrefab = null;
		[SerializeField] private float _damageEffectDuration = 1f;
		[SerializeField] private HpBarController _hpBarController;
		[SerializeField] private AudioClip _decreaseHealthSFX;
		[SerializeField] private float _decreaseHealthAnimationDuration = 0.3f;
		private float _deathAnimationTime = 1.5f;
		private AudioSource _audioSource;
		private int _currentHp = 50;
		private EnemyControl _enemyControl = null;

		public int StartingHP { get => _startHp; set => _startHp = value; }
		public int MaxHP { get => _maxHp; set => _maxHp = value; }
		public int CurrentHP { get => _currentHp; set => _currentHp = value; }

		private void Start()
		{
			_currentHp = _startHp;
			_enemyControl = GetComponent<EnemyControl>();
			_hpBarController.UpdateSlider(_currentHp);
			_audioSource = GetComponent<AudioSource>();
			
			AnimationClip[] clips = GetComponentInChildren<Animator>().runtimeAnimatorController.animationClips;

			foreach (AnimationClip clip in clips)
			{
				if (clip.name == "Death")
				{
					_deathAnimationTime = clip.length;
					break;
				}
			}
		}
		
		private void OnTriggerEnter(Collider collision)
		{
			if (collision.gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				StartCoroutine(Die(0f));
			}
		}

		/// <summary>
		/// Decrease the health of the enemy by a specified amount.
		/// If the health reaches zero, the enemy will die and drop an item.
		/// </summary>
		/// <param name="amount">The amount of health to be reduced.</param>
		public void DecreaseHealth(int amount)
		{
			if (_decreaseHealthSFX != null)
			{
				_audioSource.PlayOneShot(_decreaseHealthSFX);
			}

			GameObject damageEffect = null;

			if (_currentHp > amount)
			{
				// If the enemy is in idle or walk state, change to aggro state when taking damage.
				if (_enemyControl.CurrentState.EnemyStateType == EnemyStateType.Idle ||
					_enemyControl.CurrentState.EnemyStateType == EnemyStateType.Walk)
				{
					_enemyControl.ChangeState(EnemyStateType.Aggro);
				}
				
				_currentHp -= amount;
				if (_damageEffectPrefab != null)
				{
					damageEffect = Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity);
					damageEffect.GetComponentInChildren<TextMeshPro>().text = amount.ToString();
				}
			}
			else
			{
				_currentHp = 0;
				_enemyControl.ChangeState(EnemyStateType.Dead);
				gameObject.layer = LayerMask.NameToLayer(TagsAndLayers.IGNORE_COLLISION);
				StartCoroutine(Die(_deathAnimationTime));
			}

			_hpBarController.UpdateSlider(_currentHp);
			Destroy(damageEffect, _damageEffectDuration);
		}

		/// <summary>
		/// Coroutine to handle the death of the enemy.
		/// Triggers the return to the pool and adds items to the inventory.
		/// </summary>
		/// <param name="deathAnimationTime">The time taken for the death animation.</param>
		private IEnumerator Die(float deathAnimationTime)
		{
			deathAnimationTime -= _decreaseHealthAnimationDuration;
			float timeElapsed = 0f;

			while (timeElapsed < deathAnimationTime)
			{
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			_enemyControl.TriggerReturn();
			Inventory.AddItems(_enemyControl.DroppedItem.itemType, 1);
			gameObject.layer = LayerMask.NameToLayer(TagsAndLayers.ENEMY);
		}
	}
}

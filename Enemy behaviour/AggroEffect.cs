using UnityEngine;
using System.Collections;

namespace CatFish.Enemies
{
	public class AggroEffect : MonoBehaviour
	{
		[SerializeField] private float _effectDuration = 0.2f;
		[SerializeField] private GameObject _effect = null;

		public void PlayAggroEffect()
		{
			StartCoroutine(EffectRoutine());
		}
		
		public IEnumerator EffectRoutine()
		{
			_effect.SetActive(true);
			float timeElapsed = 0f;

			while (timeElapsed < _effectDuration)
			{
				timeElapsed += Time.fixedDeltaTime;
				yield return null;
			}
			
			_effect.SetActive(false);
		}
	}
}
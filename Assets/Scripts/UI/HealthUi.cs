using System.Collections;
using Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Health))]
	public class HealthUi : MonoBehaviour
	{
		// Bottom bar
		[Header("Parameters")]
		[Tooltip("Whether to fade in when stat changing then fade out (typically for others bar)")] public bool fade;
		[Tooltip("Time between fade in-out, leave it if not using fade")] public int fadeDuration = 5;

		[Header("Objects references")]
		public GameObject bar;
		public Image fill;
		public TextMeshProUGUI valueText;
		public RectTransform barFill;

		private void Awake()
		{
			GetComponent<Health>().HealthChanged += UpdateUi;
		}

		private void UpdateUi(float value)
		{
			var c = GetComponent<Renderer>().material.color;
			// Gray-ize the block to look less alive
			// TODO: color changing doesn't work
			for (var i = 0; i < 3; i++)
			{
				c[i] = Mathf.Clamp(4.0f * value, 0.2f, 0.8f);
			}
			var sizeY = barFill.sizeDelta.y;
			barFill.sizeDelta = new Vector2(sizeY * value * 10, sizeY);
			fill.fillAmount = value;
			valueText.text = value > 0 ? value.ToString("0%") : "0%";
			if(fade && !bar.activeInHierarchy) StartCoroutine(FadeInFadeOut());
		}

		private IEnumerator FadeInFadeOut()
		{
			bar.SetActive(true);
			yield return new WaitForSeconds(fadeDuration);
			bar.SetActive(false);
		}
	}
}

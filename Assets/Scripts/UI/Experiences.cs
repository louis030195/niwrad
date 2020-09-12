using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Evolution;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class Experiences : MonoBehaviour
    {
        [SerializeField] private GameObject experienceButtonTemplate;
        [SerializeField] private GameObject experienceMenuTemplate;
        [SerializeField] private GameObject experienceButtonsGrid;
        [SerializeField] private GameObject experienceMenus;
        [SerializeField] private CanvasGroup experienceButtonScrollView;


        private readonly List<Menu> _experienceButtons = new List<Menu>();
        private readonly List<Menu> _experienceMenus = new List<Menu>();

        private bool _isFading;
        private void Start()
        {
            var experiences = Resources.LoadAll("ScriptableObjects", typeof(Experience)).Cast<Experience>();
            foreach (var experience in experiences)
            {
                var menu = Instantiate(experienceMenuTemplate, experienceMenus.transform).GetComponent<Menu>();
                _experienceMenus.Add(menu);
                experience.Render(menu.GetComponentInChildren<GridLayoutGroup>()
                    .transform); // TODO: fix put somewhere that work
                var button = Instantiate(experienceButtonTemplate, experienceButtonsGrid.transform)
                    .GetComponent<Button>();
                button.onClick.AddListener(() => menu.Push()); // TODO: check parent
                var buttonMenu = button.GetComponent<Menu>();
                buttonMenu.GetComponentInChildren<TextMeshProUGUI>().text = experience.name;
                buttonMenu.GetComponent<Image>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                buttonMenu.Show();
                _experienceButtons.Add(buttonMenu);
            }
        }

        public async void OnPointerEnter(BaseEventData _)
        {
            // TODO: could extend Menu with a "fade-in" "fade-out" function
            // _experienceButtons.ForEach(b => b.Push());
            experienceButtonScrollView.alpha = 1;
            experienceButtonScrollView.interactable = true;
            experienceButtonScrollView.blocksRaycasts = true;
        }

        public async void OnPointerExit(BaseEventData _)
        {
            await UniTask.Delay(5000, true);
            var elapsedTime = 0.0f;
            while (experienceButtonScrollView.alpha > 0)
            {
                await UniTask.Delay(100);
                experienceButtonScrollView.alpha -= elapsedTime;
                elapsedTime += 0.01f;
            }
            experienceButtonScrollView.interactable = false;
            experienceButtonScrollView.blocksRaycasts = false;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Api.Realtime;
using Api.Session;
using Evolution;
using Google.Protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace UI
{
    public class ExperiencesMenuList : Menu
    {
        [SerializeField] private Transform experienceGrid;
        [SerializeField] private GameObject experienceButtonTemplate;
        [SerializeField] private GameObject experienceMenuTemplate;
        [SerializeField] private Button newExperienceButton;
        private readonly List<ExperienceMenu> _experienceMenus = new List<ExperienceMenu>();
        protected override async void Start()
        {
            base.Start();
            var expDir = $"{Application.persistentDataPath}/Experiences";
            if (!Directory.Exists(expDir)) // TODO: could load on Show() ...
            {
                Debug.Log($"Creating directory {expDir}");
                Directory.CreateDirectory(expDir);
            }
            var experiences = Directory.GetFiles(expDir);
            foreach (var experienceFile in experiences)
            {
                var e  = ExperienceExtensions.Load(experienceFile, true);
                AddExperienceMenu(e);
            }
            newExperienceButton.onClick.AddListener(() =>
            {
                var e = ExperienceExtensions.New();
                AddExperienceMenu(e);
            });
            Sm.instance.ConnectionSucceed += async () =>
            {
                var communityExperiences = await Sm.instance.ExperienceList();
                foreach (var experience in communityExperiences.Objects)
                {
                    var parsedExperience = Experience.Parser.ParseJson(experience.Value);
                    var result = await Sm.instance.Client.GetUsersAsync(Sm.instance.Session, new []{experience.UserId});
                    parsedExperience.Name += $"- By {result.Users.FirstOrDefault()?.Username}";
                    AddExperienceMenu(parsedExperience);
                }
            };
        }

        /// <summary>
        /// Based on an experience, create an experience menu and a button referring to it
        /// </summary>
        /// <param name="e"></param>
        private void AddExperienceMenu(Experience e)
        {
            var goButton = Instantiate(experienceButtonTemplate, experienceGrid);
            goButton.GetComponentInChildren<TextMeshProUGUI>().text = e.Name;
            var expMenu = Instantiate(experienceMenuTemplate, transform.parent).GetComponent<ExperienceMenu>();
            expMenu.Load(e);
            expMenu.Deleted += () =>
            {
                _experienceMenus.Remove(expMenu);
                Destroy(expMenu.gameObject);
                Destroy(goButton);
            };
            expMenu.Added += () => AddExperienceMenu(e);
            _experienceMenus.Add(expMenu);
            goButton.GetComponent<Button>().onClick.AddListener(() => expMenu.Push());
        }
    }
}

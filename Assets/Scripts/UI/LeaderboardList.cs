using Api.Session;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LeaderboardList : Menu
    {
        public GameObject grid;
        public GameObject recordTemplate;
        public override async void Show()
        {
            base.Show();
            var records = await Sm.instance.ListNaiveLeaderboard();
            foreach (var record in records.Records) // TODO: cursor
            {
                var go = Instantiate(recordTemplate, grid.transform);
                var username = go.transform.Find("Username");
                var score = go.transform.Find("Score");
                if (username == null || score == null)
                {
                    Debug.LogError("Invalid leaderboard record template");
                    return;
                }
                var usernameText = username.gameObject.GetComponentInChildren<TextMeshProUGUI>();    
                var scoreText = score.gameObject.GetComponentInChildren<TextMeshProUGUI>();
                if (usernameText == null || scoreText == null)
                {
                    Debug.LogError("Invalid leaderboard record template");
                    return;
                }
                usernameText.text = record.Username;
                scoreText.text = record.MaxNumScore.ToString();
            }
        }
        /**
         * Cleanup
         */
        public override void Hide()
        {
            base.Hide();
            foreach (Transform record in grid.transform)
            {
                Destroy(record.gameObject);
            }
        }
    }
}

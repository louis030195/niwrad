using System.Collections;
using Api.Session;
using Cysharp.Threading.Tasks;
using Evolution;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(Sm),typeof(Hm), typeof(Gm))]
    public class Leaderboard : MonoBehaviour
    {
        private Hm _hostManager;
        private Gm _gameManager;
        private Sm _sessionManager;
        private readonly WaitForSeconds _delay = new WaitForSeconds(10);

        private void Start()
        {
            _hostManager = GetComponent<Hm>();
            _gameManager = GetComponent<Gm>();
            _sessionManager = GetComponent<Sm>();
            StartCoroutine(SendRecords());
        }

        private IEnumerator SendRecords()
        {
            while (true)
            {
                yield return _delay;
                if (_gameManager.state == GameState.Experience)
                {
                    _sessionManager.WriteNaiveLeaderboard(_hostManager.Animals.Count + _hostManager.Plants.Count);
                    // Debug.Log(
                        // $"Sending new leaderboard score {_hostManager.Animals.Count + _hostManager.Plants.Count}");
                }
            }
        }
    }
}

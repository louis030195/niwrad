using System;
using System.Collections;
using System.Collections.Generic;
using Api.Match;
using Api.Realtime;
using Api.Rpc;
using Api.Session;
using Cysharp.Threading.Tasks;
using ProceduralTree;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Initialization : MonoBehaviour
    {
        private static readonly Dictionary<string, string> Envs = new Dictionary<string, string>
        {
            {"INITIAL_REGION", ""},
            {"TERRAIN_SIZE", "100"},
            {"NAKAMA_IP", "127.0.0.1"},
            {"NAKAMA_PORT", "6666"},
            {"WORKER_ID", "unityIDE"},
        };

        [SerializeField] private Terrain map;
        [SerializeField] private GameObject sessionManagerPrefab;
        [SerializeField] private Slider timescaleSlider;
        [SerializeField] private TextMeshProUGUI timescaleText;
    
        private async void Awake()
        {
            // TODO: maybe move whole class to host manager or other ... or change name

            // If there is no session manager it's a host
            if (FindObjectOfType<SessionManager>() == null)
            {
                foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
                {
                    var k = kv.Key as string;
                    var v = kv.Value as string;
                    if (k != null) Envs[k] = v;
                }
                foreach (var environmentVariable in Envs)
                {
                    Debug.Log($"env var:{environmentVariable.Key}:{environmentVariable.Value}");
                }

                // Only server can change timescale
                timescaleSlider.onValueChanged.AddListener(value =>
                {
                    Time.timeScale = value;
                    timescaleText.text = $"{value}";
                });
                Instantiate(sessionManagerPrefab);
                await InitializeNet();
            }
            else
            {
                // Clients can't tweak timescale
                timescaleSlider.gameObject.SetActive(false);
                timescaleText.gameObject.SetActive(false);
            }
            InitializeGameplay();
        }

        private async UniTask InitializeNet()
        {
            Debug.Log($"Trying to connect to nakama at {Envs["NAKAMA_IP"]}:{Envs["NAKAMA_PORT"]}");
            // Server account !
            var (res, msg) = await SessionManager.instance.ConnectAsync("bbbb@bbbb.com",
                "bbbbbbbb",
                ip: Envs["NAKAMA_IP"],
                p: int.Parse(Envs["NAKAMA_PORT"]),
                create: true);
            if (!res)
            {
                Debug.LogError($"Failed to connect to Nakama {msg}");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
            await SessionManager.instance.ConnectSocketAsync();
            // Join match with null id = create
            await MatchCommunicationManager.instance.JoinMatchAsync(workerId: Envs["WORKER_ID"],
                matchConfiguration:
                new MatchConfiguration
                {
                    TerrainSize = int.Parse(Envs["TERRAIN_SIZE"])
                });
            SessionManager.instance.isServer = true;
        }

        private async void InitializeGameplay()
        {
            // Seems to be best to wait a bit before spawning things as there is navmesh baking
            // Camera stuff, opengl thing
            Debug.Log($"Initializing gameplay ...");
            await UniTask.WaitUntil(() => MatchCommunicationManager.instance.seed != -1);
            Random.InitState(MatchCommunicationManager.instance.seed);
            Debug.Log($"Seed loaded value: {MatchCommunicationManager.instance.seed}");

            // Once the seed is loaded, we can generate the map to have a deterministically same map than others
            var diamondSquare = map.GetComponent<DiamondSquareTerrain>();
            Debug.Log($"Generating map and navmesh");
            diamondSquare.ExecuteDiamondSquare(int.Parse(Envs["TERRAIN_SIZE"]));

            // Wait until it's generated and baked
            await UniTask.WaitUntil(() => diamondSquare.navMeshBaked);
            Debug.Log($"Navmesh baked, ready for gameplay");
            // Notifying self and others that we can handle game play
            var msg = new Packet {Initialized = new Initialized()};
            foreach (var instancePlayer in MatchCommunicationManager.instance.players)
            {
                msg.Recipients.Add(instancePlayer.UserId);
            }
            MatchCommunicationManager.instance.RpcAsync(msg);

            // Start filling the pool
            TreePool.instance.FillSlowly(100);
        }
    }
}

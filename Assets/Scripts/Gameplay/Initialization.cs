using System;
using System.Collections;
using System.Collections.Generic;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using Cysharp.Threading.Tasks;
using ProceduralTree;
using Protometry.Vector3;
using Protometry.Volume;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Initialization : MonoBehaviour
    {
        private static readonly Dictionary<string, string> Envs = new Dictionary<string, string>
        {
            {"NAKAMA_IP", "127.0.0.1"},
            {"NAKAMA_PORT", "6666"},
            {"MATCH_ID", ""},
            {"EMAIL", ""},
            {"PASSWORD", ""},
        };

        [SerializeField] private Terrain map;
        [SerializeField] private GameObject sessionManagerPrefab;
        [SerializeField] private Slider timescaleSlider;
        [SerializeField] private TextMeshProUGUI timescaleText;
    
        private async void Awake()
        {

            // If there is no session manager it's a server
            if (FindObjectOfType<SessionManager>() == null)
            {
                // MatchCommunicationManager.instance
                foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
                {
                    var k = kv.Key as string;
                    var v = kv.Value as string;
                    if (k != null) Envs[k] = v;
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

            await UniTask.WaitUntil(() => MatchCommunicationManager.instance.region != null);
            InitializeGameplay();
        }

        private async UniTask InitializeNet()
        {
            Debug.Log($"Trying to connect to nakama at {Envs["NAKAMA_IP"]}:{Envs["NAKAMA_PORT"]}");
            // Server account !
            var (res, msg) = await SessionManager.instance.ConnectAsync(Envs["EMAIL"],
                Envs["PASSWORD"],
                ip: Envs["NAKAMA_IP"],
                p: int.Parse(Envs["NAKAMA_PORT"]));
            if (!res)
            {
                Debug.LogError($"Failed to connect to Nakama {msg}");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                Application.Quit();
            }
            await SessionManager.instance.ConnectSocketAsync();
            SessionManager.instance.isServer = true;
            await MatchCommunicationManager.instance.JoinMatchAsync(Envs["MATCH_ID"]);
        }

        private void InitializeGameplay()
        {
            Random.InitState((int) MatchCommunicationManager.instance.seed);
            Debug.Log($"Seed loaded value: {MatchCommunicationManager.instance.seed}");
            // map.terrainData.SetHeights(0, 0, info.Map.To2dArray());
            var m = map.GetComponent<NavMeshSurface>();
            Debug.Log($"Generating map and baking it for path finding");
            m.BuildNavMesh();
            var pos = MatchCommunicationManager.instance.region.GetCenter();
            transform.position = pos;
            // Start filling the tree pool
            TreePool.instance.FillSlowly(100);
            if (SessionManager.instance.isServer)
            {
                HostManager.instance.InitializeGameplay();
                return;
            }
            // Client notify everyone that it's ready to handle game-play
            // TODO: wondering if we should send to everyone to have player in state globally for global messages ?
            var msg = new Packet {Initialized = new Initialized()}.Basic(pos.Net());
            MatchCommunicationManager.instance.RpcAsync(msg);
        }
    }
}

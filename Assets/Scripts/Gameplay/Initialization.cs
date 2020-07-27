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

            MatchCommunicationManager.instance.MatchJoined += InitializeGameplay;
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
            SessionManager.instance.isServer = true;
            await MatchCommunicationManager.instance.JoinMatchAsync(Envs["MATCH_ID"]);
        }

        private async void InitializeGameplay(MatchInformation info)
        {
            Random.InitState(info.Seed);
            Debug.Log($"Seed loaded value: {info.Seed}");
            map.terrainData.SetHeights(0, 0, info.Map.To2dArray());
            var m = map.GetComponent<NavMeshBaker>();
            Debug.Log($"Generating map and baking it for path finding");
            m.Bake();
            // Notifying self and others that we can handle game play
            var msg = new Packet {Initialized = new Initialized()}.Basic();
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

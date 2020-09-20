using System;
using System.Collections;
using System.Collections.Generic;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using Cysharp.Threading.Tasks;
using Evolution;
using ProceduralTree;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay
{
    /// <summary>
    /// Gm is GameManager, used to share data across online / offline, handle logic generally
    /// </summary>
    [RequireComponent(typeof(NavMeshSurface))]
    public class Gm : Singleton<Gm>
    {
        public bool online = true;

        /// <summary>
        /// Behaviours are dependent on randomness, we want it deterministic across server and clients
        /// so we can avoid to sync many things
        /// </summary>
        [Tooltip("Leave to 0 for online, it's set by server")] public long seed;

        private static readonly Dictionary<string, string> Envs = new Dictionary<string, string>
        {
            {"NAKAMA_IP", "127.0.0.1"},
            {"NAKAMA_PORT", "6666"},
            {"MATCH_ID", ""},
            {"EMAIL", ""},
            {"PASSWORD", ""},
        };

        [SerializeField, Tooltip("Prefab containing all network managers, not required offline")]
        private GameObject networkManagersPrefab;

        [Header("Map")] 
        [SerializeField, Range(1, 12)]
        private int mapSize = 3;
        [SerializeField, Range(1, 1000)]
        private int mapHeight = 10;
        [SerializeField, Range(0, 2000)]
        private int mapSpread = 1000;
        [SerializeField, Range(0, 1)]
        private float mapSpreadReductionRate = 0.8f;

        public Experience Experience { get; private set; }
        
        protected override async void Awake()
        {
            base.Awake();

            foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
            {
                // Debug.Log($"{kv.Key}={kv.Value}");
                var v = kv.Value as string;
                if (kv.Key is string k) Envs[k] = v;
            }

            // If it's online and there is a MATCH_ID var it's an executor
            var isExecutor = Envs["MATCH_ID"].Equals(string.Empty);
            if (online && isExecutor)
            {
                Instantiate(networkManagersPrefab);
                await InitializeNet();
            }

            // Online shouldn't have set seed, server will send it
            await UniTask.WaitUntil(() => seed != default);
            InitializeGameplay();
        }

        private void Start()
        {
            Mm.instance.EnableHud(false);
        }

        private async UniTask InitializeNet()
        {
            Debug.Log($"Trying to connect to nakama at {Envs["NAKAMA_IP"]}:{Envs["NAKAMA_PORT"]}");
            // Server account !
            var (res, msg) = await Sm.instance.ConnectAsync(Envs["EMAIL"],
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
            await Sm.instance.ConnectSocketAsync();
            Sm.instance.isServer = true;
            await Mcm.instance.JoinMatchAsync(Envs["MATCH_ID"]);
        }

        private void InitializeGameplay()
        {
            // Online mode should receive seed from Nakama for determinism, otherwise 666 :)
            Random.InitState((int)seed);
            Debug.Log($"Seed loaded: {Random.state}");
            if (!online) return;
            
            var pos = Mcm.instance.region.GetCenter();
            transform.position = pos;
            if (Sm.instance.isServer)
            {
                Hm.instance.InitializeNetworkHandlers();
            }
            // Client notify everyone that it's ready to handle game-play
            // TODO: wondering if we should send to everyone to have player in state globally for global messages ?
            var msg = new Packet {Initialized = new Initialized()}.Basic(pos.Net());
            Mcm.instance.RpcAsync(msg);
        }

        public void Pause()
        {
            Time.timeScale = 0;
            Hm.instance.Pause();
        }
        
        public void Play()
        {
            Time.timeScale = 1;
            Hm.instance.Play();
        }
        public void StartExperience(Experience e)
        {
            Time.timeScale = e.General.Timescale;
            var t = ProceduralTerrain.Generate((int) e.Map.Size, 
                    (int) e.Map.Height, 
                    (int)seed, 
                    (float) e.Map.Spread, 
                    (float) e.Map.SpreadReductionRate);
            t.tag = "ground";
            var m = GetComponent<NavMeshSurface>();
            Debug.Log($"Generating map and baking it for path finding");
            m.BuildNavMesh();
            Experience = e;
            Mm.instance.EnableHud(true);
            // TODO: other general stuff
            Hm.instance.StartExperience(e);
            // TODO: generate map based on e.Map.Stuff
        }
    }
}

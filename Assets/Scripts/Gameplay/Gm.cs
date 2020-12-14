using System;
using System.Collections;
using System.Collections.Generic;
using Api.Match;
using Api.Realtime;
using Api.Session;
using Api.Utils;
using Cysharp.Threading.Tasks;
using Evolution;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public enum GameState
    {
        Menu,
        Play,
        Experience
    }
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

        public Experience Experience { get; private set; }

        private GameState _state;
        public GameState State
        {
            get => _state;
            set
            {
                _state = value;
                switch(value)
                {
                    case GameState.Menu:
                        MenuStateStarted?.Invoke();
                        break;
                    case GameState.Play:
                        PlayStateStarted?.Invoke();
                        break;
                    case GameState.Experience:
                        ExperienceStateStarted?.Invoke();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        public event Action MenuStateStarted;
        public event Action PlayStateStarted;
        public event Action ExperienceStateStarted;

        private GameObject _map;
        protected override async void Awake()
        {
            base.Awake();
            foreach (DictionaryEntry kv in Environment.GetEnvironmentVariables())
            {
                // Debug.Log($"{kv.Key}={kv.Value}");
                var v = kv.Value as string;
                if (kv.Key is string k) Envs[k] = v;
            }

            // Online shouldn't have set seed, server will send it
            await UniTask.WaitUntil(() => seed != default);
            InitializeGameplay();
            GameLoop().Forget();
        }

        private void Start()
        {
            NiwradMenu.instance.EnableHud(false);
        }

        private async UniTaskVoid GameLoop()
        {
            InitializeGameplay();
            await UniTask.Yield();
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

        public void Reset()
        {
            Hm.instance.Reset();
        }
        public void StartExperience(Experience e)
        {
            Hm.instance.Reset();
            Time.timeScale = e.General.Timescale;
            Destroy(_map);
            _map = ProceduralTerrain.Generate((int) e.Map.Size, 
                    (int) e.Map.Height, 
                    (int)seed, 
                    (float) e.Map.Spread, 
                    (float) e.Map.SpreadReductionRate);
            _map.tag = "ground";
            Experience = e;
            NiwradMenu.instance.EnableHud(true);
            NiwradMenu.instance.settings.gameObject.SetActive(true); // TODO: shouldn't be here, fix UI again
            var m = GetComponent<NavMeshSurface>();
            Debug.Log($"Generating map and baking it for path finding");
            m.BuildNavMesh(); // TODO: can crash if weird meshes, maybe should try catch here
            // TODO: other general stuff
            Hm.instance.StartExperience(e);
            State = GameState.Experience;
            // TODO: generate map based on e.Map.Stuff
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Utils.Map
{
    [ExecuteInEditMode]
    public class ZoneManager : MonoBehaviour {

        public const string DEFAULT_ZONE_NAME = "Default";

        protected static ZoneManager Instance;
        public List<ZoneTile> ZoneTileList = new List<ZoneTile>();

        public delegate void AddZoneTileDelegate ( ZoneTile zoneTile );
        public AddZoneTileDelegate OnAddZoneTileDelegate;

        public delegate void RemoveZoneTileDelegate ( ZoneTile zoneTile );
        public RemoveZoneTileDelegate OnRemoveZoneTileDelegate;

        public delegate void ActiveZoneChangedDelegate ( ZoneInfo zoneInfo );
        public ActiveZoneChangedDelegate OnActiveZoneChangedDelegate;

        public GameObject player;
        protected Vector3 lastSampledPosition;

        public ZoneInfo activeZoneInfo;
        public float distanceToResample = 5;

        void Awake () {
            // call to init default zone.
            this.onZoneChanged();
        }

        void Update () {
            if ( this.player == null ) {
                this.player = GameObject.FindGameObjectWithTag("player");
            }
            this.CheckResampleActiveZone();
        }

        public void CheckResampleActiveZone () {
            if ( this.player != null ) {
                if ( this.lastSampledPosition == null ) {
                    this.ResampleActiveZone();
                    return;
                }
                float distance = Vector3.Distance(this.player.transform.position, this.lastSampledPosition);
                if ( distance > distanceToResample ) {
                    this.ResampleActiveZone();
                }
            }
        }

        public void ResampleActiveZone () {
            if ( this.player != null ) {
                string currentActiveZoneName = this.activeZoneInfo != null ? this.activeZoneInfo.zoneName : null;
                this.activeZoneInfo = GetActiveZoneInfo(this.player.transform);
                string newActiveZoneName = this.activeZoneInfo != null ? this.activeZoneInfo.zoneName : null;
                this.lastSampledPosition = this.player.transform.position;
                bool zoneChanged = newActiveZoneName != null ? !newActiveZoneName.Equals(currentActiveZoneName) : currentActiveZoneName != null;
                if ( zoneChanged ) {
                    this.onZoneChanged();
                }
            }
        }

        protected void onZoneChanged () {
            OnActiveZoneChanged(this.activeZoneInfo);
            OnActiveZoneChangedDelegate?.Invoke(this.activeZoneInfo);
        }

        public static ZoneManager GetInstance () {
            if ( !Instance ) {
                FindInstance();
            }
            return Instance;
        }


        public static void RegisterZoneTile ( ZoneTile zoneTile ) {
            ZoneManager zoneManager = GetInstance();
            if ( zoneManager != null ) {
                zoneManager.Instance_RegisterZoneTile(zoneTile);
            }
        }

        protected void Instance_RegisterZoneTile ( ZoneTile zoneTile ) {
            if ( !ZoneTileList.Contains(zoneTile) ) {
                ZoneTileList.Add(zoneTile);
                OnAddZoneTile(zoneTile);
                OnAddZoneTileDelegate?.Invoke(zoneTile);
            }
        }

        /// <summary>
        /// Static function to find the singelton instance
        /// </summary>
        protected static void FindInstance () {
            Instance = (ZoneManager) FindObjectOfType(typeof(ZoneManager));
        }

        protected void Instance_UnregisterZoneTile ( ZoneTile zoneTile ) {
            ZoneTileList.Remove(zoneTile);
            OnRemoveZoneTile(zoneTile);
            OnRemoveZoneTileDelegate?.Invoke(zoneTile);
        }

        public static void UnregisterZoneTile ( ZoneTile zoneTile ) {
            ZoneManager zoneManager = GetInstance();
            if ( zoneManager != null ) {
                zoneManager.Instance_UnregisterZoneTile(zoneTile);
            }
        }

        public static ZoneInfo ActiveZoneInfo () {
            ZoneInfo activeZoneInfo = null;
            ZoneManager zoneManager = GetInstance();
            if ( zoneManager != null ) {
                activeZoneInfo = zoneManager.activeZoneInfo;
            }
            return activeZoneInfo;
        }

        public void OnAddZoneTile ( ZoneTile zoneTile ) {

        }

        public void OnRemoveZoneTile ( ZoneTile zoneTile ) {

        }

        public void OnActiveZoneChanged ( ZoneInfo zoneInfo ) {

        }

        public static ZoneInfo GetActiveZoneInfo ( Transform transform ) {
            ZoneTile zoneTile = transform != null ? GetActiveZoneTile(transform.position) : null;
            return zoneTile != null ? zoneTile.GetActiveZoneInfo(transform) : null;
        }

        public static ZoneTile GetActiveZoneTile ( Vector3 objectPosition ) {
            Terrain terrain = GetActiveTerrain(objectPosition);
            return terrain != null ? terrain.GetComponent<ZoneTile>() : null;
        }

        public static Terrain GetActiveTerrain ( Vector3 objectPosition ) {
            Terrain activeTerrain = null;
            if ( objectPosition != null ) {
                //Get all terrain
                Terrain[] terrains = Terrain.activeTerrains;
                if ( terrains != null ) {
                    for ( int i = 0; i < terrains.Length; i++ ) {
                        Terrain terrain = terrains[i];
                        if ( Contains(terrain, objectPosition) ) {
                            activeTerrain = terrain;
                            break;
                        }
                    }
                }
            }
            return activeTerrain;
        }


        public static bool Contains ( Terrain terrain, Vector3 position ) {
            bool contains = false;
            if ( terrain != null && terrain.enabled && position != null ) {
                // get relative point.
                Vector3 positionRelativeToTerrain = getRelativePosition(terrain, position);
                // determine if the relative point is in the x and z bounds of the terrain.
                if ( positionRelativeToTerrain.x >= 0 && positionRelativeToTerrain.z >= 0 ) {
                    Vector3 terrainSize = terrain.terrainData.size;
                    contains = positionRelativeToTerrain.x < terrainSize.x && positionRelativeToTerrain.z < terrainSize.z;
                }
            }
            return contains;
        }

        public static Vector3 getRelativePosition ( Terrain terrain, Vector3 worldPosition ) {
            Vector3 terrainPosition = worldPosition - terrain.transform.position;
            return terrainPosition;
        }
    }
}

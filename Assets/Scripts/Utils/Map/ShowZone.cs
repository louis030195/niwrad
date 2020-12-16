using UnityEngine;

namespace Utils.Map
{
    public class ShowZone : MonoBehaviour {
        GUIStyle style;
        Rect rect;

        void OnGUI () {
            int w = Screen.width, h = Screen.height;
            if ( style == null ) {
                style = new GUIStyle();
                rect = new Rect(0, 0, w, h * 4 / 100);
                style.alignment = TextAnchor.UpperRight;
                style.fontSize = h * 4 / 100;
                style.normal.textColor = Color.white;
            }
            ZoneInfo activeZoneInfo = ZoneManager.ActiveZoneInfo();
            string zoneName = activeZoneInfo != null ? activeZoneInfo.zoneName : "Default";
            GUI.Label(rect, "Zone: " + zoneName, style);
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CompassUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform compassBar;
    [SerializeField] private Transform player;
    [SerializeField] private float pixelsPer360 = 2000f; // Full 360° range = 2000px (-1000 to +1000)

    [Header("Marker Settings")]
    [SerializeField] private RectTransform markerContainer;
    [SerializeField] private GameObject markerPrefab;

    private List<CompassMarkerInstance> activeMarkers = new();

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        UpdateCompassBar();
        UpdateMarkers();
    }

    void UpdateCompassBar()
    {
        //get yaw and normalize to 0-360
        float playerYaw = player.eulerAngles.y;

        float xPos = Mathf.Repeat(playerYaw / 360f, 1f) * pixelsPer360;
        if (xPos > pixelsPer360 * 0.5f)
            xPos -= pixelsPer360; //shift so that -1000 to +1000 is valid range

        compassBar.localPosition = new Vector2(-xPos, compassBar.localPosition.y);
        //print(playerYaw + " UPDATING COMPASS " + xPos);
    }

    void UpdateMarkers()
    {
        foreach (var marker in activeMarkers)
        {
            if (marker.target == null) continue;

            Vector3 dir = (marker.target.position - player.position).normalized;
            float angle = Vector3.SignedAngle(Vector3.forward, player.InverseTransformDirection(dir), Vector3.up);

            //-180..180 to -pixelsPer360/2 .. +pixelsPer360/2
            float normalized = angle / 360f;
            float x = normalized * pixelsPer360;

            marker.rect.localPosition = new Vector2(x, marker.rect.localPosition.y);
        }
    }

    /// <summary>
    /// Call this to create a marker on the compass bar.
    /// </summary>
    public void CreateWorldMarker(Transform worldTarget, string label, Sprite icon)
    {
        GameObject markerGO = Instantiate(markerPrefab, markerContainer);
        RectTransform rect = markerGO.GetComponent<RectTransform>();

        Image iconImage = markerGO.GetComponentInChildren<Image>();
        if (iconImage) iconImage.sprite = icon;

        TMP_Text text = markerGO.GetComponentInChildren<TMP_Text>();
        if (text) text.text = label;

        activeMarkers.Add(new CompassMarkerInstance
        {
            rect = rect,
            target = worldTarget
        });
    }

    private class CompassMarkerInstance
    {
        public RectTransform rect;
        public Transform target;
    }
}

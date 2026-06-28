using UnityEngine;
using System.Collections;

public class GPSPlayerTracker : MonoBehaviour
{
    [Header("Testing")]
    public bool useFakeLocation = true;
    public float fakeLat = 26.5123f;
    public float fakeLon = 80.2329f;

    [Header("Map Settings")]
    public float originLat = 26.5123f;
    public float originLon = 80.2329f;

    private Vector3 originOffset;

    void Start()
    {
        originOffset = LatLonToMeters(originLat, originLon);
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        if (!useFakeLocation)
        {
            if (!Input.location.isEnabledByUser) yield break;
            Input.location.Start();
            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }
        }
    }

    void Update()
    {
        float currentLat, currentLon;

        if (useFakeLocation)
        {
            currentLat = fakeLat;
            currentLon = fakeLon;
        }
        else
        {
            currentLat = Input.location.lastData.latitude;
            currentLon = Input.location.lastData.longitude;
        }

        Vector3 targetPos = LatLonToMeters(currentLat, currentLon) - originOffset;
        // Instead of jumping instantly to the position:
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
    }

    Vector3 LatLonToMeters(float lat, float lon)
    {
        float x = lon * 20037508.34f / 180f;
        float y = Mathf.Log(Mathf.Tan((90f + lat) * Mathf.PI / 360f)) / (Mathf.PI / 180f);
        y = y * 20037508.34f / 180f;
        return new Vector3(x, 0, y);
    }
}
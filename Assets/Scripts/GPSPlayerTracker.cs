using UnityEngine;
using System.Collections;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

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
        // 1. Request Permission if on Android
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            // Wait a few frames for the user to click "Allow"
            yield return new WaitForSeconds(1f);
        }
#endif

        // 2. Check if the user has enabled Location Services in the phone settings
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled location services in settings.");
            yield break;
        }

        // 3. Start the location service
        Input.location.Start();

        // 4. Wait for initialization
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Timed out waiting for GPS.");
            yield break;
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
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetPos.x, 1f, targetPos.z), Time.deltaTime * 5f);
    }

    Vector3 LatLonToMeters(float lat, float lon)
    {
        float x = lon * 20037508.34f / 180f;
        float y = Mathf.Log(Mathf.Tan((90f + lat) * Mathf.PI / 360f)) / (Mathf.PI / 180f);
        y = y * 20037508.34f / 180f;
        return new Vector3(x, 0, y);
    }
}
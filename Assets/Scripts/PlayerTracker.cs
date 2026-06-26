using System.Collections;
using UnityEngine;
using CesiumForUnity; // Required for the Globe Anchor
using Unity.Mathematics; // Required for double3

[RequireComponent(typeof(CesiumGlobeAnchor))]
public class PlayerTracker : MonoBehaviour
{
    private CesiumGlobeAnchor globeAnchor;
    private bool isTracking = false;

    // Use this height to keep the player slightly above the ground/terrain
    public double playerElevationOffset = 122.0; 

    void Start()
    {
        globeAnchor = GetComponent<CesiumGlobeAnchor>();
        StartCoroutine(StartLocationService());
    }

    IEnumerator StartLocationService()
    {
        // 1. Check if user enabled location
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("Location services not enabled by user.");
            yield break;
        }

        // 2. Start service
        Input.location.Start(1f, 1f);

        // 3. Wait for initialization
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Timed out or failed to initialize GPS.");
            yield break;
        }

        // 4. Tracking successful
        isTracking = true;
    }

    void Update()
    {
        if (!isTracking) return;

        // Fetch continuous coordinates from the phone's hardware
        double currentLat = Input.location.lastData.latitude;
        double currentLon = Input.location.lastData.longitude;

        // Update the Cesium Globe Anchor natively
        // Cesium expects a double3 in the format: (Longitude, Latitude, Height)
        globeAnchor.longitudeLatitudeHeight = new double3(currentLon, currentLat, playerElevationOffset);
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class LocationValidator : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text statusText;
    public Button playButton;

    [Header("Scene Management")]
    [Tooltip("The exact name of your main map scene in the Build Settings")]
    public string mainSceneName = "MainScene";

    [Header("Campus Bounds (Bounding Box)")]
    public float minLat = 26.5000f;
    public float maxLat = 26.5300f;
    public float minLon = 80.2200f;
    public float maxLon = 80.2500f;

    [Header("Testing")]
    [Tooltip("Check this to completely skip all GPS and permission checks (useful for PC testing).")]
    public bool bypassAllChecks = false;

    [Tooltip("Check this to allow the app to start even if you are not on campus right now.")]
    public bool bypassCampusCheck = false;

    void Start()
    {
        if (playButton != null)
        {
            playButton.interactable = false;
        }
        
        StartCoroutine(ValidateLocation());
    }

    IEnumerator ValidateLocation()
    {
        if (bypassAllChecks)
        {
            statusText.text = "Bypass Mode Active! Ready to start.";
            statusText.color = new Color(0.1f, 0.6f, 0.1f); 
            if (playButton != null) playButton.interactable = true;
            yield break; 
        }

        statusText.text = "Checking permissions...";

        // 1. Check and Request Location Permission (Android)
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            
            // Wait dynamically for the user to make a choice (up to 30 seconds)
            int waitTime = 60; // 60 loops * 0.5s = 30 seconds
            while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) && waitTime > 0)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime--;
            }
        }

        // Final check after the popup
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            statusText.text = "Error: Location permission is required to use this app.";
            statusText.color = Color.red;
            yield break;
        }
#endif

        // 2. Check if the device's global location setting is turned ON
        if (!Input.location.isEnabledByUser)
        {
            statusText.text = "Error: Please turn on GPS in your phone settings and restart the app.";
            statusText.color = Color.red;
            yield break;
        }

        // 3. Connect to GPS to get the current coordinates
        statusText.text = "Acquiring GPS signal (This may take a moment outdoors)...";
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            statusText.text = "Error: Could not connect to GPS satellites. Ensure you are outdoors.";
            statusText.color = Color.red;
            yield break;
        }

        // 4. Retrieve the coordinates
        float currentLat = Input.location.lastData.latitude;
        float currentLon = Input.location.lastData.longitude;

        Input.location.Stop(); 

        // 5. Verify the user is actually physically on the campus
        if (!bypassCampusCheck)
        {
            statusText.text = "Verifying campus location...";
            yield return new WaitForSeconds(1f); 

            if (currentLat < minLat || currentLat > maxLat || currentLon < minLon || currentLon > maxLon)
            {
                statusText.text = "Error: You are not currently located on campus!";
                statusText.color = Color.red;
                yield break; 
            }
        }

        // 6. Success! Enable the Play button
        statusText.text = "All conditions met! Ready to start.";
        statusText.color = new Color(0.1f, 0.6f, 0.1f); 
        
        if (playButton != null)
        {
            playButton.interactable = true;
        }
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}
using UnityEngine;
using System.Collections;

/*
 * Created by Geordie Powers on Feb 13 2015
 * 
 * Gets the skybox from the current level's WorldInfo and applies it to the camera
 * This script is intended for application to cameras. Anything else would be pointless.
 * 
 */

public class CamSkybox : MonoBehaviour
{
    private void Start()
    {
        if (!gameObject.GetComponent<Skybox>())
        {
            gameObject.AddComponent<Skybox>();
        }

        WorldInfo.info.AddSkyboxWatcher(this.gameObject);
        UpdateSkybox();
    }

    public void UpdateSkybox()
    {
        gameObject.GetComponent<Skybox>().material = WorldInfo.info.worldSkybox;
    }
}
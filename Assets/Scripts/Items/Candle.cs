using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Candle : MonoBehaviour
{
    private Light2D mainLight;
    private Light2D fireLight;
    private Collider2D trigger;

    private void Start()
    {
        // Get collider trigger object
        trigger = GetComponent<Collider2D>();

        // Find the Light2D components in the child objects
        mainLight = transform.Find("Lights/Main Light").GetComponent<Light2D>();
        fireLight = transform.Find("Lights/Fire Light").GetComponent<Light2D>();

        // Start with the lights off
        if (mainLight != null)
        {
            Debug.Log("Main Light Object Found");
            mainLight.intensity = 0;
        }

        if (fireLight != null)
        {
            Debug.Log("Fire Light Object Found");
            fireLight.intensity = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire"))
        {
            Debug.Log("Player triggered object");
            // Turn on the lights fully
            if (mainLight != null) mainLight.intensity = 1;
            if (fireLight != null) fireLight.intensity = 0.5f;
        }
    } 
}

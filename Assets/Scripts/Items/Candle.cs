using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Candle : MonoBehaviour
{
    private Light2D _mainLight;
    private Light2D _fireLight;
    private Collider2D _trigger;

    private void Start()
    {
        // Get collider trigger object
        _trigger = GetComponent<Collider2D>();

        // Find the Light2D components in the child objects
        _mainLight = transform.Find("Lights/Main Light").GetComponent<Light2D>();
        _fireLight = transform.Find("Lights/Fire Light").GetComponent<Light2D>();

        // Start with the lights off
        if (_mainLight != null)
        {
            _mainLight.intensity = 0;
        }

        if (_fireLight != null)
        {
            _fireLight.intensity = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fire"))
        {
            // Turn on the lights fully
            if (_mainLight != null) _mainLight.intensity = 1;
            if (_fireLight != null) _fireLight.intensity = 0.5f;
        }
    } 
}

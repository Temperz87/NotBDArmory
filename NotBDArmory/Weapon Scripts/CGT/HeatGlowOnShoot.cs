using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatGlow : MonoBehaviour // Thank you cheese for this script, even if i modified it a lot and you hate it now :D
{
    private MeshRenderer[] emisiveMaterials;
    private float temperature;
    public float heatLossRate = .5f;
    private Gun gun;

    private static Color32 emissionColor = new Color32(32, 32, 32, 255);

    private void Start()
    {
        this.emisiveMaterials = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.EnableKeyword("_EMISSION");
        gun = GetComponent<Gun>();
        gun.OnFired += AddHeat;
    }

    private void Update()
    {
        temperature -= temperature * heatLossRate * Time.deltaTime;
        temperature = Mathf.Clamp(temperature, 0, 6);
        SetEmission((Color)(emissionColor) * temperature);
    }

    public void AddHeat()
    {
        temperature += (Time.deltaTime * 4);
        SetEmission((Color)(emissionColor) * temperature);
    }

    private void SetEmission(Color colour)
    {
        foreach (MeshRenderer renderer in emisiveMaterials)
            renderer.material.SetColor("_EmissionColor", colour);
    }
}
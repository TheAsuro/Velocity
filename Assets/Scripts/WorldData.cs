using UnityEngine;

[CreateAssetMenu]
public class WorldData : ScriptableObject
{
    public GameObject playerTemplate;
    public GameObject demoPlayerTemplate;
    public Color backgroundColor = Color.black;
    public Material skybox;
    public float deathHeight = -100f;
}

using UnityEngine;

public class WorldData : ScriptableObject
{
    public GameObject playerTemplate;
    public Color backgroundColor = Color.black;
    public Material skybox;
    public float deathHeight = -100f;
}

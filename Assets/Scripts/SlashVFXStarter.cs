using UnityEngine;

public class SlashVFXStarter : MonoBehaviour
{
    private void Start()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            mat.SetFloat("_StartTime", Time.time);
        }
    }
}

using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    [SerializeField] private GameObject stepVFX;
    [SerializeField] private Vector3 vfxOffset = new Vector3(0,0,0);
    public void Step()
    {
        //Debug.Log("Step fired");
        GameObject step = Instantiate(stepVFX, (transform.parent.position - vfxOffset), Quaternion.identity);
        Destroy(step, .3f);
    }
}

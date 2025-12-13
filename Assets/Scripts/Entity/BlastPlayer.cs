using UnityEngine;

public class BlastPlayer : MonoBehaviour
{
    public PlayerManager targetPlayer;
    [SerializeField] private float blastDist = 1.5f;
    [SerializeField] private float damage = 1f;
    private bool blasted = false;

    private FlyingEntity fe;

    private void Start()
    {
        fe = GetComponent<FlyingEntity>();
    }

    private void Update()
    {
        targetPlayer = fe.getDiveTarget();

        if (targetPlayer != null)
        {
            if (Vector3.Distance(targetPlayer.transform.position, transform.position) < blastDist)
            {
                if (!blasted)
                {
                    //damage player
                    Debug.Log("BLAST");
                    blasted = true;
                }
            }
            else
            {
                blasted = false;
            }
        }
    }
}

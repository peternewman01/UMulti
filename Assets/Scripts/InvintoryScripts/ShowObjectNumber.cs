using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ShowObjectNumber : NetworkBehaviour
{
    public TMP_Text targetText;
    [SerializeField] private Invintory inv;
    [SerializeField] private Objects target = Objects.WOOD;
    [SerializeField] private string targetName = "Wood";

    private void Start()
    {
        inv = this.gameObject.GetComponent<Invintory>();
        foreach (TMP_Text t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            Debug.Log(t.gameObject.name);
            if(t.gameObject.name == targetName)
            {
                targetText = t; 
                break;
            }
        }
    }

    private void Update()
    {
        if(IsOwner)
        {
            if (inv.Stuff.ContainsKey((int)target))
            {
                targetText.text = targetName + ": " + inv.Stuff[(int)target].count.ToString();
            }
            else
            {
                targetText.text = targetName + ": 0";
            }
        }
    }
}

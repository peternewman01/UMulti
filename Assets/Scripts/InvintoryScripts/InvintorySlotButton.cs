using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InvintorySlotButton : MonoBehaviour
{
    public Invintory Invintory;
    public TMP_Text Lable;
    public TMP_Text Count;

    public int Target;

    private void Start()
    {
        TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>();
        Lable = texts[0];
        Count = texts[1];


    }

    private void Update()
    {
        if (Invintory.Stuff.ContainsKey(Target))
        {
            Lable.text = Invintory.Stuff[Target].obj.ObjectName;
            Count.text = Invintory.Stuff[Target].count.ToString();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Activate()
    {

    }
}

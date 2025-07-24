using System;
using UnityEngine;

public class CraftingPanel : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private PlayerControler playerControler;
    [SerializeField] private Invintory playerInvintory;

    public Invintory PlayerInvintory => playerInvintory;

    private void Start()
    {
        playerControler = this.gameObject.GetComponent<PlayerControler>();
        playerInvintory = this.gameObject.GetComponent<Invintory>();
        HidePanel();
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerControler.MainCam.freezeRotation = false;
    }

    public void HidePanel()
    {
        panel.SetActive(false); 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerControler.MainCam.freezeRotation = true;
    }    
}

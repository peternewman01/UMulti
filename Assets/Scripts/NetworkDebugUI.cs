using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkDebugUI : MonoBehaviour
{
    [SerializeField] Button serverButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;

    //Cameras
    [SerializeField] GameObject freeCam;
    [SerializeField] GameObject playerCam;

    private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            freeCam.SetActive(false);
            playerCam.SetActive(true);
        });
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            freeCam.SetActive(false);
            playerCam.SetActive(true);
        });
    }
}

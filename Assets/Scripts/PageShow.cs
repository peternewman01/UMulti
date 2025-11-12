using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Button))]
public class PageShow : MonoBehaviour
{
    [SerializeField] private GameObject targetPage;
    private static PageShow showingPage = null;

    private void Start()
    {
        if (showingPage == null)
        {
            PageClick();
        }
    }

    private void Update()
    {
        if (showingPage == this)
        {
            targetPage.SetActive(true);
        }
        else
        {
            targetPage.SetActive(false);
        }
    }

    public void PageClick()
    {
        showingPage = this;
    }
}

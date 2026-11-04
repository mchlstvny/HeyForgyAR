
using UnityEngine;

public class ForgyQuit : MonoBehaviour
{
    public GameObject quitConfirmPanel;

    void Start()
    {
        quitConfirmPanel.SetActive(false);
    }

    public void OpenQuitPopup()
    {
        quitConfirmPanel.SetActive(true);
    }

    public void CloseQuitPopup()
    {
        quitConfirmPanel.SetActive(false);
    }

    public void ConfirmQuit()
    {
        Application.Quit();
    }
}

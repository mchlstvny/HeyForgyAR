
using UnityEngine;
using UnityEngine.EventSystems;

public class ResetButtonSelection : MonoBehaviour
{
    public void ClearSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}

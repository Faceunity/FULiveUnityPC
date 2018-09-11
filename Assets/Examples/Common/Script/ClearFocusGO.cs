using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ClearFocusGO : MonoBehaviour {
	public void ClearFocusGameObject() {
        if(EventSystem.current.alreadySelecting == false)
            EventSystem.current.SetSelectedGameObject(null);
    }
}

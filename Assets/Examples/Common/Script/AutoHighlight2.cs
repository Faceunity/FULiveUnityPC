using UnityEngine;
using System.Collections;

public class AutoHighlight2 : MonoBehaviour {

    public GameObject[] gos;

    private void Awake()
    {
        this.GetComponent<AddPointEnterEvent>().AddListener(delegate
        {
            foreach(var go in gos)
            {
                go.SetActive(true);
            }
        });
        this.GetComponent<AddPointExitEvent>().AddListener(delegate
        {
            foreach (var go in gos)
            {
                go.SetActive(false);
            }
        });
    }
}

using UnityEngine;
using System.Collections;

public class AutoHighlight4 : MonoBehaviour {

    public GameObject[] normal_gos;
    public GameObject[] highlight_gos;

    private void Awake()
    {
        this.GetComponent<AddPointEnterEvent>().AddListener(delegate
        {
            foreach (var go in normal_gos)
            {
                go.SetActive(false);
            }
            foreach (var go in highlight_gos)
            {
                go.SetActive(true);
            }
        });
        this.GetComponent<AddPointExitEvent>().AddListener(delegate
        {
            foreach (var go in normal_gos)
            {
                go.SetActive(true);
            }
            foreach (var go in highlight_gos)
            {
                go.SetActive(false);
            }
        });
    }
}

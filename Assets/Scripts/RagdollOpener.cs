using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollOpener : MonoBehaviour
{
    [SerializeField] private GameObject RagdolMan;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {

        RagdolMan.GetComponent<Animator>().enabled = false;
        RagdolMan.transform.GetChild(2).gameObject.SetActive(true);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("A");
        if (other.gameObject.CompareTag("Player"))
        {
            gameObject.GetComponent<Animator>().SetBool("fall", true);
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("B");
        if (other.gameObject.CompareTag("Player"))
        {
            gameObject.GetComponent<Animator>().SetBool("fall", true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeScript : MonoBehaviour {

    public GameObject CharacterController;

    private void Update()
    {
        
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            CharacterController.GetComponent<CharacterControllerBehaviour>().CurrentLedge = this.gameObject;
            if (Input.GetButtonDown("Interact"))
            {
                col.gameObject.GetComponent<CharacterControllerBehaviour>().IsClimbing = true;
            }
        }
    }
}

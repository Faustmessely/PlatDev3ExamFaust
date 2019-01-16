using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeScript : MonoBehaviour {

    private void OnTriggerStay(Collider col)
    {
        if (col.tag == "Player")
        {
            if (Input.GetButtonDown("Interact"))
            {
                col.gameObject.GetComponent<CharacterControllerBehaviour>().IsClimbing = true;
            }
        }
    }
}

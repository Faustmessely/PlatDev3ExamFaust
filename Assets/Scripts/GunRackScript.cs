using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRackScript : MonoBehaviour {

    [SerializeField]
    private GameObject _player;
    private bool _canTakeWeapon;
    private bool _inRange;
    private float _timer=3000;

	
	// Update is called once per frame
	void Update () {
        if (_inRange == true && Input.GetButtonDown("Interact") &&_timer >= 3000)
        {
            _player.GetComponent<CharacterControllerBehaviour>().HasWeapon = true;
            _timer = 0;
        }
	}

    private void FixedUpdate()
    {
            _timer++;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag=="Player")
        {
            _inRange = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            _inRange = false;
        }
    }
}

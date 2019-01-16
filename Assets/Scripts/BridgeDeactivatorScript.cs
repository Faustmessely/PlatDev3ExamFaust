using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDeactivatorScript : MonoBehaviour {

    [SerializeField]
    private GameObject _bridge;
    private bool _destroyBridge=false;
    private bool _startTimer = false;
    public float _timer;

	
	// Update is called once per frame
	void Update () {
        if (_destroyBridge == true)
        {
            _bridge.gameObject.SetActive(false);
            _startTimer = true;
        }

        if (_destroyBridge == false && _timer>=1000)
        {
            _bridge.gameObject.SetActive(true);
            _startTimer = false;
            _timer = 0;
        }
    }

    private void FixedUpdate()
    {
        if (_startTimer==true)
        {
            _timer++;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag=="StabHitBox")
        {
            _destroyBridge = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "StabHitBox")
        {
            _destroyBridge = false;
        }
    }
}

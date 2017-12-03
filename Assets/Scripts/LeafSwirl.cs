using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafSwirl : MonoBehaviour {

    private float timeElapsed;
    private float lifetime;

    // Use this for initialization
	void Start () {
        lifetime = GetComponent<ParticleSystem>().main.startLifetime.constant;
	}
	
	// Update is called once per frame
	void Update () {
        timeElapsed += Time.deltaTime;
        if (timeElapsed >= lifetime) Destroy(gameObject);
	}
}

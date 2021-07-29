using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseController : MonoBehaviour
{
    public float playerForce;
    //public float noiseRate;

    public float nearZeroSpeed;

    private Rigidbody rigid;
    private AudioSource noise;



    //private bool playingNoise;

    private float lastFrameSpeed;

    private void Start()
    {
        rigid = this.GetComponent<Rigidbody>();
        noise = this.GetComponent<AudioSource>();
        //playingNoise = false;
        lastFrameSpeed = 0.0f;
    }

    private void Update()
    {
        
        if((lastFrameSpeed <= nearZeroSpeed && rigid.velocity.magnitude > lastFrameSpeed + nearZeroSpeed) || (rigid.velocity.magnitude <= nearZeroSpeed && lastFrameSpeed > rigid.velocity.magnitude + nearZeroSpeed))
        {
            noise.Play();
            //playingNoise = true;

        }

        lastFrameSpeed = rigid.velocity.magnitude;
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Colpito: " + this.name);
            Vector3 forceDirection = (this.transform.position - other.transform.position).normalized;
            rigid.AddForceAtPosition(forceDirection * playerForce, this.transform.position);

            foreach(GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                enemy.GetComponent<SimpleAI>().SetDestination(this.transform.position);
            }
        }
    }
}

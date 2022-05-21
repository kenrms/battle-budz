using Cinemachine;
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody rigidbody;
    CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.centerOfMass = transform.position;
    }

    public void Fire()
    {
        rigidbody.AddForce(transform.forward * (100 * Random.Range(1.3f, 1.7f)), ForceMode.Impulse);
        impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();
        impulseSource.GenerateImpulse(Camera.main.transform.forward);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Player")
        {
            rigidbody.isKinematic = true;
            StartCoroutine(Countdown());
        }
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}

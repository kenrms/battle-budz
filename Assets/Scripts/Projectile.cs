using Cinemachine;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject HitDecal;
    public float Speed;
    public float TimeToDestroy = 3f;
    public CinemachineImpulseSource impulseSource;
    public Vector3 Target { get; set; }
    public bool hit { get; set; }

    private void OnEnable()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        impulseSource.GenerateImpulse(Camera.main.transform.forward);
        Destroy(gameObject, TimeToDestroy);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            current: transform.position,
            target: Target,
            maxDistanceDelta: Speed * Time.deltaTime);

        if (!hit && Vector3.Distance(transform.position, Target) < .01f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);

        Instantiate(
            original: HitDecal,
            position: contact.point + contact.normal * .0001f,
            rotation: Quaternion.LookRotation(contact.normal));

        Destroy(gameObject);
    }
}

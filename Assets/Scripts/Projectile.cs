using Cinemachine;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject HitDecal;
    public float Speed;
    public float TimeToDestroy = 3f;
    public Vector3 Target { get; set; }
    public bool IsHit { get; set; }

    private void OnEnable()
    {
        Destroy(gameObject, TimeToDestroy);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            current: transform.position,
            target: Target,
            maxDistanceDelta: Speed * Time.deltaTime);

        if (!IsHit && Vector3.Distance(transform.position, Target) < .01f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        var contact = collision.GetContact(0);

        if (collision.transform.CompareTag("Actor"))
        {
            // make player take dmg
            var actor = collision.gameObject.GetComponentInParent<Actor>();
            actor.TakeDamage(10);
        }
        else if (collision.transform.CompareTag("Environment"))
        {
            Instantiate(
                original: HitDecal,
                position: contact.point + contact.normal * .0001f,
                rotation: Quaternion.LookRotation(contact.normal));
        }
    }
}

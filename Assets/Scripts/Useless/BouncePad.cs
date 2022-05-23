using UnityEngine;

public class BouncePad : MonoBehaviour
{
    public float BounceForce;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Actor"))
        {
            var pctrl = collision.transform.GetComponentInParent<PlayerController>();

            if (pctrl != null)
            {
                pctrl.verticalVelocity = BounceForce;
            }
        }
    }
}

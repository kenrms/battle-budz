using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : MonoBehaviour
{
    public CinemachineImpulseSource ImpulseSource;

    [SerializeField]
    protected WeaponType type;

    void Start()
    {
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void ActionDown()
    {
        ImpulseSource.GenerateImpulse(Camera.main.transform.forward);
    }
}

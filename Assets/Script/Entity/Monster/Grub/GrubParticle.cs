using System;
using UnityEngine;

public class GrubParticle : MonoBehaviour
{
    
    ParticleShooter shooter;

    private void Awake()
    {
        shooter = GetComponent<ParticleShooter>();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    ParticleToggle(true);
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    ParticleToggle(false);

        //}
    }

    public void ParticleToggle(bool activate)
    {
        shooter.enabled = activate;
    }

    public void Flip()
    {
        float newZ = transform.localEulerAngles.z + 180f;
        transform.localRotation = Quaternion.Euler(0f, 0f, newZ);
    }
    
}

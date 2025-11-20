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
        if (Input.GetKeyDown(KeyCode.K))
        {
            ParticleToggle(true);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            ParticleToggle(false);

        }
    }

    public void ParticleToggle(bool activate)
    {
        shooter.enabled = activate;
    }

    
}

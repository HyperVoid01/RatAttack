using System;
using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float radius;
    [SerializeField] private float range;
    [SerializeField] private float fireRate; // shots per second
    [SerializeField] private float recoilKickAngle;
    [SerializeField] private LayerMask layerMask;
    
    [SerializeField] private ParticleSystem sprayParticle;

    private float nextFireTime;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public bool CanShoot => Time.time >= nextFireTime;

    public void Shoot()
    {
        if (!CanShoot || GameManager.Instance.Ammo < 1)
            return;

        nextFireTime = Time.time + (1f / fireRate);

        SoundPlayer.Instance.PlaySound(SoundID.ShotgunFire, transform.position);
        
        sprayParticle.Play();
        RaycastHit[] hit = Physics.CapsuleCastAll(transform.position, transform.position + transform.forward * range, radius, transform.position + transform.forward, range,
            layerMask);

        foreach (RaycastHit target in hit)
        {
            if (target.collider.gameObject.GetComponent<ITargetable>() != null)
            {
                target.collider.gameObject.GetComponent<ITargetable>().TakeDamage(damage);
            }
        }
        
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.AddRecoil(recoilKickAngle);
        }

        GameManager.Instance.DecreaseAmmo(1);
        HUDManager.Instance.UpdateAmmoCounter();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * range);
        Gizmos.DrawWireSphere(transform.position + transform.forward * range, radius);
    }
}
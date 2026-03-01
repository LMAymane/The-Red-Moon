using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("Gun Stats")]
    public float damage = 30f;
    public float range = 500f;
    public float fireRate = 0.075f;
    private float nextTimeToFire = 0f;

    [Header("Ammo Settings")]
    public int maxAmmo = 30;
    private int currentAmmo;
    public float reloadTime = 2.5f;
    private bool isReloading = false;

    [Header("Aiming Settings (ADS)")]
    public Vector3 normalLocalPosition;
    public Vector3 aimingLocalPosition;
    public float aimAimingSpeed = 10f;

    [Header("Camera Zoom (ADS)")]
    public float normalFOV = 60f;
    public float aimFOV = 45f;
    public float zoomSpeed = 10f;
    
    [Header("Recoil Settings")]
    public float recoilKickback = -0.08f;
    public float recoilUpward = 0.03f;
    public float recoilSideShake = 0.01f;
    public float recoilRecoverySpeed = 20f;
    private Vector3 currentRecoil;

    [Header("Weapon Bobbing (Walking)")]
    public float bobSpeed = 14f;
    public float bobAmount = 0.015f;
    private float bobTimer = 0f;

    [Header("Weapon Breathing (Idle)")]
    public float idleBobSpeed = 2f;
    public float idleBobAmount = 0.005f;
    private float idleBobTimer = 0f;

    [Header("Audio & Visuals")]
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public Light muzzleLight;
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;

    void Start()
    {
        currentAmmo = maxAmmo;
        normalLocalPosition = transform.localPosition;

        if (fpsCam != null) fpsCam.fieldOfView = normalFOV;
    }

    void Update()
    {
        Vector3 targetPosition = normalLocalPosition;

        bool isAiming = (Input.GetButton("Fire2")) && !isReloading;

        if (isAiming)
        {
            targetPosition = aimingLocalPosition;

         
            if (fpsCam != null)
                fpsCam.fieldOfView = Mathf.Lerp(fpsCam.fieldOfView, aimFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {

            if (fpsCam != null)
                fpsCam.fieldOfView = Mathf.Lerp(fpsCam.fieldOfView, normalFOV, Time.deltaTime * zoomSpeed);

            float moveSpeed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;

            if (moveSpeed > 0.1f)
            {
                bobTimer += Time.deltaTime * bobSpeed;
                targetPosition.y += Mathf.Sin(bobTimer) * bobAmount;
                targetPosition.x += Mathf.Cos(bobTimer / 2f) * bobAmount;
                idleBobTimer = 0f;
            }
            else
            {
                idleBobTimer += Time.deltaTime * idleBobSpeed;
                targetPosition.y += Mathf.Sin(idleBobTimer) * idleBobAmount;
                bobTimer = 0f;
            }
        }

        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition + currentRecoil, Time.deltaTime * aimAimingSpeed);

        if (isReloading) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        if (audioSource != null && reloadSound != null) audioSource.PlayOneShot(reloadSound);
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    void Shoot()
    {
        currentAmmo--;

        float randomX = Random.Range(-recoilSideShake, recoilSideShake);
        float randomY = Random.Range(recoilUpward * 0.5f, recoilUpward * 1.5f);
        currentRecoil += new Vector3(randomX, randomY, recoilKickback);

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
            muzzleFlash.Play();
        }

        if (muzzleLight != null)
        {
            StartCoroutine(FlashLight());
        }

        if (audioSource != null && shootSound != null) audioSource.PlayOneShot(shootSound);

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            // Target target = hit.transform.GetComponent<Target>(); 
            // if (target != null) target.TakeDamage(damage);
        }
    }

    IEnumerator FlashLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        muzzleLight.enabled = false;
    }
}
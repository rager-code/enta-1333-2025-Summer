using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.Pool;

public class ShootOverTime : MonoBehaviour
{
	[Header("Projectile")]
	[SerializeField] private ArcingProjectile ProjectilePrefab;
	[SerializeField] private Transform ProjectileStartPoint;

	

	public ObjectPool<ArcingProjectile>ArrowPool;

	// Replace this with some kind of targeted enemy position
	[SerializeField] private Transform DebugEndPoint;

	[Header("Flight Settings")]
	[SerializeField] [Min(0.1f)] private float ProjectileSpeed = 5f; // units / second
	[SerializeField] private float ArcHeight = 2f; // world units

	/// <summary>
	///     Fires the projectile
	/// </summary>
	public void Shoot()
	{


		// Instantiate with generic overload so no GetComponent is needed.
		var proj =
			Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);

		proj.Launch(transform.position, DebugEndPoint.position, ProjectileSpeed, ArcHeight);

		

	}
	public void CreatePool()
	{
        ArrowPool = new ObjectPool<ArcingProjectile>(CreateFunc, ArrowOnGet, ArrowOnLoose, ArrowDestroy, true, 1, 10);
    }

    private void ArrowOnLoose(ArcingProjectile projectile)
    {
		projectile.gameObject.SetActive(false);
    }

    private void ArrowDestroy(ArcingProjectile projectile)
    {
        Destroy(projectile.gameObject);
    }

    private void ArrowOnGet(ArcingProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private ArcingProjectile CreateFunc()
	{
		return Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);

	}
}

using UnityEngine;
using System;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Boulder : MonoBehaviour
{
	private VoxelRemover explosionQueryPrefab;
	ParticleEmitter smokePrefab;
	private bool launched = false;
	private float launchTime = -1;
	private bool collided = false;
	private float collisionTime = -1;
	
	void Awake ()
	{
		gameObject.layer = LayerMask.NameToLayer ("Boulder");
		rigidbody.isKinematic = true;
	}

	public void Launch (float strength, VoxelRemover explosionQueryPrefab, ParticleEmitter smokePrefab)
	{
		this.explosionQueryPrefab = explosionQueryPrefab;
		this.smokePrefab = smokePrefab;
		rigidbody.isKinematic = false;
		rigidbody.AddForce (transform.TransformDirection (new Vector3 (0.0f, 0.7f, 0.7f)) * strength, ForceMode.Impulse);
		launchTime = Time.time;
		launched = true;
	}
	
	void OnCollisionEnter (Collision collision)
	{
		if (collided) {
			return;
		}
		
		// FIXME: checking invariants
		if (explosionQueryPrefab == null) {
			throw new Exception ("explosionQueryPrefab == null");
		}
	
		Vector3 hitPoint = collision.contacts [0].point;
		
		VoxelRemover explosionQuery = (VoxelRemover)Instantiate (explosionQueryPrefab, hitPoint, Quaternion.identity);
		explosionQuery.Execute ();
		Destroy (explosionQuery.gameObject);
		
		Instantiate (smokePrefab, hitPoint, Quaternion.identity);
		
		collided = true;
		collisionTime = Time.time;
	}
	
	void Update ()
	{
		if (!launched) {
			return;
		}
	
		if (Time.time - launchTime > 30.0f) {
			Destroy (gameObject);
		}
	
		if (!collided) {
			return;
		}
		
		if (Time.time - collisionTime >= 2.0f) {
			Destroy (gameObject);
		}
	}
}

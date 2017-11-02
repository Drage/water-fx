using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CollisionSoundTrigger : MonoBehaviour, ISoundEventGenerator
{
	private static float minVelocityThreshold = 2.0f;
	private static float maxMass = 10.0f;
	
	private new Rigidbody2D rigidbody;
    
    private SoundEvent playCrashSound;
    public event SoundEvent OnSoundEvent
    {
        add { playCrashSound += value; }
        remove { playCrashSound -= value; }
    }

	private void Start()
	{
		rigidbody = GetComponent<Rigidbody2D>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
    {
		if (rigidbody.velocity.magnitude < minVelocityThreshold)
			return;
        
        float volume = 1.0f;
        volume *= Mathf.Clamp(rigidbody.mass / maxMass, 0, 1);
            
        if (playCrashSound != null)
            playCrashSound(volume);
    }
}

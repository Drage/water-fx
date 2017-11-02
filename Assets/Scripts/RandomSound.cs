using UnityEngine;
using System.Collections;

public class RandomSound : MonoBehaviour 
{
    public MonoBehaviour trigger;
    private ISoundEventGenerator soundEventGenerator;
    public AudioClip[] sounds;
    
	private void Start()
    {
        if (trigger != null)
        {
            soundEventGenerator = (ISoundEventGenerator)trigger;
            soundEventGenerator.OnSoundEvent += Play;
        }
    }
    
    public void Play(float volume = 1.0f)
    {
        if (GetComponent<AudioSource>().isPlaying)
        {
            if (GetComponent<AudioSource>().volume < volume)
                GetComponent<AudioSource>().Stop();
        }
        
        GetComponent<AudioSource>().PlayOneShot(sounds[Random.Range(0, sounds.Length)], volume);
    }
}

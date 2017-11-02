using UnityEngine;
using System.Collections;

public delegate void SoundEvent(float volume);

public interface ISoundEventGenerator 
{
    event SoundEvent OnSoundEvent;
}

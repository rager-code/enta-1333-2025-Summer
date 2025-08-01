using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    //AudioSource m_MyAudioSourcee;

    public AudioClip[] soundEffects;
    public enum soundsNames
    {

        spawnPlayerUnit,
        BackGroundSounds,
        Music,

    }
    
    //public soundsNames Test;
    public void PlaySound(soundsNames name)
    {

        AudioSource.PlayClipAtPoint(soundEffects[(int)name], transform.position);


    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            PlaySound(soundsNames.spawnPlayerUnit);
        }
    }
    public void Awake()
    {
        PlaySound(soundsNames.Music);
        PlaySound(soundsNames.BackGroundSounds);
    }

}

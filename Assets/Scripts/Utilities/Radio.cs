using UnityEngine;
using System.Collections;

public class Radio : MonoBehaviour {

    AudioSource musicSource1;
    AudioSource musicSource2;
    AudioSource combatSource;

    float minVolume = 0.3f;
    float maxVolume = 1.0f;

    float crossTime = 199.2f;
    public bool combat = false;
    public bool praying = false;

    void Awake()
    {
        musicSource1 = GetComponents<AudioSource>()[0];
        musicSource2 = GetComponents<AudioSource>()[1];
        combatSource = GetComponents<AudioSource>()[2];

        musicSource1.Play();
        combatSource.volume = 0;
    }

	
	// Update is called once per frame
	void Update () {

        if (!musicSource2.isPlaying && musicSource1.time >= crossTime) musicSource2.Play();
        if (!musicSource1.isPlaying && musicSource2.time >= crossTime) musicSource1.Play();

        if (praying)
        {
            if (musicSource1.volume > minVolume) musicSource1.volume -= 0.01f;
            if (musicSource2.volume > minVolume) musicSource2.volume -= 0.01f;
            if (combatSource.isPlaying) if(combatSource.volume > minVolume) combatSource.volume -= 0.01f;  
        }
        else
        {
            if (combat)
            {
                if (!combatSource.isPlaying) combatSource.Play();

                if (musicSource1.volume > minVolume) musicSource1.volume -= 0.01f;
                if (musicSource2.volume > minVolume) musicSource2.volume -= 0.01f;
                if (combatSource.volume < maxVolume) combatSource.volume += 0.01f;
            }
            else
            {
                if (musicSource1.volume < maxVolume) musicSource1.volume += 0.01f;
                if (musicSource2.volume < maxVolume) musicSource2.volume += 0.01f;
                if (combatSource.volume > 0)
                {
                    combatSource.volume -= 0.01f;
                    if (combatSource.volume < 0)
                    {
                        combatSource.volume = 0;
                        combatSource.Stop();
                        combatSource.time = 0;
                    }
                }
            }
        }
    }
}

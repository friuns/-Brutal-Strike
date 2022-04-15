using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[StaticReset]
[Serializable]
public class AudioClip2 : IEnumerable<AudioClip> /*, ISerializationCallbackReceiver*/
{
//    [Serializable]
//    public class Sound
//    {
//        public AudioClip clip;
//    }
    public bool playInSequence;
    public float volume = 1;
    
    public float audioClipLoudness; //dont set to 1 in arrays serialization still give 0
    public float AudioClipLoudness { get { return audioClipLoudness == 0 ? 1 : audioClipLoudness; } }
    
    public AudioClip[] audioClips = new AudioClip[0];

    public string name;
    private AudioClip last;
    public float lastPlayTime;
    private int frame = 0;
    #if game
    public bool hasSound { get { return sound; } }
    public AudioClip sound
    {
        get
        {
            
            lastPlayTime = Time.time;
            frame++;
            AudioClip audioClip;
            if (playInSequence)
            {
                audioClip = audioClips.GetClampedSQ(frame);
            }
            else
            {
                if (audioClips.Length == 0)
                    return null;
                if (audioClips.Length == 1)
                    return audioClips[0];
                audioClip = audioClips.Random();
                if (audioClip == last)
                    audioClip = audioClips.FirstOrDefault(a => a != last);
                
            }
            last = audioClip;
            return audioClip;
        }
    }
    public AudioClip2 Fallback(AudioClip2 other)
    {
        if (hasSound) return this;
        return other;
    }
#endif
    public AudioClip this[int index] { get { return audioClips[index]; } set { audioClips[index] = value; } }
    public int Length { get { return audioClips.Length==1 && audioClips[0]==null?0: audioClips.Length ; } }
    

    public IEnumerator<AudioClip> GetEnumerator()
    {
        foreach (var a in audioClips)
            yield return a;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    
    public AudioClip distantSound;
    public float distantSoundVolume=1;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Sound
{
    public string ID;
    public AudioClip sound;
}

public class AudioManager : MonoBehaviour
{
    private const int MAX_SRCS = 20;
    public Sound[] sounds;
    public static AudioManager instance;

    // IDs that are currently muted (also applies to sounds played later)
    private readonly HashSet<string> mutedIDs = new HashSet<string>();

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
            Destroy(gameObject);
        if (AudioManager.instance != null)
            Destroy(gameObject);

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Lazy workaround
        if (instance == null)
            instance = this;
    }

    // Returns the clip registered under an ID, or null if none exists
    private static AudioClip GetClip(string ID)
    {
        foreach (Sound s in instance.sounds)
        {
            if (s.ID == ID)
                return s.sound;
        }
        return null;
    }

    public static void Play(string ID, float volume, float pitch, bool loop)
    {
        if (instance.GetComponents<AudioSource>().Length >= MAX_SRCS)
            return;

        // Find the correct sound
        AudioClip clip = GetClip(ID);
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: no sound with ID '" + ID + "'");
            return;
        }

        // Play the sound
        AudioSource source = instance.gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.mute = instance.mutedIDs.Contains(ID);
        source.Play();

        if (!loop)
            instance.StartCoroutine(DespawnSource(source.clip.length, source));
    }

    // Mutes (or unmutes) every playing instance of a sound. The sounds keep
    // playing silently, and new plays of this ID start muted until unmuted.
    public static void Mute(string ID, bool mute = true)
    {
        if (mute)
            instance.mutedIDs.Add(ID);
        else
            instance.mutedIDs.Remove(ID);

        AudioClip clip = GetClip(ID);
        if (clip == null)
            return;

        foreach (AudioSource src in instance.GetComponents<AudioSource>())
        {
            if (src.clip == clip)
                src.mute = mute;
        }
    }

    public static void Unmute(string ID)
    {
        Mute(ID, false);
    }

    public static bool IsMuted(string ID)
    {
        return instance.mutedIDs.Contains(ID);
    }

    public static void Kill()
    {
        AudioSource[] srcs = instance.GetComponents<AudioSource>();
        foreach (AudioSource src in srcs)
            Destroy(src);
    }

    public static IEnumerator DespawnSource(float clipLength, AudioSource source)
    {
        yield return new WaitForSeconds(clipLength);
        Destroy(source);
    }
}
using System.Collections;
using UnityEngine.Audio;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;
	public AudioMixer audioMixer;
	public Sound[] sounds;
	
	// Duration of the volume fade
	public float fadeDuration = 200.0f;

	// Target volume levels
	public float quietVolume = -80.0f;  // Adjust to desired quiet volume
	public float loudVolume = 0.0f;     // Adjust to desired loud volume

	// Coroutine for the volume fade
	private Coroutine _fadeCoroutine;

	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;
			s.source.outputAudioMixerGroup = s.mixerGroup;
		}
		
		//Play("music_waiting");
		//FadeMusicVolume(false);
	}
	
	/// <summary>
	/// searches for all sounds starting with the string argument and plays one randomly
	/// </summary>
	/// <param name="sound">sound name starts with string ("pig_" for pig_1,pig_2,pig_3) </param>
	public void Play(string sound)
	{
		Sound[] matchingSounds = sounds.Where(item => item.name.StartsWith(sound)).ToArray();
		if (matchingSounds.Length == 0)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		
		Sound s = matchingSounds[UnityEngine.Random.Range(0, matchingSounds.Length)];
		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		s.source.Play();
	}

	public void FadeMusicVolume(bool fadeIn)
	{
		if (_fadeCoroutine != null)
		{
			StopCoroutine(_fadeCoroutine);
			_fadeCoroutine = null;
		}

		float targetVolume = fadeIn ? loudVolume : quietVolume;
		
		_fadeCoroutine = StartCoroutine(FadeMusicVolumeCoroutine(audioMixer, targetVolume, fadeDuration));
	}

	private IEnumerator FadeMusicVolumeCoroutine(AudioMixer audioGroup, float targetVolume, float duration)
	{
		float initialVolume;
		audioMixer.GetFloat("MusicVolume", out initialVolume);

		float volumeIncrement = (targetVolume - initialVolume) / duration;

		float elapsedTime = 0.0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;

			float newVolume = initialVolume + volumeIncrement * elapsedTime;
			
			audioMixer.SetFloat("MusicVolume", newVolume);

			yield return null;
		}

		audioMixer.SetFloat("MusicVolume", targetVolume);

		_fadeCoroutine = null;
	}

}

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volumes")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.35f;
    [Range(0f, 1f)] public float sfxVolume = 0.85f;

    [Header("Fade de música")]
    public float musicFadeDuration = 0.45f;

    [Header("Músicas")]
    public AudioClip musicMenu;
    public AudioClip musicFase1;
    public AudioClip musicFase2;
    public AudioClip musicFase3;
    public AudioClip musicFinal;

    [Header("SFX - UI")]
    public AudioClip uiClick;
    public AudioClip uiPause;

    [Header("SFX - Combate")]
    public AudioClip attackLight;
    public AudioClip attackHeavy;
    public AudioClip hitEnemy;
    public AudioClip hitPlayer;
    public AudioClip enemyDeath;

    [Header("SFX - Skills")]
    public AudioClip skill1Impact;
    public AudioClip skill2Dash;
    public AudioClip skill3Aura;
    public AudioClip ultimate;

    [Header("SFX - Pickups")]
    public AudioClip pickupGeneric;
    public AudioClip pickupHeal;
    public AudioClip pickupAttack;
    public AudioClip pickupDefense;

    [Header("SFX - Progressão e telas")]
    public AudioClip levelUp;
    public AudioClip victory;
    public AudioClip defeat;

    private Coroutine musicFadeRoutine;
    private AudioClip currentMusic;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    public void PlayMusicForScene(string sceneName)
    {
        AudioClip targetMusic = null;

        switch (sceneName)
        {
            case "Menu":
                targetMusic = musicMenu;
                break;

            case "Fase1":
                targetMusic = musicFase1;
                break;

            case "Fase2":
                targetMusic = musicFase2;
                break;

            case "Fase3":
                targetMusic = musicFase3;
                break;

            case "TelaFinal":
                targetMusic = musicFinal;
                break;
        }

        PlayMusic(targetMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (currentMusic == clip && musicSource.isPlaying)
            return;

        currentMusic = clip;

        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
        }

        musicFadeRoutine = StartCoroutine(FadeToMusic(clip));
    }

    IEnumerator FadeToMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / musicFadeDuration);
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        elapsed = 0f;
        float targetVolume = masterVolume * musicVolume;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / musicFadeDuration);
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicFadeRoutine = null;
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null || sfxSource == null)
            return;

        float finalVolume = masterVolume * sfxVolume * volumeMultiplier;
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    public void PlayUIClick()
    {
        PlaySFX(uiClick, 0.85f);
    }

    public void PlayUIPause()
    {
        PlaySFX(uiPause, 0.9f);
    }

    public void PlayAttackLight()
    {
        PlaySFX(attackLight, 0.9f);
    }

    public void PlayAttackHeavy()
    {
        PlaySFX(attackHeavy, 1f);
    }

    public void PlayHitEnemy()
    {
        PlaySFX(hitEnemy, 0.75f);
    }

    public void PlayHitPlayer()
    {
        PlaySFX(hitPlayer, 0.9f);
    }

    public void PlayEnemyDeath()
    {
        PlaySFX(enemyDeath, 0.9f);
    }

    public void PlaySkill1Impact()
    {
        PlaySFX(skill1Impact, 1f);
    }

    public void PlaySkill2Dash()
    {
        PlaySFX(skill2Dash, 0.9f);
    }

    public void PlaySkill3Aura()
    {
        PlaySFX(skill3Aura, 1f);
    }

    public void PlayUltimate()
    {
        PlaySFX(ultimate, 1f);
    }

    public void PlayPickupHeal()
    {
        PlaySFX(pickupHeal != null ? pickupHeal : pickupGeneric, 0.9f);
    }

    public void PlayPickupAttack()
    {
        PlaySFX(pickupAttack != null ? pickupAttack : pickupGeneric, 0.9f);
    }

    public void PlayPickupDefense()
    {
        PlaySFX(pickupDefense != null ? pickupDefense : pickupGeneric, 0.9f);
    }

    public void PlayPickupGeneric()
    {
        PlaySFX(pickupGeneric, 0.9f);
    }

    public void PlayLevelUp()
    {
        PlaySFX(levelUp, 1f);
    }

    public void PlayVictory()
    {
        PlaySFX(victory, 1f);
    }

    public void PlayDefeat()
    {
        PlaySFX(defeat, 1f);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }

    void ApplyMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }
    }
}
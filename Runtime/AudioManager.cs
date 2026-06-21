using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Playlist")]
    public List<AudioClip> mainMenuMusic = new List<AudioClip>();
    public bool loopMainMenuMusic = true;
    [Range(0f, 1f)] public float mainMenuMusicVolume = 0.5f;

    [Header("UI SFX Clips")]
    public AudioClip clickSFX;
    [Range(0f, 1f)] public float clickSFXVolume = 0.8f;

    public AudioClip toggleSFX;
    [Range(0f, 1f)] public float toggleSFXVolume = 0.8f;

    public AudioClip panelChangeSFX;
    [Range(0f, 1f)] public float panelChangeSFXVolume = 0.8f;

    public AudioClip navigateSFX;
    [Range(0f, 1f)] public float navigateSFXVolume = 0.6f;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private int _currentMusicIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
    }

    private void Start()
    {
        UpdateVolumes();

        if (mainMenuMusic.Count > 0)
        {
            PlayMusicAtIndex(0);
        }

        // Dynamically find UIDocument root and bind listeners
        var uiDoc = GetComponent<UIDocument>() ?? FindFirstObjectByType<UIDocument>();
        if (uiDoc != null && uiDoc.rootVisualElement != null)
        {
            HookUIEvents(uiDoc.rootVisualElement);
        }
    }

    private void Update()
    {
        if (_musicSource != null && !_musicSource.isPlaying && mainMenuMusic.Count > 0)
        {
            int nextIndex = _currentMusicIndex + 1;
            if (nextIndex >= mainMenuMusic.Count)
            {
                if (loopMainMenuMusic)
                {
                    nextIndex = 0;
                }
                else
                {
                    nextIndex = -1;
                }
            }

            if (nextIndex >= 0)
            {
                PlayMusicAtIndex(nextIndex);
            }
        }
    }

    public void PlayMusicAtIndex(int index)
    {
        if (index < 0 || index >= mainMenuMusic.Count) return;
        _currentMusicIndex = index;

        float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);

        _musicSource.clip = mainMenuMusic[index];
        _musicSource.volume = mainMenuMusicVolume * music * master;
        _musicSource.Play();
    }

    public void PlayClickSFX()
    {
        if (clickSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            _sfxSource.PlayOneShot(clickSFX, clickSFXVolume * sfx * master);
        }
    }

    public void PlayToggleSFX()
    {
        if (toggleSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            _sfxSource.PlayOneShot(toggleSFX, toggleSFXVolume * sfx * master);
        }
    }

    public void PlayPanelChangeSFX()
    {
        if (panelChangeSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            _sfxSource.PlayOneShot(panelChangeSFX, panelChangeSFXVolume * sfx * master);
        }
    }

    public void PlayNavigateSFX()
    {
        if (navigateSFX != null && _sfxSource != null)
        {
            float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
            _sfxSource.PlayOneShot(navigateSFX, navigateSFXVolume * sfx * master);
        }
    }

    public void HookUIEvents(VisualElement root)
    {
        if (root == null) return;

        // Buttons
        root.Query<Button>().ForEach(btn =>
        {
            btn.RegisterCallback<ClickEvent>(evt => PlayClickSFX());
            btn.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });

        // Toggles
        root.Query<Toggle>().ForEach(tgl =>
        {
            tgl.RegisterCallback<ChangeEvent<bool>>(evt => PlayToggleSFX());
            tgl.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });

        // Sliders
        root.Query<Slider>().ForEach(sld =>
        {
            // Small tick audio on value change
            sld.RegisterCallback<ChangeEvent<float>>(evt => PlayClickSFX());
            sld.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });

        // Dropdowns
        root.Query<DropdownField>().ForEach(drp =>
        {
            drp.RegisterCallback<ChangeEvent<string>>(evt => PlayClickSFX());
            drp.RegisterCallback<FocusEvent>(evt => PlayNavigateSFX());
        });
    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        UpdateVolumes();
    }

    public void UpdateVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);

        if (_musicSource != null)
        {
            _musicSource.volume = mainMenuMusicVolume * music * master;
        }
    }
}

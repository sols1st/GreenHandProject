using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 简单音频管理器 - 只处理背景音乐和单次音效
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    public AudioSource musicSource;  // 背景音乐播放器
    public AudioSource sfxSource;   // 音效播放器

    [Header("背景音乐")]
    [SerializeField] private AudioEntry[] backgroundMusics; // 背景音乐数组

    [Header("音效")]
    [SerializeField] private AudioEntry[] soundEffects;    // 音效数组

    [System.Serializable]
    public class AudioEntry
    {
        public string key;
        public AudioClip clip;
    }
    

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

    /// <summary>
    /// 播放指定key的背景音乐
    /// </summary>
    public void PlayBackgroundMusic(string key)
    {
        // 如果key为空、null，停止背景音乐
        if (string.IsNullOrEmpty(key))
        {
            musicSource.Stop();
            return;
        }

        foreach (var entry in backgroundMusics)
        {
            if (entry.key == key)
            {
                musicSource.Stop();
                musicSource.clip = entry.clip;
                musicSource.Play();
                return;
            }
        }

        // 如果没有找到对应的key，停止背景音乐
        musicSource.Stop();
    }

    /// <summary>
    /// 播放指定key的音效
    /// </summary>
    public void PlaySoundEffect(string key)
    {
        foreach (var entry in soundEffects)
        {
            if (entry.key == key)
            {
                sfxSource.PlayOneShot(entry.clip);
                return;
            }
        }
    }
    
}

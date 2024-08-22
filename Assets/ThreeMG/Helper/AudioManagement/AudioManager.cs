using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.VisualScripting;
using UnityEngine;

namespace ThreeMG.Helper.AudioManagement
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        public SerializedDictionary<string, AudioSourceConfig> AudioSourceMap;
        public bool MuteSounds = false;

        private void Awake()
        {
            setAsSingleton();
        }

        private void setAsSingleton()
        {
            gameObject.transform.parent = null;
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            CreateInstances();
        }

        private void CreateInstances()
        {
            foreach (AudioSourceConfig config in AudioSourceMap.Values)
            {
                config.Instances = new List<AudioSource>();

                for (int i = 0; i < config.InstanceCount; i++)
                {
                    AudioSource instance;

                    if (config.InstanceCount == 1)
                    {
                        instance = config.Source;
                    }
                    else
                    {
                        instance = CreateNewInstance(config);
                    }

                    config.Instances.Add(instance);
                }
            }
        }

        private AudioSource CreateNewInstance(AudioSourceConfig config)
        {
            AudioSource instance = this.AddComponent<AudioSource>();
            instance.clip = config.Source.clip;
            instance.pitch = config.Source.pitch;
            instance.volume = config.Source.volume;
            
            instance.outputAudioMixerGroup = config.Source.outputAudioMixerGroup;
            return instance;
        }

        public void PlayWithTag(string tag, bool isLooping = false, float duration = 0f, bool playOneShot = false)
        {
            if (MuteSounds)
            {
                return;
            }

            AudioSourceConfig config;

            AudioSourceMap.TryGetValue(tag, out config);

            if (config == null)
            {
                Debug.LogError($"Cant find config with tag {tag}");
                return;
            }

            AudioSource src;

            if (config.Instances.Count == 0)
            {
                AudioSource instance;
                if (config.CanExpand)
                {
                    instance = CreateNewInstance(config);
                    config.Instances.Add(instance);
                }
                else
                {
                    instance = config.ActiveInstances[0];
                }

                src = instance;
            }
            else
            {
                src = config.Instances[0];
            }

            if (config.Instances.Contains(src))
            {
                config.Instances.Remove(src);
            }

            if (playOneShot)
            {
                src.PlayOneShot(src.clip);
            }
            else
            {
                src.Play();
                if (isLooping)
                {
                    src.loop = true;
                    StartCoroutine(StopLoopingAfterDuration(src, duration));
                }
            }

            if (!config.ActiveInstances.Contains(src))
            {
                config.ActiveInstances.Add(src);
            }

            StartCoroutine(DisableAfterDelay(config, src));
        }

        public IEnumerator StopLoopingAfterDuration(AudioSource src, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            src.loop = false;
        }


        private IEnumerator DisableAfterDelay(AudioSourceConfig config, AudioSource instance)
        {
            yield return new WaitForSeconds(instance.clip.length);
            if (config.ActiveInstances.Contains(instance))
            {
                config.ActiveInstances.Remove(instance);
            }
            if (!config.Instances.Contains(instance))
            {
                config.Instances.Add(instance);
            }
        }

        // public AudioSource SetAudioSource(string tag)
        // {
        //     AudioSource source;
        //     dollAudioSourceDictionary.TryGetValue(tag, out source);

        //     if (source == null)
        //     {
        //         Debug.LogError($"Cant find source with tag {tag}");
        //         return null;
        //     }

        //     return source;
        // }
    }

    [Serializable]
    public class AudioSourceConfig
    {
        public int InstanceCount = 1;
        public AudioSource Source;
        public bool CanExpand;
        public List<AudioSource> Instances;
        public List<AudioSource> ActiveInstances;
    }
}


using System.Collections.Generic;
using System;
using UnityEngine;
/*using SaintsField.Playa;
using SaintsField;*/
using Remedy.Schematics.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Remedy.Audio
{
    public class SoundCue : MonoBehaviour
    {
/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        [Tooltip("The Event that triggers this Sound Cue.")]
        public SignalData Event;
        public SignalData StopEvent;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Audio Selection")]*/
        [Tooltip("The AudioDefinition List to play, received from the AudioData Scriptable Object")]
        //[Dropdown("GetAudio")]
        public string AudioName;
        [Tooltip("Determines how the Audio Manager handles the playback of this Sound Cue. If set to Music, it will loop globally in the Music Mixer, otherwise it will be positioned and played in the SFX Mixer")]
        public AudioType Type = AudioType.SoundEffect;
/*
        public DropdownList<string> GetAudio()
        {
            var list = new DropdownList<string>();

            foreach (var def in AudioData.AudioDefinitions.Keys)
            {
                list.Add(def, def);
            }

            return list;
        }
*/
        public bool IsMusic => Type == AudioType.Music;
        [Tooltip("The Content of the Audio Definitions that have been selected by name in the above Dropdown")]
        public List<AudioDefinition> AudioContent;
        [Tooltip("The way in which Audio Definitions are picked for Playback.")]
        public SelectionMode SelectionMode = SelectionMode.Random;
        //[ShowIf("IsSequence")]
        public float SequenceDelay = 0f;
/*
        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Playback")]*/
        [Tooltip("When an AudioDefinition is played")]
        //[HideIf("IsMusic")]
        public PlaybackMode PlaybackMode = PlaybackMode.OnStart;
        [Tooltip("The target to play the sound at when the Collision is Entered/Exitted.")]
        //[ShowIf("IsCollisionMode")]
        public CollisionTargetMode CollisionPlaybackTarget = CollisionTargetMode.HitPoint;
        public bool IsCollisionMode => (PlaybackMode == PlaybackMode.OnCollisionEnter || PlaybackMode == PlaybackMode.OnCollisionExit) && !IsMusic;

        [Tooltip("The target to play the sound at when the Trigger is Entered/Exitted.")]
        //[ShowIf("IsTriggerMode")]
        public TriggerTargetMode TriggerPlaybackTarget = TriggerTargetMode.Other;
        public bool IsTriggerMode => (PlaybackMode == PlaybackMode.OnTriggerEnter || PlaybackMode == PlaybackMode.OnTriggerExit) && !IsMusic;

        public bool IsScriptableEventMode => (PlaybackMode == PlaybackMode.ScriptableEvent);


        [Tooltip("Multiplied into the Volume of the AudioClip that gets played by this Sound Cue")]
        [Range(0.1f, 1.0f)]
        public float VolumeMultiplier = 1.0f;
        [Tooltip("Whether to Set the Spacial Blend of the Audio played by the Sound Cue")]
        public bool SetSpacialBlend = false;
        [Tooltip("Sets the Spacial Blender of the AudioClip that gets played by this Sound Cue")]
        [Range(0.1f, 1.0f)]
        //[ShowIf("SetSpacialBlend")]
        public float SpacialBlend = 1.0f;
/*
        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Timing")]*/
        [Tooltip("If above 0, will Delay the playback by that amount in seconds.")]
        public float Delay = 0f;
        [Tooltip("When the Audio is told to Play, it will fade in instead of stopping immidiately")]
        public bool FadeIn = false;
        //[ShowIf("FadeIn")]
        public float FadeInTime = 0.1f;
        [Tooltip("When the Audio is told to stop, it will fade out instead of stopping immidiately.")]
        public bool FadeOut = false;
        //[ShowIf("FadeOut")]
        public float FadeOuttime = 0.1f;

        public bool IsSequence => SelectionMode == SelectionMode.Sequential;

        /// <summary>
        /// The AudioSource that was returned from the AudioManager 
        /// </summary>
        private AudioSource _source;

        private void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                AudioContent = AudioData.AudioDefinitions[AudioName];
            };
#endif
        }

        // On Start Playback
        public virtual void Start()
        {
            if (PlaybackMode == PlaybackMode.OnStart || IsMusic)
                Play();

            Event?.Subscribe(this, (Union value) => Play());
            StopEvent?.Subscribe(this, (Union value) => Stop());
        }

        public void Play()
        {
            Play(null);
        }

        public void Stop()
        {
            AudioManager.Stop(gameObject, AudioName);
        }

        protected void Play(GameObject otherObject = null, Vector3 position = default, int index = -1, Action onComplete = null)
        {
            // Handle Selection
            if (SelectionMode == SelectionMode.Simulaneous && index == -1)
            {
                index = 0;
                for (int i = 1; i < AudioContent.Count; i++)
                {
                    Play(otherObject, position, index);
                }
            }
            else if (SelectionMode == SelectionMode.Sequential && index < AudioContent.Count)
            {
                onComplete = () => Play(otherObject, position, index, onComplete);
                index++;
                Play(otherObject, position, index, onComplete);
            }

            //Get definition index and Play through the AudioManager
            if (index == -1) index = UnityEngine.Random.Range(0, AudioContent.Count - 1);
            AudioManager.Play(AudioContent[index], Type, otherObject == null ? gameObject : otherObject, position, onComplete);
        }

        // Collision Playback
        private void OnCollisionEnter(Collision collision)
        {
            if (PlaybackMode == PlaybackMode.OnCollisionEnter)
                PlayCollision(collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            if (PlaybackMode == PlaybackMode.OnCollisionExit)
                PlayCollision(collision);
        }
        void PlayCollision(Collision collision)
        {
            switch (CollisionPlaybackTarget)
            {
                case CollisionTargetMode.HitPoint:
                    Play(position: collision.GetContact(0).point);
                    break;
                case CollisionTargetMode.Other:
                    Play(otherObject: collision.gameObject);
                    break;
                default:
                    Play();
                    break;
            }
        }

        // Trigger Playback
        private void OnTriggerEnter(Collider other)
        {
            if (PlaybackMode == PlaybackMode.OnTriggerEnter)
                PlayTrigger(other);
        }
        private void OnTriggerExit(Collider other)
        {
            if (PlaybackMode == PlaybackMode.OnTriggerExit)
                PlayTrigger(other);
        }
        void PlayTrigger(Collider other)
        {
            switch (TriggerPlaybackTarget)
            {
                case TriggerTargetMode.Other:
                    Play(otherObject: other.gameObject);
                    break;
                default:
                    Play();
                    break;
            }
        }
    }

    public enum AudioType
    {
        Music,
        SoundEffect
    }

    public enum PlaybackMode
    {
        OnStart,
        OnCollisionEnter,
        OnCollisionExit,
        OnTriggerEnter,
        OnTriggerExit,
        External,
        ScriptableEvent
    }
    public enum SelectionMode
    {
        Random,
        Sequential,
        Simulaneous,
        LoopSequentially
    }

    public enum TriggerTargetMode
    {
        This,
        Other
    }

    public enum CollisionTargetMode
    {
        HitPoint,
        This,
        Other
    }

}
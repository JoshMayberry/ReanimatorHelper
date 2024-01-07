using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Aarthificial.Reanimation;
using AYellowpaper.SerializedCollections;

namespace jmayberry.ReanimatorHelper {
	[Serializable]
	public class AudioEventResult {
		internal AudioSource audioSource;
		public List<AudioClip> possibleClips;
		public Action fmodTriggerFunction;

		public void PlayAudio() {
			if (this.fmodTriggerFunction != null) {
				this.fmodTriggerFunction.Invoke();
				return;
			}

			if (this.possibleClips.Count <= 0) {
				Debug.LogWarning("No audio files or an fmod trigger were set");
				return;
			}

			audioSource.PlayOneShot(this.possibleClips[UnityEngine.Random.Range(0, this.possibleClips.Count)]);
		}
	}

	public class ReanimatorPlus : Reanimator {
		[SerializedDictionary("Driver", "Audio Event")] public SerializedDictionary<string, AudioEventResult> audioEventCatalog;
		[SerializedDictionary("Driver", "Listen Event")] public SerializedDictionary<string, ListenEventResult> listenEventCatalog;
		public AudioSource audioSource;

		protected virtual void OnEnable() {
			foreach (var audioEvent in this.audioEventCatalog) {
				audioEvent.Value.audioSource = this.audioSource;
				this.AddListener($"{audioEvent.Key}", audioEvent.Value.PlayAudio);
			}

			foreach (var listenEvent in this.listenEventCatalog) {
				this.AddListener($"{listenEvent.Key}", listenEvent.Value.DoAction);
			}
		}

		protected virtual void OnDisable() {
			foreach (var audioEvent in this.audioEventCatalog) {
				this.RemoveListener($"{audioEvent.Key}", audioEvent.Value.PlayAudio);
			}

			foreach (var listenEvent in this.listenEventCatalog) {
				this.RemoveListener($"{listenEvent.Key}", listenEvent.Value.DoAction);
			}
		}

		public virtual void AddAudio(string key, AudioEventResult audioEvent) {
			audioEvent.audioSource = this.audioSource;
			this.audioEventCatalog.Add(key, audioEvent);
			this.AddListener($"{key}", audioEvent.PlayAudio);
		}

		public virtual void RemoveAudio(string key, AudioEventResult audioEvent) {
			this.audioEventCatalog.Remove(key);
			this.RemoveListener($"{key}", audioEvent.PlayAudio);
		}

		public virtual void AddListen(string key, ListenEventResult listenEvent) {
			this.listenEventCatalog.Add(key, listenEvent);
			this.AddListener($"{key}", listenEvent.DoAction);
		}

		public virtual void RemoveListen(string key, ListenEventResult listenEvent) {
			this.listenEventCatalog.Remove(key);
			this.RemoveListener($"{key}", listenEvent.DoAction);
		}
	}
}
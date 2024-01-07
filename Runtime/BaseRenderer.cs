using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Aarthificial.Reanimation;
using AYellowpaper.SerializedCollections;

namespace jmayberry.ReanimatorHelper {
	[Serializable]
	public class UpdateEventResult {
		public Func<int> getValueFunction;
	}

	[Serializable]
	public class FixedUpdateEventResult {
		public Func<int> getValueFunction;
	}

	[Serializable]
	public class ListenEventResult {
		public Action triggerFunction;

		public void DoAction() {
			this.triggerFunction.Invoke();
		}
	}

	public class BaseRenderer<AudioEvents, ListenEvents, UpdateEvents, FixedUpdateEvents> : MonoBehaviour where AudioEvents : Enum where ListenEvents : Enum where UpdateEvents : Enum where FixedUpdateEvents : Enum {
		[SerializedDictionary("Audio Event", "Result")] public SerializedDictionary<AudioEvents, AudioEventResult> audioEventCatalog;
		[SerializedDictionary("Listen Event", "Result")] public SerializedDictionary<ListenEvents, ListenEventResult> listenEventCatalog;
		[SerializedDictionary("Update Event", "Result")] public SerializedDictionary<UpdateEvents, UpdateEventResult> updateEventCatalog;
		[SerializedDictionary("FixedUpdate Event", "Result")] public SerializedDictionary<FixedUpdateEvents, FixedUpdateEventResult> fixedUpdateEventCatalog;

		public Reanimator reanimator;
		public AudioSource audioSource;

		protected virtual void Awake() {
			foreach (var audioEvent in audioEventCatalog) {
				audioEvent.Value.audioSource = this.audioSource;
			}
		}

		protected virtual void OnEnable() {
			foreach (var audioEvent in audioEventCatalog) {
				reanimator.AddListener($"{audioEvent.Key}", audioEvent.Value.PlayAudio);
			}

			foreach (var listenEvent in listenEventCatalog) {
				reanimator.AddListener($"{listenEvent.Key}", listenEvent.Value.DoAction);
			}
		}

		protected virtual void OnDisable() {
			foreach (var audioEvent in audioEventCatalog) {
				reanimator.RemoveListener($"{audioEvent.Key}", audioEvent.Value.PlayAudio);
			}

			foreach (var listenEvent in listenEventCatalog) {
				reanimator.RemoveListener($"{listenEvent.Key}", listenEvent.Value.DoAction);
			}
		}

		//protected virtual void Update() {
		//	foreach (var updateEvent in updateEventCatalog) {
		//		this.reanimator.Set($"{updateEvent.Key}", updateEvent.Value.getValueFunction());
		//	}
		//}

		//protected virtual void FixedUpdate() {
		//	foreach (var fixedUpdateEvent in fixedUpdateEventCatalog) {
		//		this.reanimator.Set($"{fixedUpdateEvent.Key}", fixedUpdateEvent.Value.getValueFunction());
		//	}
		//}

		public void SetKey(string key, int value) {
			this.reanimator.Set(key, value);
		}
	}
}
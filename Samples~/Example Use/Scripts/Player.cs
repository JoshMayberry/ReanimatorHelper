using UnityEngine;
using UnityEngine.InputSystem;
using jmayberry.CustomAttributes;

namespace jmayberry.ReanimatorHelper.Samples.ExampleUse {
	public class Player : MonoBehaviour {
		[Header("Setup")]
		[Required] public ReanimatorPlus reanimator;

		[Header("Physics")]
		public float speed = 5.0f;

		[Header("Input")]
		private Vector2 inputMove;
		private float lastDoActionPressed;
		[Readonly] public bool isDoActionPressed;
		[SerializeField] private float doActionGraceTime = 0.1f;

		void Start() {
			reanimator.Set("twist_type", 0);
		}

		void Update() {
			Vector3 direction = this.inputMove.normalized;
			transform.position += speed * Time.deltaTime * direction;
		}

		void OnMove(InputValue context) {
			reanimator.Set("state", 0);
			this.inputMove = context.Get<Vector2>();

			if (this.inputMove.x < 0) {
				reanimator.Set("move_direction", 1); // Right
			}
			else if (this.inputMove.x > 0) {
				reanimator.Set("move_direction", 2); // Left
			}
			else if (this.inputMove.y > 0) {
				reanimator.Set("move_direction", 3); // Up
			}
			else if (this.inputMove.y < 0) {
				reanimator.Set("move_direction", 4); // Down
			}
			else {
				reanimator.Set("move_direction", 0); // Idle
			}
		}

		void OnTwistNormal() {
			reanimator.Set("twist_type", 0);
			reanimator.Set("state", 2);
		}

		void OnTwistRock() {
			reanimator.Set("twist_type", 1);
			reanimator.Set("state", 2);
		}

		void OnTwistPaper() {
			reanimator.Set("twist_type", 2);
			reanimator.Set("state", 2);
        }

        void OnTwistScissor() {
            reanimator.Set("twist_type", 3);
            reanimator.Set("state", 2);
        }

        void OnDoActionPress() {
			reanimator.Set("state_action", 0);
			reanimator.Set("state", 3);
        }

        void OnDoActionRelease() {
			reanimator.Set("state_action", 2);
		}
	}
}
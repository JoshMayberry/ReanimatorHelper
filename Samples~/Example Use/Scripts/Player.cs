using UnityEngine;

namespace jmayberry.ReanimatorHelper.Samples.ExampleUse {
	public class Player : MonoBehaviour {
		public float speed = 5.0f;

		void Update() {
			Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0).normalized;
			transform.position += speed * Time.deltaTime * direction;
		}
	}
}
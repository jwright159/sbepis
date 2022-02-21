using UnityEngine;

namespace SBEPIS.Interaction.VR
{
	[RequireComponent(typeof(Rigidbody))]
	public class VelocityJoint : MonoBehaviour
	{
		public Transform connectionPoint;
		public float velocityFactor = 10;
		public float angularVelocityFactor = 10;

		private new Rigidbody rigidbody;

		private void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			rigidbody.velocity = (connectionPoint.position - rigidbody.position) * velocityFactor;

			(connectionPoint.rotation * Quaternion.Inverse(rigidbody.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
			if (angle > 180) angle -= 360;
			rigidbody.angularVelocity = angle * axis * angularVelocityFactor;
		}
	}
}

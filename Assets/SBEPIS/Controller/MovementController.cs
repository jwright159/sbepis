using UnityEngine;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

namespace SBEPIS.Controller
{
	[RequireComponent(typeof(Rigidbody), typeof(Orientation))]
	public class MovementController : MonoBehaviour
	{
		public Transform moveAimer;
		public float maxGroundSpeed = 8;
		public float groundAcceleration = 10;
		public float airAcceleration = 1;
		public float frictionDeceleration = 1;
		public float sprintFactor = 2;
		public float sprintControlThreshold = 0.9f;

		private new Rigidbody rigidbody;
		private Orientation orientation;
		private Vector3 controlsTarget;
		private bool isTryingToSprint;
		private bool isSprinting;

		private void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();
			orientation = GetComponent<Orientation>();
		}

		private void FixedUpdate()
		{
			MoveTick();
		}

		private void MoveTick()
		{
			CheckSprint();

			// if the player is moving controls and is not grounded, accelerate in that direction until we are moving faster than the max
			// if the player is moving controls and is grounded, accelerate, probably apply friction in not the direction that the player is going
			// if the player is not moving controls and the player is not grounded, do nothing
			// if the player is not moving controls and the player is grounded, apply friction
			
			Accelerate(orientation.relativeVelocity, orientation.groundVelocity, orientation.upDirection);
			ApplyFriction(orientation.groundVelocity);
		}

		private void CheckSprint()
		{
			if (isSprinting && controlsTarget.magnitude < sprintControlThreshold)
				isSprinting = false;
			else if (!isSprinting && isTryingToSprint && controlsTarget.magnitude > sprintControlThreshold)
				isSprinting = true;
		}

		private void Accelerate(Vector3 velocity, Vector3 groundVelocity, Vector3 upDirection)
		{
			if (controlsTarget != Vector3.zero)
			{
				Vector3 controlledVelocity = orientation.isGrounded ? velocity : groundVelocity;
				Vector3 accelerationDirection = moveAimer.right * controlsTarget.x + Vector3.Cross(moveAimer.right, upDirection) * controlsTarget.z;
				float maxSpeed = Mathf.Max(controlledVelocity.magnitude, maxGroundSpeed * accelerationDirection.magnitude * (isSprinting ? sprintFactor : 1));
				Vector3 newVelocity = controlledVelocity + Time.fixedDeltaTime * (orientation.isGrounded ? groundAcceleration : airAcceleration) * accelerationDirection.normalized;
				Vector3 clampedNewVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
				AddVelocityAgainstGround(rigidbody, clampedNewVelocity - controlledVelocity, orientation.groundRigidbody);
			}
		}

		private void ApplyFriction(Vector3 groundVelocity)
		{
			if (controlsTarget == Vector3.zero && orientation.isGrounded)
				if (groundVelocity.magnitude < frictionDeceleration * Time.fixedDeltaTime)
					AddVelocityAgainstGround(rigidbody, -groundVelocity, null);
				else
					AddVelocityAgainstGround(rigidbody, -frictionDeceleration * Time.fixedDeltaTime * groundVelocity.normalized, null);
		}

		public void OnMove(CallbackContext context)
		{
			Vector2 controls = context.ReadValue<Vector2>();
			controlsTarget = new Vector3(controls.x, 0, controls.y);
		}

		public void OnSprint(CallbackContext context)
		{
			isTryingToSprint = context.performed;
		}

		public static void AddVelocityAgainstGround(Rigidbody rigidbody, Vector3 velocity, Rigidbody ground)
		{
			rigidbody.velocity += velocity;
			if (ground)
				//jumpPlatform.AddForceAtPosition(-velocity * rigidbody.mass, groundDetector.groundCheck.position + Vector3.down * 0.5f, ForceMode.Impulse);
				ground.AddForce(-velocity * rigidbody.mass, ForceMode.Impulse);
		}
	}
}
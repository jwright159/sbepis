using SBEPIS.Controller;
using SBEPIS.Physics;
using UnityEngine;

namespace SBEPIS.Items
{
	[RequireComponent(typeof(CompoundRigidbody), typeof(Grabbable), typeof(ItemBase))]
	[RequireComponent(typeof(GravitySum))]
	public class Item : MonoBehaviour
	{
		public new CompoundRigidbody rigidbody { get; private set; }
		public ItemBase itemBase { get; private set; }

		private void Awake()
		{
			rigidbody = GetComponent<CompoundRigidbody>();
			itemBase = GetComponent<ItemBase>();
		}
	}
}

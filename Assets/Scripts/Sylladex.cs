using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WrightWay.SBEPIS.Modus;
using WrightWay.SBEPIS.Player;

namespace WrightWay.SBEPIS
{
	public class Sylladex : MonoBehaviour
	{
		[SerializeField]
		private Animator animator;
		[SerializeField]
		private float ejectionSpeed = 6f;
		[SerializeField]
		private Transform insertParent, retrieveParent;

		public bool isWorking => animator.GetInteger("InsertState") != 0 || animator.GetInteger("RetrieveState") != 0;
		private FetchModus modus = new StackModus(null);

		public void Captchalogue(Item item, SylladexOwner owner)
		{
			Eject(modus.Insert(item), owner);

			foreach (Transform oldDisplay in insertParent)
			{
				oldDisplay.gameObject.SetActive(false);
				oldDisplay.SetParent(transform);
			}

			CaptchalogueCard newDisplay = modus.Display();
			newDisplay.gameObject.SetActive(true);
			newDisplay.transform.SetParent(insertParent);
			newDisplay.transform.localPosition = Vector3.zero;
			newDisplay.transform.localRotation = Quaternion.identity;
		}

		public Item Display()
		{
			CaptchalogueCard card = modus.Display();
			if (card)
				return card.heldItem;
			return null;
		}

		public void Retrieve(SylladexOwner owner)
		{
			Eject(modus.Retrieve(), owner);
		}

		private void Eject(Item item, SylladexOwner owner)
		{
			if (!item)
				return;

			item.gameObject.SetActive(true);
			item.transform.SetParent(null);
			Quaternion ejectionAngle = Quaternion.AngleAxis(Random.value * 360, owner.transform.up);
			item.transform.position = owner.transform.position + ejectionAngle * Vector3.forward;
			item.transform.rotation = ejectionAngle;
			item.rigidbody.velocity = ejectionAngle * Vector3.forward * ejectionSpeed;
			item.rigidbody.angularVelocity = Vector3.zero;
		}
		
		public void InsertCard(CaptchalogueCard card)
		{
			if (modus.cards.Count != 0 && modus.Display().heldItem && card.heldItem && modus.flippedInsert)
			{
				card.transform.SetParent(retrieveParent);
				animator.SetInteger("RetrieveState", 1);
			}
			else
			{
				foreach (Transform oldDisplay in insertParent)
				{
					oldDisplay.transform.SetParent(retrieveParent);
					oldDisplay.transform.localPosition = Vector3.zero;
					oldDisplay.transform.localRotation = Quaternion.identity;
				}

				card.transform.SetParent(insertParent);
				animator.SetInteger("InsertState", 1);
			}

			card.transform.localPosition = Vector3.zero;
			card.transform.localRotation = Quaternion.identity;
			modus.InsertCard(card);
			card.OnInserted();
		}

		public CaptchalogueCard RetrieveCard()
		{
			if (modus.cards.Count == 0)
				return null;

			CaptchalogueCard card = modus.RetrieveCard();

			if (modus.flippedRetrieve)
			{
				foreach (Transform oldDisplay in insertParent) // Largely affects blank card order
				{
					oldDisplay.gameObject.SetActive(false);
					oldDisplay.transform.SetParent(transform);
				}

				CaptchalogueCard newDisplay = modus.Display();
				if (newDisplay)
				{
					newDisplay.gameObject.SetActive(true);
					newDisplay.transform.SetParent(retrieveParent);
					newDisplay.transform.localPosition = Vector3.zero;
					newDisplay.transform.localRotation = Quaternion.identity;
				}

				card.gameObject.SetActive(true);
				card.transform.SetParent(insertParent);
				animator.SetInteger("InsertState", -1);
			}
			else
			{
				card.transform.SetParent(retrieveParent);
				animator.SetInteger("RetrieveState", -1);
			}

			card.transform.localPosition = Vector3.zero;
			card.transform.localRotation = Quaternion.identity;
			return card;
		}

		public void StopInsertDepositing()
		{
			animator.SetInteger("InsertState", 0);
			foreach (Transform child in retrieveParent)
			{
				child.gameObject.SetActive(false);
				child.SetParent(transform);
			}
		}

		public void StopInsertWithdrawing()
		{
			animator.SetInteger("InsertState", 0);
			foreach (Transform child in insertParent)
			{
				child.SetParent(null);
				child.GetComponent<CaptchalogueCard>().OnRetrieved();
			}
			foreach (Transform child in retrieveParent)
			{
				child.SetParent(insertParent, false);
			}
		}

		public void StopRetrieveDepositing()
		{
			animator.SetInteger("RetrieveState", 0);
			foreach (Transform child in retrieveParent)
			{
				child.gameObject.SetActive(false);
				child.SetParent(transform);
			}
		}

		public void StopRetrieveWithdrawing()
		{
			animator.SetInteger("RetrieveState", 0);
			foreach (Transform child in retrieveParent)
			{
				child.SetParent(null);
				child.GetComponent<CaptchalogueCard>().OnRetrieved();
			}
		}
	}
}
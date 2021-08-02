using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using WrightWay.SBEPIS.Util;

namespace WrightWay.SBEPIS.Player
{
	public class SylladexOwner : MonoBehaviour
	{
		public Sylladex sylladex;
		public Transform sylladexParent;
		public ItemHolder itemHolder;
		public PlayerModeSwapper modeSwapper;
		public PlayerInput playerInput;

		private Item retrievingItem;
		private bool _isViewing;
		private bool IsViewing
		{
			get => _isViewing;
			set
			{
				_isViewing = value;
				if (_isViewing)
				{
					playerInput.actions.FindAction("Pick Up").Disable();
					playerInput.actions.FindAction("Captchalogue Mode").Enable();
					sylladexParentRotTarget = Quaternion.identity;
					sylladexParentScaleTarget = Vector3.one;
				}
				else
				{
					playerInput.actions.FindAction("Pick Up").Enable();
					playerInput.actions.FindAction("Captchalogue Mode").Disable();
					sylladexParentRotTarget = Quaternion.Euler(0, -90, 0);
					sylladexParentScaleTarget = Vector3.zero;
				}
			}
		}
		private bool _canCaptchalogue;
		private bool CanCaptchalogue
		{
			get => _canCaptchalogue;
			set
			{
				_canCaptchalogue = value;
				if (_canCaptchalogue)
				{
					playerInput.actions.FindAction("Captchalogue").Enable();
					playerInput.actions.FindAction("Captchalogue Use").Enable();
					sylladexPosTarget = new Vector3(0, -0.25f, 1);
					sylladexRotTarget = Quaternion.Euler(-90, 180, 0);
				}
				else
				{
					playerInput.actions.FindAction("Captchalogue").Disable();
					playerInput.actions.FindAction("Captchalogue Use").Disable();
					sylladexPosTarget = new Vector3(0, 0, 1);
					sylladexRotTarget = Quaternion.Euler(0, 180, 0);
				}
			}
		}
		private Vector3 sylladexPosTarget;
		private Vector3 sylladexPosTargetVel;
		private Quaternion sylladexRotTarget;
		private Quaternion sylladexRotDeriv;
		private Quaternion sylladexParentRotTarget;
		private Quaternion sylladexParentRotDeriv;
		private Vector3 sylladexParentScaleTarget;
		private Vector3 sylladexParentScaleVel;

		private void Start()
		{
			IsViewing = false;
			CanCaptchalogue = false;
		}

		private void Update()
		{
			sylladex.transform.localPosition = Vector3.SmoothDamp(sylladex.transform.localPosition, sylladexPosTarget, ref sylladexPosTargetVel, 0.1f);
			sylladex.transform.localRotation = QuaternionUtil.SmoothDamp(sylladex.transform.localRotation, sylladexRotTarget, ref sylladexRotDeriv, 0.1f);
			sylladexParent.localRotation = QuaternionUtil.SmoothDamp(sylladexParent.localRotation, sylladexParentRotTarget, ref sylladexParentRotDeriv, 0.2f);
			sylladexParent.localScale = Vector3.SmoothDamp(sylladexParent.localScale, sylladexParentScaleTarget, ref sylladexParentScaleVel, 0.1f);

			if (retrievingItem)
				itemHolder.UpdateItem(retrievingItem);
		}

		private void OnViewSylladex()
		{
			if (sylladex.isWorking)
				return;

			IsViewing = !IsViewing;
		}

		private void OnCaptchalogueMode(InputValue value)
		{
			CanCaptchalogue = value.isPressed;
		}

		private void OnCaptchalogue()
		{
			Item hitItem;
			if (Physics.Raycast(itemHolder.camera.position, itemHolder.camera.forward, out RaycastHit captchaHit, itemHolder.maxDistance) && captchaHit.rigidbody && (hitItem = captchaHit.rigidbody.GetComponent<Item>()))
				sylladex.Captchalogue(hitItem, this);
		}

		private void OnRetrieve(InputValue value)
		{
			if (value.isPressed && (retrievingItem = sylladex.Display()))
			{
				retrievingItem.gameObject.SetActive(true);
				retrievingItem.transform.SetParent(null);
			}
			else if (retrievingItem)
			{
				retrievingItem.gameObject.SetActive(false);
				retrievingItem.transform.SetParent(sylladex.transform, this);
				retrievingItem = null;
			}
		}

		private void OnCaptchalogueUse()
		{
			if (retrievingItem)
			{
				sylladex.Retrieve(this);
				retrievingItem = null;
				return;
			}

			if (sylladex.isWorking)
				return;

			CaptchalogueCard hitCard;
			if (Physics.Raycast(itemHolder.camera.position, itemHolder.camera.forward, out RaycastHit captchaHit, itemHolder.maxDistance) && captchaHit.rigidbody && (hitCard = captchaHit.rigidbody.GetComponent<CaptchalogueCard>()) && hitCard.punchedHash == 0)
				sylladex.InsertCard(hitCard);
		}

		private void OnCaptchaloguePrint()
		{
			if (sylladex.isWorking)
				return;

			sylladex.RetrieveCard();
		}
	}
}
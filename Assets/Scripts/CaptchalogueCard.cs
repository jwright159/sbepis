using System;
using UnityEngine;

public class CaptchalogueCard : Item
{
	public Material iconMaterial;
	public Material captchaMaterial;
	public Renderer[] renderers;
	public SkinnedMeshRenderer holeCaps;

	private Quaternion forceFlip = Quaternion.identity;
	private Quaternion upRot;
	private Quaternion downRot;

	public Item heldItem { get; private set; }
	public long punchedHash { get; private set; }

	private void Start()
	{
		UpdateMaterials(0, null);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!gameObject.activeInHierarchy || !collision.gameObject.activeInHierarchy)
			return;

		Item collisionItem = collision.gameObject.GetComponent<Item>();
		if (punchedHash == 0 && !heldItem && collisionItem)
		{
			if (collisionItem.isMouseDown)
				collisionItem.OnMouseUp();
			Captchalogue(collisionItem);
		}
	}

	public override void OnMouseDrag()
	{
		if (Input.GetAxis("Mouse ScrollWheel") < 0)
			holdDistance = 1;
		else if (Input.GetAxis("Mouse ScrollWheel") > 0)
			holdDistance = 2;

		base.OnMouseDrag();

		if (Input.GetMouseButtonDown(1) && heldItem)
			Eject();

		Quaternion lookRot = Quaternion.LookRotation(GameManager.instance.player.camera.transform.position - transform.position, GameManager.instance.player.camera.transform.up);
		upRot = lookRot * Quaternion.Euler(0, 180, 0);
		downRot = lookRot;

		if (Input.GetMouseButtonDown(2))
			if (forceFlip == Quaternion.identity)
				forceFlip = Quaternion.Angle(transform.rotation, upRot) > 90 ? upRot : downRot;
			else
				forceFlip = forceFlip == downRot ? upRot : downRot;
		if (forceFlip != Quaternion.identity && Quaternion.Angle(transform.rotation, forceFlip) < 90)
			forceFlip = Quaternion.identity;

		Quaternion deriv = QuaternionUtil.AngVelToDeriv(transform.rotation, rigidbody.angularVelocity);
		if (forceFlip == Quaternion.identity)
			transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, Quaternion.Angle(transform.rotation, upRot) < 90 ? upRot : downRot, ref deriv, 0.2f);
		else
			transform.rotation = QuaternionUtil.SmoothDamp(transform.rotation, forceFlip, ref deriv, 0.2f);
		rigidbody.angularVelocity = QuaternionUtil.DerivToAngVel(transform.rotation, deriv);
	}

	public override void OnMouseUp()
	{
		base.OnMouseUp();
		forceFlip = Quaternion.identity;
	}

	public void Captchalogue(Item item)
	{
		if (heldItem)
			Eject();

		heldItem = item;
		item.transform.SetParent(transform);
		item.gameObject.SetActive(false);

		UpdateMaterials(item.itemType.captchaHash, GameManager.instance.captcharoid.Captcha(item));
	}

	public void Eject()
	{
		if (!heldItem)
			return;

		heldItem.transform.SetParent(null);
		heldItem.transform.position = transform.position + transform.forward;
		heldItem.transform.rotation = transform.rotation;
		heldItem.rigidbody.velocity = transform.forward * 6 + rigidbody.velocity;
		heldItem.rigidbody.angularVelocity = Vector3.zero;
		SetLayerRecursively(heldItem.gameObject, LayerMask.NameToLayer("Default"));
		heldItem.gameObject.SetActive(true);
		heldItem = null;

		UpdateMaterials(0, null);
	}

	private void UpdateMaterials(long captchaHash, Texture2D icon)
	{
		float seed = 0;
		if (captchaHash != 0)
			for (int i = 0; i < 8; i++)
				seed += Mathf.Pow(10f, i - 4) * ((captchaHash >> 6 * i) & ((1L << 6) - 1));

		foreach (Renderer renderer in renderers)
			for (int i = 0; i < renderer.materials.Length; i++)
			{
				string materialName = renderer.materials[i].name.Replace(" (Instance)", "");
				if (materialName == iconMaterial.name)
				{
					if (!icon)
						Destroy(renderer.materials[i].mainTexture);
					renderer.materials[i].mainTexture = icon;
				}
				else if (materialName == captchaMaterial.name)
				{
					renderer.materials[i].SetFloat("Seed", seed);
					renderer.materials[i].SetTexture("CaptchaCode", ItemType.GetCaptchaTexture(captchaHash));
				}
			}
	}

	public void Punch(long captchaHash)
	{
		this.punchedHash = captchaHash;

		for (int i = 0; i < 48; i++)
			holeCaps.SetBlendShapeWeight(i, Math.Min(punchedHash & (1L << i), 1) * 100);
	}
}

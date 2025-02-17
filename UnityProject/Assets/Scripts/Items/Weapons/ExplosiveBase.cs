﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Mirror;
using Communications;
using Items.Devices;
using Managers;
using Objects;
using Systems.Explosions;
using Scripts.Core.Transform;
using UI.Items;

namespace Items.Weapons
{
	/// <summary>
	/// The base script for holding all universal data and functions between explosives.
	/// Interactions are written separately.
	/// </summary>
	public class ExplosiveBase : SignalReceiver
	{
		[Header("Explosive settings")]
		[SerializeField] protected ExplosiveType explosiveType;
		[SerializeField] protected bool detonateImmediatelyOnSignal;
		[SerializeField] protected int timeToDetonate = 10;
		[SerializeField] protected int minimumTimeToDetonate = 10;
		[SerializeField] protected float explosiveStrength = 150f;
		[SerializeField] protected SpriteDataSO activeSpriteSO;
		[SerializeField] protected float progressTime = 3f;
		[Header("Explosive Components")]
		[SerializeField] protected SpriteHandler spriteHandler;
		[SerializeField] protected ScaleSync scaleSync;
		protected RegisterItem registerItem;
		protected ObjectBehaviour objectBehaviour;
		protected Pickupable pickupable;
		protected HasNetworkTabItem explosiveGUI;
		[HideInInspector] public GUI_Explosive GUI;
		[SyncVar] protected bool isArmed;
		[SyncVar] protected bool countDownActive = false;

		public int MinimumTimeToDetonate => minimumTimeToDetonate;
		public bool DetonateImmediatelyOnSignal => detonateImmediatelyOnSignal;
		public bool CountDownActive => countDownActive;
		public ExplosiveType ExplosiveType => explosiveType;

		public int TimeToDetonate
		{
			get => timeToDetonate;
			set => timeToDetonate = value;
		}

		public bool IsArmed
		{
			get => isArmed;
			set => isArmed = value;
		}

		private void Awake()
		{
			if(spriteHandler == null) spriteHandler = GetComponentInChildren<SpriteHandler>();
			if(scaleSync == null) scaleSync = GetComponent<ScaleSync>();
			registerItem = GetComponent<RegisterItem>();
			objectBehaviour = GetComponent<ObjectBehaviour>();
			pickupable = GetComponent<Pickupable>();
			explosiveGUI = GetComponent<HasNetworkTabItem>();
		}

		[Server]
		public virtual IEnumerator Countdown()
		{
			countDownActive = true;
			spriteHandler.SetSpriteSO(activeSpriteSO);
			if (GUI != null) GUI.StartCoroutine(GUI.UpdateTimer());
			yield return WaitFor.Seconds(timeToDetonate); //Delay is in milliseconds
			Detonate();
		}

		protected virtual void Detonate()
		{
			// Get data before despawning
			var worldPos = objectBehaviour.AssumedWorldPositionServer();
			// Despawn the explosive
			_ = Despawn.ServerSingle(gameObject);
			Explosion.StartExplosion(worldPos, explosiveStrength);
		}

		/// <summary>
		/// Toggle the detention mode of the explosive, true means it will only work with a signaling device.
		/// </summary>
		/// <param name="mode"></param>
		public void ToggleMode(bool mode)
		{
			detonateImmediatelyOnSignal = mode;
		}

		public override void ReceiveSignal(SignalStrength strength, ISignalMessage message = null)
		{
			if(gameObject == null || countDownActive == true) return;
			if (detonateImmediatelyOnSignal)
			{
				Detonate();
				return;
			}
			StartCoroutine(Countdown());
		}

		protected void PairEmitter(SignalEmitter emitter, GameObject Performer)
		{
			Emitter = emitter;
			Frequency = emitter.Frequency;
			Chat.AddExamineMsg(Performer, "You successfully pair the remote signal to the device.");
		}
	}

	public enum ExplosiveType
	{
		C4,
		X4,
		SyndicateBomb,
	}
}
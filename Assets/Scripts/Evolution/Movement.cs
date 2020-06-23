using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Evolution
{
	public class Movement : MonoBehaviour
	{
		[Header("Parameters")]
		[SerializeField]
		private bool debugPath;

		[Header("Audio")]
		[SerializeField]
		private AudioClip[] moveClip; // Audio that plays when each movement is fired.

		[Header("Animations")]
		[SerializeField]
		private string[] walkingAnimations;
		[SerializeField]
		private string[] runningAnimations;

		[HideInInspector] public NavMeshAgent navMeshAgent;
		public event Action<Vector3> destinationChanged;

		private Animator m_Animator;
		private Rigidbody m_Rbody;
		private List<Vector3> m_Path;
		private LineRenderer m_Lr;
		private int m_SpeedFloat; // Speed parameter on the Animator.
		private AudioSource m_AudioSource;

		// Broadcasting navmesh params
		public float? remainingDistance
		{
			get
			{
				if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
					return navMeshAgent.remainingDistance;
				return null;
			}
		}


		public bool pathPending => navMeshAgent.pathPending;
		public float stoppingDistance => navMeshAgent.stoppingDistance;

		public bool isStopped
		{
			set => navMeshAgent.isStopped = value;
			get => navMeshAgent.isStopped;
		}

		public float speed
		{
			set => navMeshAgent.speed = value;
			get => navMeshAgent.speed;
		}


		private void OnEnable()
		{
			m_Animator = GetComponent<Animator>();
			navMeshAgent = GetComponent<NavMeshAgent>();
			m_Rbody = GetComponent<Rigidbody>();
			m_AudioSource = GetComponent<AudioSource>();
			m_Path = new List<Vector3>();
			if (gameObject.GetComponent<LineRenderer>() == null) m_Lr = gameObject.AddComponent<LineRenderer>();
		}

		private void OnDisable()
		{
			if (debugPath) m_Path?.Clear();
		}

		private void Update()
		{
			if (debugPath && m_Path != null && m_Path.Count > 1)
			{
				m_Lr.positionCount = m_Path.Count;
				for (int i = 0; i < m_Path.Count; i++)
				{
					m_Lr.SetPosition(i, m_Path[i]);
				}
			}
		}

		public void MoveTo(Vector3 destination)
		{
			navMeshAgent.destination = destination;
			destinationChanged?.Invoke(destination);
			isStopped = false;

			if (debugPath)
			{
				m_Path.Add(destination);
				if (m_Path.Count > 10) // Clear a bit
				{
					m_Path.RemoveRange(0, 10);
				}
			}
		}
	}
}

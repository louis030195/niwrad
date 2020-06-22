using System;
using Net.Match;
using Net.Session;
using Net.Utils;
using UnityEngine;

namespace Net.Realtime
{
	/// <summary>
	/// Attached to a GameObject, will sync its transform over network regularly
	/// </summary>
	public class TransformSync : MonoBehaviour
	{
		[Tooltip("How frequent the transform should be sync-ed"), Range(0.01f, 1f), SerializeField]
		private float syncFrequency = 0.50f;

		/// <summary>
		/// Id used to sync over network
		/// </summary>
		[HideInInspector] public ulong id;


		private UnityEngine.Vector3 m_LastPosition = UnityEngine.Vector3.zero;
		private float m_LastSentTime;
		private Packet m_Packet;
		private Vector3 m_Position;
		private Quaternion m_Rotation;

		private void Start()
		{
			m_Packet = new Packet();
			m_Position = transform.position.Net();
			m_Rotation = transform.rotation.Net();
			m_Packet.UpdateTransform = new UpdateTransform
			{
				Transform = new Transform
				{
					Id = id,
					Position = m_Position,
					Rotation = m_Rotation
				}
			};
		}

		private void Update()
		{
			var p = transform.position;
			if (Time.time - m_LastSentTime > syncFrequency && UnityEngine.Vector3.Distance(p,m_LastPosition) > 1)
			{
				var r = transform.rotation;
				m_LastSentTime = Time.time;
				m_Position.X = p.x;
				m_Position.Y = p.y;
				m_Position.Z = p.z;
				m_Rotation.X = r.x;
				m_Rotation.Y = r.y;
				m_Rotation.Z = r.z;
				m_Rotation.W = r.w;
				MatchCommunicationManager.instance.Rpc(m_Packet);
				m_LastPosition = p;
			}
		}
	}
}

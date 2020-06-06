using System;
using Net.Match;
using Net.Utils;
using UnityEngine;

namespace Net.Realtime
{
	/// <summary>
	/// Attached to a GameObject, will sync its transform over network regularly
	/// </summary>
	public class TransformSync : MonoBehaviour
	{
		[Tooltip("How frequent the transform should be sync-ed"), Range(0.01f, 0.10f), SerializeField]
		private float syncFrequency = 0.05f;

		/// <summary>
		/// Id used to sync over network
		/// </summary>
		[HideInInspector] public ulong id;

		private UnityEngine.Vector3 m_LastPosition = UnityEngine.Vector3.zero;
		private float m_LastSentTime;

		private void Update()
		{
			if (Time.time - m_LastSentTime > syncFrequency /*&& Vector3.Distance(newPos,_lastPosition) > 1*/) {
				m_LastSentTime = Time.time;
				var p = transform.position;
				MatchCommunicationManager.instance.Rpc(new Packet.Types.UpdateTransformPacket
				{
					ObjectTransform = new Transform
					{
						Id = id,
						Position = p.ToVector3(),
						Rotation = transform.rotation.ToQuaternion()
					}
				});
				m_LastPosition = p;
			}
		}
	}
}

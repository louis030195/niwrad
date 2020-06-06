using Net.Realtime;
using Net.Session;

namespace Net.Utils
{
	public static class Helper
	{
		public static UnityEngine.Quaternion ToQuaternion(this Quaternion q)
		{
			return new UnityEngine.Quaternion(q.X, q.Y, q.Z, q.W);
		}
		public static UnityEngine.Vector3 ToVector3(this Vector3 v)
		{
			return new UnityEngine.Vector3(v.X, v.Y, v.Z);
		}

		public static Quaternion ToQuaternion(this UnityEngine.Quaternion q)
		{
			return new Quaternion {X = q.x, Y = q.y, Z = q.z, W = q.w};
		}
		public static Vector3 ToVector3(this UnityEngine.Vector3 v)
		{
			return new Vector3 { X = v.x, Y = v.y,  Z = v.z};
		}

		/// <summary>
		/// Initialize a Packet with basic information retrieved in the current state
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Packet Basic(this Packet p)
		{
			p.SenderId = SessionManager.instance.session.UserId;
			p.IsServer = SessionManager.instance.isServer;
			return p;
		}

		public static Packet SpawnAnimal(this Packet p, ulong id, UnityEngine.Vector3 v, UnityEngine.Quaternion q)
		{
			p.Spawn = new Packet.Types.SpawnPacket
			{
				Animal = new Packet.Types.SpawnPacket.Types.AnimalObject
				{
					ObjectTransform = new Transform
					{
						Id = id,
						Position = v.ToVector3(),
						Rotation = q.ToQuaternion()
					}
				}
			};
			return p;
		}

		public static Packet SpawnTree(this Packet p, ulong id, UnityEngine.Vector3 v, UnityEngine.Quaternion q)
		{
			p.Spawn = new Packet.Types.SpawnPacket
			{
				Tree = new Packet.Types.SpawnPacket.Types.TreeObject
				{
					ObjectTransform = new Transform
					{
						Id = id,
						Position = v.ToVector3(),
						Rotation = q.ToQuaternion()
					}
				}
			};
			return p;
		}
	}
}

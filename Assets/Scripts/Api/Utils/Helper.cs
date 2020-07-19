using Api.Realtime;
using Api.Session;

namespace Api.Utils
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

		public static Quaternion Net(this UnityEngine.Quaternion q)
		{
			return new Quaternion {X = q.x, Y = q.y, Z = q.z, W = q.w};
		}
		public static Vector3 Net(this UnityEngine.Vector3 v)
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

		public static Packet ReqSpawnAnimal(this Packet p, UnityEngine.Vector3 v, UnityEngine.Quaternion q)
		{
			p.RequestSpawn = new Spawn
			{
				Animal = new Animal
				{
					Transform = new Transform
					{
						Position = v.Net(),
						Rotation = q.Net()
					}
				}
			};
			return p;
		}
		public static Packet SpawnAnimal(this Packet p, ulong id, UnityEngine.Vector3 v, UnityEngine.Quaternion q)
		{
			p.Spawn = new Spawn
			{
				Animal = new Animal
				{
					Transform = new Transform
					{
						Id = id,
						Position = v.Net(),
						Rotation = q.Net()
					}
				}
			};
			return p;
		}
		public static Packet ReqSpawnTree(this Packet p, UnityEngine.Vector3 v, UnityEngine.Quaternion q)
		{
			p.RequestSpawn = new Spawn
			{
				Tree = new Tree
				{
					Transform = new Transform
					{
						Position = v.Net(),
						Rotation = q.Net()
					}
				}
			};
			return p;
		}
		public static Packet SpawnTree(this Packet p, ulong id, UnityEngine.Vector3 v, UnityEngine.Quaternion q)
		{
			p.Spawn = new Spawn
			{
				Tree = new Tree
				{
					Transform = new Transform
					{
						Id = id,
						Position = v.Net(),
						Rotation = q.Net()
					}
				}
			};
			return p;
		}

		public static Packet DestroyAnimal(this Packet p, ulong id)
		{
			p.Destroy = new Destroy
			{
				Animal = new Animal
				{
					Transform = new Transform
					{
						Id = id
					}
				}
			};
			return p;
		}

		public static Packet DestroyTree(this Packet p, ulong id)
		{
			p.Destroy = new Destroy
			{
				Tree = new Tree
				{
					Transform = new Transform
					{
						Id = id
					}
				}
			};
			return p;
		}
	}
}

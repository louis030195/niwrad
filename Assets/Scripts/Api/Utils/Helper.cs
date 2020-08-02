using System;
using Api.Realtime;
using Api.Session;
using Protometry.Quaternion;
using Protometry.Vector3;
using Protometry.Volume;

namespace Api.Utils
{
	public static class Helper
	{
		public static UnityEngine.Quaternion ToQuaternion(this Quaternion q)
		{
			return new UnityEngine.Quaternion((float) q.X, (float) q.Y, (float) q.Z, (float) q.W);
		}
		public static UnityEngine.Vector3 ToVector3(this Vector3 v)
		{
			return new UnityEngine.Vector3((float) v.X, (float) v.Y, (float) v.Z);
		}

		public static Quaternion Net(this UnityEngine.Quaternion q)
		{
			return new Quaternion {X = q.x, Y = q.y, Z = q.z, W = q.w};
		}
		public static Vector3 Net(this UnityEngine.Vector3 v)
		{
			return new Vector3 { X = v.x, Y = v.y,  Z = v.z};
		}
        
        public static Box OfSize(this Box b, double x, double y, double z, double size)
        {
            b.Min.X = x - size / 2;
            b.Min.Y = y - size / 2;
            b.Min.Z = z - size / 2;
            
            b.Max.X = x + size / 2;
            b.Max.Y = y + size / 2;
            b.Max.Z = z + size / 2;

            return b;
        }

        /// <summary>
        /// Initialize a Packet with basic information retrieved in the current state
        /// By default has a global impact
        /// </summary>
        /// <param name="p"></param>
        /// <param name="impact">specify a spatial impact or global and leave null</param>
        /// <returns></returns>
        public static Packet Basic(this Packet p, Vector3 impact = null)
		{
			p.IsServer = SessionManager.instance.isServer;
            p.Impact = impact;
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

        public static float[,] To2dArray(this Matrix m)
        {
            var twoDArray = new float[m.Rows.Count, m.Rows[0].Cols.Count];
            for (var i = 0; i < m.Rows.Count; i++)
            {
                for (var j = 0; j < m.Rows[0].Cols.Count; j++)
                {
                    twoDArray[i, j] = (float)m.Rows[i].Cols[j];
                }
            }
            return twoDArray;
        }

        public static UnityEngine.Vector3 GetCenter(this Box b)
        {
            var max = b.Max.ToVector3();
            var min = b.Min.ToVector3();
            return UnityEngine.Vector3.Lerp(min, max , 0.5f);
        }
	}
}

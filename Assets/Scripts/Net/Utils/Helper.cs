using Net.Realtime.Protometry.Vector3;

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
	}
}

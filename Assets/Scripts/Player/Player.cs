using System.Collections.Generic;
using Nakama;

namespace Player
{
	public class NiwradUser : IApiUser
	{
		public string AvatarUrl { get; }
		public string CreateTime { get; }
		public string DisplayName { get; }
		public int EdgeCount { get; }
		public string FacebookId { get; }
		public string GamecenterId { get; }
		public string GoogleId { get; }
		public string Id { get; }
		public string LangTag { get; }
		public string Location { get; }
		public string Metadata { get; }
		public bool Online { get; }
		public string SteamId { get; }
		public string Timezone { get; }
		public string UpdateTime { get; }
		public string Username { get; }
		public List<Right> rights { get; } = new List<Right>(); // TODO: this player has the right to do X on X server ...
		// For example player X can spawn animals on Y but can't spawn animals on Z
	}

	public class Right
	{

	}
}

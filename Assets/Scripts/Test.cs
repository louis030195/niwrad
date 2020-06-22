using Nakama;

using UnityEngine;

public class Test : MonoBehaviour
{


	private void Start()
	{
		G();
	}

	private async void G()
	{
		var client = new Client("http", "127.0.0.1", 7350, "defaultkey");
		var session = await client.AuthenticateEmailAsync("aaaa@aaaa.com", "aaaaaaaa", create: false);
		var s = client.NewSocket();
		await s.ConnectAsync(session);
		// See https://github.com/heroiclabs/nakama/blob/master/server/config.go
		await s.RpcAsync( "create_match");
	}
}

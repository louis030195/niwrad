using Nakama;
using Nakama.TinyJson;
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
		var payload = "{\"PokemonName\": \"dragonite\"}".ToJson();
		var res = await client.RpcAsync(session, "create_match", payload);
		Debug.Log($"{res}");
		// var c = res.Payload.FromJson<createPartyResponse>();
		// JsonParser.Default.Parse<createPartyResponse>(res.Payload);
		// Debug.Log($"{c}");
	}
}

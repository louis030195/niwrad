
using Nakama;
using UnityEngine;

public class Test : MonoBehaviour
{
	private async void Start()
	{
		var client = new Client("http", "localhost", 6666, "defaultkey", UnityWebRequestAdapter.Instance);
		var session = await client.AuthenticateEmailAsync("aaaa@aaaa.com", "aaaaaaaa", create: true);

	}
}

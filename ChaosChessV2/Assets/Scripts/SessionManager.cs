using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : NetworkBehaviour
{
	private NetworkRunner runner;
	async void StartGame(GameMode mode)
	{
		runner.ProvideInput = true;
		var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
		var sceneInfo = new NetworkSceneInfo();
		if (scene.IsValid)
		{
			sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
		}

		await runner.StartGame(new StartGameArgs()
		{
			GameMode = mode,
			SessionName = "TestRoom",
			Scene = scene,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		});
	}
}

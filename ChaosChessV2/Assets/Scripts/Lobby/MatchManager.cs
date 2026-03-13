using Fusion;
using Fusion.Photon.Realtime;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private NetworkRunner runner;

    async void StartGame(GameMode mode)
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "Room123",
            Scene = SceneRef.FromIndex(1),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            MatchmakingMode = MatchmakingMode.FillRoom
        });
    }
}

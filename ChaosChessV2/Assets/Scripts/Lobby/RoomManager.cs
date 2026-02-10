using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class RoomManager : MonoBehaviour, INetworkRunnerCallbacks
{
	private NetworkRunner runner;

	[Header("UI References")]
	[SerializeField] private GameObject roomEntryPrefab;    // Scroll View에 들어가는 항목
	[SerializeField] private GameObject playerPrefab;		// 플레이어 프리펩
	[SerializeField] private Transform roomListContent;     // Scroll View Content

	[Header("Canvases")]
	[SerializeField] private GameObject hostRoomCanvas;
	[SerializeField] private GameObject lobbyCanvas;
	[SerializeField] private GameObject roomListCanvas;

	[Header("Component References")]
	[SerializeField] private RoomUIManager roomUIManager;

	public async void JoinLobby()
	{
		if(runner == null) runner = gameObject.AddComponent<NetworkRunner>();

		var result = await runner.JoinSessionLobby(SessionLobby.ClientServer);
		if (result.Ok)
		{
			lobbyCanvas.SetActive(false);
			roomListCanvas.SetActive(true);
		} 
	}

	public async void HostLobby(string roomName)
	{
		if(runner == null) runner = gameObject.AddComponent<NetworkRunner>();

		var startGameArgs = new StartGameArgs()
		{
			GameMode = GameMode.Host,
			SessionName = roomName,
			PlayerCount = 2,
			Scene = SceneRef.FromIndex(1),
			SceneManager = GetComponent<NetworkSceneManagerDefault>()
		};

		var result = await runner.StartGame(startGameArgs);

		if(result.Ok)
		{
			lobbyCanvas.SetActive(false);
			hostRoomCanvas.SetActive(true);
		}
		else
		{
			Debug.LogError($"방 생성 실패 : {result.ShutdownReason}");
		}
	}

	public async void JoinGame(string roomName)
	{
		if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();

		var startGameArgs = new StartGameArgs()
		{
			GameMode = GameMode.Client,
			SessionName = roomName,
			SceneManager = GetComponent<NetworkSceneManagerDefault>()
		};

		var result = await runner.StartGame(startGameArgs);

		if (result.Ok)
		{
			roomListCanvas.SetActive(false);
			hostRoomCanvas.SetActive(true);
		}
		else
		{
			Debug.LogError($"방 생성 실패 : {result.ShutdownReason}");
		}
	}

	public async void LeaveRoom()
	{
		if (runner != null)
		{
			// 1. (클라이언트인 경우) 내가 조종하던 오브젝트를 명시적으로 파괴 요청
			// 서버가 이 파괴를 감지하면 다른 사람들의 화면에서도 Despawned가 더 잘 불립니다.
			var myPlayerObj = runner.GetPlayerObject(runner.LocalPlayer);
			if (myPlayerObj != null && !runner.IsServer)
			{
				// 클라이언트는 권한이 없으므로 직접 Despawn 못하지만, 
				// Shutdown을 하기 전에 서버가 인지할 시간을 줍니다.
			}

			// 2. 네트워크 종료
			// Shutdown(true)를 하면 현재 세션에서 완전히 깨끗하게 물러납니다.
			await runner.Shutdown();
		}
	}

	public void OnConnectedToServer(NetworkRunner runner)
	{
		throw new NotImplementedException();
	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{
		throw new NotImplementedException();
	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{
		throw new NotImplementedException();
	}

	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{
		throw new NotImplementedException();
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{
		throw new NotImplementedException();
	}

	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{
		throw new NotImplementedException();
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		throw new NotImplementedException();
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{
		throw new NotImplementedException();
	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		throw new NotImplementedException();
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{
		throw new NotImplementedException();
	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		if (runner.IsServer)
		{
			Debug.Log($"플레이어 {player} 접속! 오브젝트 생성 중...");

			// 네트워크 상에 플레이어 오브젝트 생성
			var playerObj = runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, player, (runner, obj) =>
			{
				obj.GetComponent<NetworkPlayer>().PlayerName = $"Player {player.PlayerId}";
			});
			runner.SetPlayerObject(player, playerObj);
		}

		roomUIManager.ShowRoom();
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{
		Debug.Log($"플레이어 {player.PlayerId}가 나갔습니다.");
		

		if (runner.IsServer)
		{
			NetworkObject playerObj = runner.GetPlayerObject(player);
			if (playerObj != null)
			{
				runner.Despawn(playerObj);
			}
			else
			{
				// 만약 GetPlayerObject로 안 찾아진다면 전체 검색 (안전장치)
				foreach (var obj in runner.GetAllNetworkObjects())
				{
					if (obj.InputAuthority == player)
					{
						runner.Despawn(obj);
					}
				}
			}
		}

		if (roomUIManager != null)
		{
			Debug.Log("플레이어 지우기 로직 시작");
			roomUIManager.RemoveUserEntry(player.PlayerId);
		}
	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{
		throw new NotImplementedException();
	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
		throw new NotImplementedException();
	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
		throw new NotImplementedException();
	}

	public void OnSceneLoadStart(NetworkRunner runner)
	{
		throw new NotImplementedException();
	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{
		foreach(Transform child in roomListContent)
		{
			Destroy(child.gameObject);
		}

		foreach(var session in sessionList)
		{
			if(session.IsVisible && session.PlayerCount < session.MaxPlayers)
			{
				GameObject entry = Instantiate(roomEntryPrefab, roomListContent);
				entry.GetComponent<RoomInfo>().Setup(session, this);
			}
		}
	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{
		Debug.Log($"Shutdown 사유 : {shutdownReason}");

		if(this.runner != null )
		{
			Destroy(this.runner);
			this.runner = null;
		}

		roomUIManager.HideRoom();
		lobbyCanvas.SetActive(true);
	}

	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{
		throw new NotImplementedException();
	}
}

using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
	[Networked] public string PlayerName { get; set; }
	[Networked] public int PlayerId { get; set; }

	private bool isLoaded = false;

	public override void Spawned()
	{
		if (Object.HasStateAuthority)
		{
			PlayerId = Object.InputAuthority.PlayerId;
			PlayerName = $"Player {PlayerId}";

			UpdateRoomUI();
		}
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		var roomUI = FindFirstObjectByType<RoomUIManager>();
		if (roomUI != null)
		{
			roomUI.RemoveUserEntry(PlayerId);
		}
	}

	public override void Render()
	{
		if (!string.IsNullOrEmpty(PlayerName))
		{
			UpdateRoomUI();
		}
	}


	private void UpdateRoomUI()
	{
		var roomUI = FindFirstObjectByType<RoomUIManager>();
		if (roomUI != null)
		{
			if (!isLoaded)
			{
				// ID를 함께 넘겨서 정렬에 사용합니다.
				roomUI.AddUserEntry(PlayerId, PlayerName);
				isLoaded = true;
			}
		}
	}
}

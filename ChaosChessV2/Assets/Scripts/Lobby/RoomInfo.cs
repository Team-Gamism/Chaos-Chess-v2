using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomInfo : MonoBehaviour
{
	[SerializeField] private TMP_Text RoomName;
	[SerializeField] private Button joinButton;

	private string sessionName;
	private RoomManager roomManager;

	public void Setup(SessionInfo info, RoomManager roomManager)
	{
		this.roomManager = roomManager;
		sessionName = info.Name;
		RoomName.text = sessionName;

		joinButton.onClick.AddListener(() => roomManager.JoinGame(sessionName));
	}
}

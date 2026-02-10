using TMPro;
using UnityEngine;

public class CreateRoom : MonoBehaviour
{
	[SerializeField] private TMP_InputField field;
	[SerializeField] private RoomManager manager;

	public void CreateRoomSession()
	{
		if(field.text == string.Empty)
		{
			return;
		}
		manager.HostLobby(field.text);
	}
}

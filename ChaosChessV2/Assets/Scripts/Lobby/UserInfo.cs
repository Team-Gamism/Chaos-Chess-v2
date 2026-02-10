using TMPro;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
	[SerializeField] private TMP_Text playerNameText;

	public void Setup(string playerName)
	{
		playerNameText.text = playerName;
	}
}

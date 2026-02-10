using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomUIManager : MonoBehaviour
{
	[SerializeField] private GameObject userEntryPrefab;
	[SerializeField] private Transform userListContent;
	[SerializeField] private GameObject roomCanvas;

	private Dictionary<int, GameObject> userEntries = new Dictionary<int, GameObject>();

	public void ShowRoom()
	{
		roomCanvas.SetActive(true);
	}

	public void AddUserEntry(int id, string name)
	{
		if (userEntries.ContainsKey(id)) return;
		
		GameObject entry = Instantiate(userEntryPrefab, userListContent);
		entry.GetComponent<UserInfo>().Setup(name);

		userEntries.Add(id, entry);

		sortEntries();	
	}

	public void RemoveUserEntry(int id)
	{
		if (userEntries.TryGetValue(id, out GameObject entry))
		{
			Debug.Log($"[삭제 시도] ID: {id}, 이름: {entry.name}");
			Destroy(entry);
			userEntries.Remove(id);
			Debug.Log($"[삭제 완료] ID {id}가 딕셔너리에서 제거되었습니다.");
		}
		else
		{
			Debug.LogWarning($"[삭제 실패] ID {id}를 딕셔너리에서 찾을 수 없습니다.");
		}

		// 딕셔너리 상태 출력
		PrintDictionary();

		sortEntries();
	}

	public void ClearAllEntries()
	{
		foreach (var entry in userEntries.Values)
		{
			Destroy(entry);
		}
		userEntries.Clear();
	}

	public void HideRoom()
	{
		roomCanvas.SetActive(false);
		ClearAllEntries();
	}

	private void sortEntries()
	{
		// 방어 코드: 딕셔너리에 없는 자식 오브젝트 제거
		foreach (Transform child in userListContent)
		{
			if (!userEntries.ContainsValue(child.gameObject))
			{
				Destroy(child.gameObject);
			}
		}

		var sortedIds = userEntries.Keys.OrderBy(id => id).ToList();
		for (int i = 0; i < sortedIds.Count; i++)
		{
			userEntries[sortedIds[i]].transform.SetAsLastSibling();
		}

		// 정렬 후 상태 출력
		PrintDictionary();
	}

	private void PrintDictionary()
	{
		if (userEntries.Count == 0)
		{
			Debug.Log("<color=yellow>현재 딕셔너리가 비어 있습니다.</color>");
			return;
		}

		string dictStatus = string.Join("\n", userEntries.Select(kv => $"- ID: {kv.Key}, 오브젝트: {(kv.Value != null ? kv.Value.name : "null")}"));
		Debug.Log($"<color=cyan><b>[현재 딕셔너리 목록]</b></color>\n{dictStatus}");
	}
}

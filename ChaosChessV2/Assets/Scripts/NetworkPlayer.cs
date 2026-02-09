using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
	[Networked] public int TeamSide { get; set; }

	[Networked] public string Nickname { get;set; }
	[Networked, Capacity(8)] public NetworkArray<int> SelectedDeck { get; }
	[Networked, Capacity(4)] public NetworkArray<int> CurrentHand { get; }

	public override void Spawned()
	{
		if (Object.HasStateAuthority)
		{
			// 게임 시작 시 서버에서 랜덤하게 4장 세팅
			ShuffleAndDraw();
		}
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void RPC_UseCard(int cardID, int targetID, Vector2Int targetPos)
	{
		// 카드 소모 및 효과 발동 로직 (GameSession 호출)
	}

	private void ShuffleAndDraw()
	{
		// 8장의 Deck에서 랜덤하게 4개를 골라 CurrentHand에 채우는 로직
	}
}

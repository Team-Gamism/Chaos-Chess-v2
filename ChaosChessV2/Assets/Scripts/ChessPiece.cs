using Fusion;
using UnityEngine;

public class ChessPiece : NetworkBehaviour
{
	[Networked] public Vector2Int LogicPos { get; set; }
	[Networked] public PieceType PieceType { get; set; }
	[Networked] public NetworkBool IsShielded { get; set; }
	[Networked] public TickTimer FrozenTimer { get; set; }
	[Networked] public int CurrentOwnerID { get; set; }

	public override void Render()
	{
		// 여기서 IsShielded나 FrozenTimer를 체크해서 이펙트를 On/Off 합니다.
	}

	//public void OnPositionChanged()
	//{

	//}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void RPC_RequestMove(Vector2Int targetPos)
	{
		// 1. 이동 규칙 검중 (체스 룰)
		// 2. 타일 봉쇄 여부 확인
		// 3. 검증 통과 및 좌표 업데이트
		this.LogicPos = targetPos;
	}
}
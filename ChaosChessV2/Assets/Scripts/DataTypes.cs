using Fusion;
using UnityEngine;

// 카드 등급
public enum CardRank { Common, Uncommon, Rare, Mythic, Legendary }

// 카드 타겟팅 타입
public enum CardType { TargetPiece, TargetTile, Global, InstantMove }

// 체스 기물 종류
public enum PieceType { Pawn, Knight, Bishop, Rook, Queen, King }

// 타일 상태 구조체 (네트워크 동기화 가능)
public struct TileState : INetworkStruct
{
	public NetworkBool IsBlocked;
	public int RemainingTurns;
}
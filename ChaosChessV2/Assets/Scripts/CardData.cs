using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "ChaosChess/Card")]
public class CardData : ScriptableObject
{
	public int CardID;
	public string CardName;
	public CardRank Rank;
	public CardType Type;

	public void Execute(ChessPiece targetPiece, Vector2Int targetPos)
	{
		// 카드 타입별 실제 효과 구현
		// 이 로직은 서버(StateAuthority)에서만 실행되도록 설계해야 함
	}
}
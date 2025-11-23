// 기물의 움직임을 나타내는 구조체입니다.

public struct ChessMove
{
	public byte FromX;
	public byte FromY;
	public byte ToX;
	public byte ToY;
	public PieceType PromotionType;
}
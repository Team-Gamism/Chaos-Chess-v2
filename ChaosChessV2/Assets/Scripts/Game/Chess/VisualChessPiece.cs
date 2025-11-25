using ChaosChess.Core;
using DG.Tweening;
using UnityEngine;

public class VisualChessPiece : MonoBehaviour
{
	private ChessGame chessGame;
	private void Start()
	{
		chessGame = FindFirstObjectByType<ChessGame>();
	}
	public void VisualUpdate(ChessMove move)
	{
		Vector2 to = new Vector2(move.ToX, move.ToY);
		Vector2 from = new Vector2(move.FromX, move.FromY);

		Vector3 toDir = chessGame.GetTransformFromCoord(to);
		Vector3 fromDir = chessGame.GetTransformFromCoord(from);

		transform.DOMove(toDir, 0.5f).SetEase(Ease.OutQuint);
	}
}

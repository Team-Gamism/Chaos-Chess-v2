using ChaosChess.Core;
using DG.Tweening;
using System;
using UnityEngine;

public class VisualChessPiece : MonoBehaviour
{
	private ChessGame chessGame;
	private SpriteRenderer spriteRenderer;
	
	public ChessPiece ChessPiece;
	public bool IsCaptured = false;
	public Action OnCaptured;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		chessGame = FindFirstObjectByType<ChessGame>();

		Vector2 coord = chessGame.GetCoordFromTransform(transform.position);
		ChessPiece = chessGame.GetPiece((int)coord.x, (int)coord.y);
	}

	public void VisualUpdate(ChessMove move)
	{
		ChessPiece = chessGame.GetPiece(move.ToX, move.ToY);
		if(move.PromotionType != PieceType.None)
		{
			Sprite newSprite = chessGame.GetPieceSprite(ChessPiece);
			Debug.Log("새 스프라이트: " + (newSprite == null ? "NULL" : newSprite.name));
			Debug.Log("현재 타입: " + ChessPiece.Type);

			spriteRenderer.sprite = newSprite;
		}

		Vector2 to = new Vector2(move.ToX, move.ToY);
		Vector2 from = new Vector2(move.FromX, move.FromY);

		Vector3 toDir = chessGame.GetTransformFromCoord(to);
		Vector3 fromDir = chessGame.GetTransformFromCoord(from);

		transform.DOMove(toDir, 0.5f).SetEase(Ease.OutQuint);
	}

	public void CapturedCheck()
	{
		if (IsCaptured)
		{
			OnCaptured?.Invoke();
			Destroy(gameObject);
		}
	}
}

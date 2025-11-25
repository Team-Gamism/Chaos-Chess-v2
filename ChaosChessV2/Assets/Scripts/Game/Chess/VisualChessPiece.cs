using ChaosChess.Core;
using DG.Tweening;
using System;
using UnityEditor;
using UnityEngine;

public class VisualChessPiece : MonoBehaviour
{
	private ChessGame chessGame;
	
	public ChessPiece ChessPiece;
	public bool IsCaptured = false;
	public Action OnCaptured;

	private void Start()
	{
		chessGame = FindFirstObjectByType<ChessGame>();
		Vector2 coord = chessGame.GetCoordFromTransform(transform.position);
		ChessPiece = chessGame.GetPiece((int)coord.x, (int)coord.y);
	}
	public void VisualUpdate(ChessMove move)
	{
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

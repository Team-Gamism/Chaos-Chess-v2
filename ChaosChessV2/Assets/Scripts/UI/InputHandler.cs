using ChaosChess.Core;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
	private ChessMove move;
	private VisualChessPiece piece;

	[SerializeField] private ChessGame game;
	[SerializeField] private bool isSelected = false;

	private List<VisualChessPiece> chessPieces = new List<VisualChessPiece>();

	private void Start()
	{
		ChessPiece[,] pieces = game.GetBoard();
		for(int x = 0; x < 8; x++)
		{
			for(int y = 0; y < 8; y++)
			{
				if(pieces[x,y].Type != PieceType.None)
				{
					VisualChessPiece piece = game.GetVisualPiece(x, y);
					chessPieces.Add(piece);

					piece = game.GetVisualPiece(x, y);
					piece.OnCaptured.AddListener(() =>
					{
						chessPieces.Remove(piece);
					});
				}
			}
		}
	}

	private void Update()
	{
		if(Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			if(touch.phase == TouchPhase.Began)
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(touch.position);

				if (Physics.Raycast(ray, out hit))
				{
					if (hit.collider.CompareTag("Piece"))
					{
						VisualChessPiece clicked = hit.collider.GetComponent<VisualChessPiece>();
						if (clicked == null) return;

						Vector2 vec = game.GetCoordFromTransform(clicked.transform.position);

						if (!isSelected)
						{
							// 내 기물을 선택
							if (clicked.ChessPiece.Color == game.GetCurrentPlayer())
							{
								piece = clicked;
								isSelected = true;

								piece.OnSelected?.Invoke();
								move.FromX = (byte)vec.x;
								move.FromY = (byte)vec.y;
								Debug.Log($"기물 선택됨! 선택된 기물 타입 : {piece.ChessPiece.Type}");
							}
						}
						else
						{
							// 이미 선택된 기물이 있을 때
							if (clicked.ChessPiece.Color == game.GetCurrentPlayer())
							{
								// 내 기물 클릭 → 선택 변경
								piece.OnDeselected?.Invoke();
								piece = clicked;
								piece.OnSelected?.Invoke();
								move.FromX = (byte)vec.x;
								move.FromY = (byte)vec.y;
								Debug.Log($"기물 선택됨! 선택된 기물 타입 : {piece.ChessPiece.Type}");
							}
							else
							{
								// 상대 기물 클릭 → 공격 시도
								move.ToX = (byte)vec.x;
								move.ToY = (byte)vec.y;
								Debug.Log($"상대 기물 선택됨! 공격을 시도합니다!");
								game.TryMove(move, piece);

								for (int i = 0; i < chessPieces.Count; i++)
									chessPieces[i].CapturedCheck();

								// 선택 해제
								piece.OnDeselected?.Invoke();
								piece = null;
								isSelected = false;
							}
						}
					}
					else if (hit.collider.CompareTag("Board"))
					{
						if(piece != null)
						{
							// 터치한 위치를 무조건 도착 지점으로 설정
							Vector2 vec = hit.transform.position;
							vec = game.GetCoordFromTransform(vec);
							Debug.Log($"타일 선택됨! 선택된 타일 위치 : {vec}");
							//Debug.Log($"{vec.x} {vec.y}");
							move.ToX = (byte)vec.x;
							move.ToY = (byte)vec.y;

							// 이동을 시도하고 성공/실패 여부에 관계없이 선택 해제
							bool tryMove = game.TryMove(move, piece);
							Debug.Log($"기물 이동 시도함! 이동을 시도한 기물 : {piece.ChessPiece.Type} 결과 : {tryMove}");

							piece.OnDeselected?.Invoke();
							piece = null;
							isSelected = false;
							for (int i = 0; i < chessPieces.Count; i++)
								chessPieces[i].CapturedCheck();
						}
					}
				}
			}
		}
	}
}

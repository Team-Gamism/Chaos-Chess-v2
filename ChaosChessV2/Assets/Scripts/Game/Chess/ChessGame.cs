using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Tilemaps;

namespace ChaosChess.Core
{
	public class ChessGame : MonoBehaviour
	{
		private const int BOARD_SIZE = 8;

		private ChessPiece[,] board = new ChessPiece[BOARD_SIZE, BOARD_SIZE];
		private PlayerColor currentPlayer = PlayerColor.White;
		private GameState gameState = GameState.Playing;

		// 마지막 이동 추적 (앙파상용)
		private ChessMove? lastMove = null;

		// 이동 히스토리 (나중에 무르기 기능용)
		private List<ChessMove> moveHistory = new List<ChessMove>();

		[Header("Visual Settings")]
		[SerializeField] private float gap = 0.1f;
		[SerializeField] private float pieceSize = 1f;
		[SerializeField] private float boardSize = 1f;
		[SerializeField] private Vector3 offset;

		[Header("Sprites")]
		[SerializeField] private Sprite[] sprites; // 0: None, 1: Pawn, 2: Knight, 3: Bishop, 4: Rook, 5: Queen, 6: King

		[Header("Materials")]
		[SerializeField] private Material white;
		[SerializeField] private Material black;

		[Header("Prefabs")]
		[SerializeField] private GameObject piecePrefab;
		[SerializeField] private GameObject boardPrefab;

		// ============================================
		// Unity 생명주기
		// ============================================

		private void Awake()
		{
			Initialize();
		}

		// ============================================
		// 초기화
		// ============================================

		private void Initialize()
		{
			// 보드 초기화 (모두 빈 칸)
			for (int x = 0; x < BOARD_SIZE; x++)
			{
				for (int y = 0; y < BOARD_SIZE; y++)
				{
					board[x, y] = new ChessPiece { Type = PieceType.None };
				}
			}

			// 백 기물 배치 (아래쪽, y = 7, 6)
			SetupPieces(PlayerColor.Black, 7, 6);

			// 흑 기물 배치 (위쪽, y = 0, 1)
			SetupPieces(PlayerColor.White, 0, 1);

			// 화면에 표시
			SpawnObjects();
		}

		private void SetupPieces(PlayerColor color, int backRow, int pawnRow)
		{
			// 폰 줄
			for (int x = 0; x < BOARD_SIZE; x++)
			{
				board[x, pawnRow] = new ChessPiece { Type = PieceType.Pawn, Color = color };
			}

			// 뒷줄: 룩, 나이트, 비숍, 퀸, 킹, 비숍, 나이트, 룩
			board[0, backRow] = new ChessPiece { Type = PieceType.Rook, Color = color };
			board[1, backRow] = new ChessPiece { Type = PieceType.Knight, Color = color };
			board[2, backRow] = new ChessPiece { Type = PieceType.Bishop, Color = color };
			board[3, backRow] = new ChessPiece { Type = PieceType.Queen, Color = color };
			board[4, backRow] = new ChessPiece { Type = PieceType.King, Color = color };
			board[5, backRow] = new ChessPiece { Type = PieceType.Bishop, Color = color };
			board[6, backRow] = new ChessPiece { Type = PieceType.Knight, Color = color };
			board[7, backRow] = new ChessPiece { Type = PieceType.Rook, Color = color };
		}

		/// <summary>
		/// 보드와 기물을 화면에 생성
		/// </summary>
		private void SpawnObjects()
		{
			for (int x = 0; x < BOARD_SIZE; x++)
			{
				for (int y = 0; y < BOARD_SIZE; y++)
				{
					GameObject boardObj = Instantiate(boardPrefab, transform);

					SpriteRenderer boardSR = boardObj.GetComponent<SpriteRenderer>();
					boardSR.material = (x + y) % 2 == 0 ? white : black;

					Vector3 pos = new Vector3(x * gap, y * gap, 0f);
					boardObj.transform.position = pos + offset;
					boardObj.transform.localScale = Vector3.one * boardSize;

					// 기물 생성
					if (board[x, y].Type != PieceType.None)
					{
						GameObject pieceObj = Instantiate(piecePrefab, transform);
						SpriteRenderer pieceSR = pieceObj.GetComponent<SpriteRenderer>();
						pieceSR.sprite = GetPieceSprite(board[x, y]);

						pieceObj.transform.position = pos + offset;
						pieceObj.transform.localScale = Vector3.one * pieceSize;
					}
				}
			}
		}

		public Vector2 GetCoordFromTransform(Vector3 worldPos)
		{
			Vector3 posWithoutOffset = worldPos - offset;

			// 2. 간격(gap)으로 나눕니다 (픽셀 단위를 격자 단위로 변환)
			float rawX = posWithoutOffset.x / gap;
			float rawY = posWithoutOffset.y / gap;

			// 3. 가장 가까운 정수로 반올림합니다 (격자 좌표 인덱스)
			int coordX = Mathf.RoundToInt(rawX);
			int coordY = Mathf.RoundToInt(rawY);

			// 4. Vector2 (float) 형태로 반환 (InputHandler에서 (byte)로 변환할 것)
			return new Vector2(coordX, coordY);
		}

		public Vector2 GetTransformFromCoord(Vector2 coord)
		{
			Vector3 pos = new Vector3(coord.x * gap, coord.y * gap, 0f);
			return pos + offset;
		}

		/// <summary>
		/// 기물에 맞는 스프라이트 반환
		/// </summary>
		public Sprite GetPieceSprite(ChessPiece piece)
		{
			if (piece.Type == PieceType.None || sprites == null || sprites.Length < 12)
				return null;

			// sprites 배열 구조:
			// 0-5: White (Pawn, Knight, Bishop, Rook, Queen, King)
			// 6-11: Black (Pawn, Knight, Bishop, Rook, Queen, King)

			int offset = piece.Color == PlayerColor.White ? 0 : 6;
			int index = (int)piece.Type - 1; // PieceType.Pawn = 1, 그래서 -1

			return sprites[offset + index];
		}

		/// <summary>
		/// 이동 시도
		/// </summary>
		public bool TryMove(ChessMove move, VisualChessPiece visualPiece)
		{
			// 1. 규칙 검증 (lastMove 전달)
			if (!GameRules.IsValidMove(board, move, currentPlayer, lastMove))
				return false;

			// 2. 이동 실행
			//Debug.Log($"{move.ToX}, {move.ToY}, {move.FromX}, {move.FromY}");
			ExecuteMove(ref move);

			// 3. lastMove 업데이트
			lastMove = move;

			// 4. 히스토리 추가
			moveHistory.Add(move);

			// 5. 턴 변경
			currentPlayer = currentPlayer == PlayerColor.White
				? PlayerColor.Black
				: PlayerColor.White;

			// 6. 게임 상태 업데이트
			UpdateGameState();

			// 7. 화면 업데이트
			visualPiece.VisualUpdate(move);
			//RefreshVisuals();

			return true;
		}

		/// <summary>
		/// 실제 이동 실행
		/// </summary>
		private void ExecuteMove(ref ChessMove move)
		{
			ChessPiece piece = board[move.FromX, move.FromY];

			// 특수 처리: 캐슬링
			if (piece.Type == PieceType.King &&
				Mathf.Abs(move.ToX - move.FromX) == 2)
			{
				ExecuteCastling(move);
				return;
			}

			// 특수 처리: 앙파상
			if (piece.Type == PieceType.Pawn)
			{
				if(move.ToX != move.FromX &&
				board[move.ToX, move.ToY].Type == PieceType.None)
				{
					ExecuteEnPassant(move);
					return;
				}
			}

			// 이동 위치에 상대 기물 있을 시 삭제
			if (board[move.ToX, move.ToY].Type != PieceType.None)
				GetVisualPiece(move.ToX, move.ToY).IsCaptured = true;

			// 일반 이동
			board[move.ToX, move.ToY] = piece;
			board[move.FromX, move.FromY] = new ChessPiece { Type = PieceType.None };

			// 이동 횟수 증가 (캐슬링, 앙파상 판정용)
			piece.MoveCount++;
			board[move.ToX, move.ToY] = piece;

			// 폰 프로모션
			if (piece.Type == PieceType.Pawn)
			{
				int promotionRow = piece.Color == PlayerColor.White ? 7 : 0;
				if (move.ToY == promotionRow)
				{
					PieceType promoteTo = move.PromotionType != PieceType.None
						? move.PromotionType
						: PieceType.Queen; // 기본값

					board[move.ToX, move.ToY] = new ChessPiece
					{
						Type = promoteTo,
						Color = piece.Color,
						MoveCount = 1
					};
					move.PromotionType = promoteTo;
				}
			}
		}

		/// <summary>
		/// 캐슬링 실행
		/// </summary>
		private void ExecuteCastling(ChessMove move)
		{
			ChessPiece king = board[move.FromX, move.FromY];

			// 킹 이동
			board[move.ToX, move.ToY] = king;
			board[move.FromX, move.FromY] = new ChessPiece { Type = PieceType.None };
			king.MoveCount++;
			board[move.ToX, move.ToY] = king;

			// 룩 이동
			int rookFromX = move.ToX > move.FromX ? 7 : 0; // 킹사이드 or 퀸사이드
			int rookToX = move.ToX > move.FromX ? move.ToX - 1 : move.ToX + 1;

			// 룩 이동 애니메이션
			VisualChessPiece rookPiece = GetVisualPiece(rookFromX, move.FromY);

			ChessPiece rook = board[rookFromX, move.FromY];
			board[rookToX, move.FromY] = rook;
			board[rookFromX, move.FromY] = new ChessPiece { Type = PieceType.None };
			rook.MoveCount++;
			board[rookToX, move.FromY] = rook;

			ChessMove rookMove = new ChessMove
			{
				FromX = (byte)rookFromX,
				FromY = move.FromY,
				ToX = (byte)rookToX,
				ToY = move.FromY
			};

			rookPiece.VisualUpdate(rookMove);
		}

		/// <summary>
		/// 앙파상 실행
		/// </summary>
		private void ExecuteEnPassant(ChessMove move)
		{
			ChessPiece pawn = board[move.FromX, move.FromY];

			// 적 기물 오브젝트
			ChessPiece capturedPawn = board[move.ToX, move.FromY];
			if (capturedPawn.Type != PieceType.None)
			{
				VisualChessPiece capturedVisual = GetVisualPiece(move.ToX, move.FromY);
				if (capturedVisual != null)
					capturedVisual.IsCaptured = true;  // 여기서 처리
			}

			// 아군 폰 이동
			board[move.ToX, move.ToY] = pawn;
			board[move.FromX, move.FromY] = new ChessPiece { Type = PieceType.None };
			pawn.MoveCount++;

			// 4. 적 기물 삭제
			board[move.ToX, move.FromY] = new ChessPiece { Type = PieceType.None };
		}


		/// <summary>
		/// 게임 상태 업데이트
		/// </summary>
		private void UpdateGameState()
		{
			if (GameRules.IsCheckmate(board, currentPlayer))
			{
				gameState = GameState.Checkmate;
				Debug.Log($"체크메이트! 패배자 : {currentPlayer}");
			}
			else if (GameRules.IsStalemate(board, currentPlayer))
			{
				gameState = GameState.Stalemate;
			}
			else if (GameRules.IsKingInCheck(board, currentPlayer))
			{
				gameState = GameState.Check;
			}
			else
			{
				gameState = GameState.Playing;
			}
		}

		/// <summary>
		/// 화면 업데이트 (이동 후)
		/// </summary>
		//private void RefreshVisuals()
		//{
		//	// 기존 오브젝트 제거
		//	Transform piecesParent = transform.Find("Pieces");
		//	if (piecesParent != null)
		//		Destroy(piecesParent.gameObject);

		//	Transform boardsParent = transform.Find("Boards");
		//	if (boardsParent != null)
		//		Destroy(boardsParent.gameObject);

		//	// 다시 생성
		//	SpawnObjects();
		//}

		// ============================================
		// Getter 함수
		// ============================================

		public ChessPiece GetPiece(int x, int y)
		{
			if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE)
				return new ChessPiece { Type = PieceType.None };

			return board[x, y];
		}

		public VisualChessPiece GetVisualPiece(int x, int y)
		{
			if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE)
				return null;

			Vector3 targetPos = GetTransformFromCoord(new Vector2(x, y));

			return FindObjectsOfType<VisualChessPiece>()
				.OrderBy(p => Vector3.Distance(p.transform.position, targetPos))
				.FirstOrDefault();
		}

		public PlayerColor GetCurrentPlayer()
		{
			return currentPlayer;
		}

		public GameState GetGameState()
		{
			return gameState;
		}

		public ChessMove? GetLastMove()
		{
			return lastMove;
		}

		public List<ChessMove> GetMoveHistory()
		{
			return new List<ChessMove>(moveHistory);
		}

		public ChessPiece[,] GetBoard()
		{
			return (ChessPiece[,])board.Clone();
		}

		// ============================================
		// 추가 기능
		// ============================================

		/// <summary>
		/// 게임 리셋
		/// </summary>
		public void ResetGame()
		{
			Initialize();
			currentPlayer = PlayerColor.White;
			gameState = GameState.Playing;
			lastMove = null;
			moveHistory.Clear();
		}

		/// <summary>
		/// 특정 기물 개수 세기 (AI용)
		/// </summary>
		public int CountPieces(PlayerColor color)
		{
			int count = 0;
			for (int x = 0; x < BOARD_SIZE; x++)
			{
				for (int y = 0; y < BOARD_SIZE; y++)
				{
					if (board[x, y].Type != PieceType.None &&
						board[x, y].Color == color)
					{
						count++;
					}
				}
			}
			return count;
		}

		/// <summary>
		/// 가능한 모든 이동 반환 (AI용)
		/// </summary>
		public List<ChessMove> GetAllLegalMoves()
		{
			return GameRules.GetAllLegalMoves(board, currentPlayer);
		}
	}
}
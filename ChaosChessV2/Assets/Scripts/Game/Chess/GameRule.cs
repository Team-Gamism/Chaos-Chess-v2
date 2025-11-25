using UnityEngine;
using System.Collections.Generic;

namespace ChaosChess.Core
{
	public static class GameRules
	{
		// ============================================
		// 1. 메인 검증 함수
		// ============================================

		/// <summary>
		/// 이동이 합법적인지 최종 검증
		/// </summary>
		public static bool IsValidMove(
			ChessPiece[,] board,
			ChessMove move,
			PlayerColor currentPlayer,
			ChessMove? lastMove = null)
		{
			// 1. 기본 검증
			if (!IsBasicValid(board, move, currentPlayer))
			{
				Debug.Log("기본 움직임 거부됨!");
				return false;
			}

			// 2. 기물별 이동 규칙 검증
			ChessPiece piece = board[move.FromX, move.FromY];
			if (!IsValidPieceMove(board, move, piece, lastMove))
			{
				Debug.Log("기물 움직임 거부됨! 기물 타입 : " + piece.Type.ToString());
				return false;
			}

			// 3. 이동 후 자신의 킹이 체크 상태인지 검증 (자살수 방지)
			if (WouldBeInCheck(board, move, currentPlayer))
				return false;

			return true;
		}

		// ============================================
		// 2. 기본 검증
		// ============================================

		private static bool IsBasicValid(
			ChessPiece[,] board,
			ChessMove move,
			PlayerColor currentPlayer)
		{
			Debug.Log($"[IsBasicValid] From({move.FromX}, {move.FromY}) → To({move.ToX}, {move.ToY}), Player = {currentPlayer}");

			// 범위 체크
			if (!IsInBounds(move.FromX, move.FromY) ||
				!IsInBounds(move.ToX, move.ToY))
			{
				Debug.Log("[IsBasicValid] 실패: 이동 범위를 벗어남.");
				return false;
			}

			// 출발지에 기물이 있는가?
			ChessPiece piece = board[move.FromX, move.FromY];
			if (piece.Type == PieceType.None)
			{
				Debug.Log("[IsBasicValid] 실패: 출발 위치에 기물이 없음.");
				return false;
			}

			// 내 기물인가?
			if (piece.Color != currentPlayer)
			{
				Debug.Log($"[IsBasicValid] 실패: 출발 기물 색상이 현재 플레이어와 다름. piece.Color={piece.Color}, currentPlayer={currentPlayer}");
				return false;
			}

			// 같은 자리로 이동?
			if (move.FromX == move.ToX && move.FromY == move.ToY)
			{
				Debug.Log("[IsBasicValid] 실패: 같은 칸으로 이동하려 함.");
				return false;
			}

			// 목적지에 내 기물이 있는가?
			ChessPiece targetPiece = board[move.ToX, move.ToY];
			if (targetPiece.Type != PieceType.None &&
				targetPiece.Color == currentPlayer)
			{
				Debug.Log("[IsBasicValid] 실패: 목적 위치에 내 기물이 있음.");
				return false;
			}

			Debug.Log("[IsBasicValid] 성공: 기본 유효성 검사 통과.");
			return true;
		}


		// ============================================
		// 3. 기물별 이동 규칙
		// ============================================

		private static bool IsValidPieceMove(
			ChessPiece[,] board,
			ChessMove move,
			ChessPiece piece,
			ChessMove? lastMove)
		{
			switch (piece.Type)
			{
				case PieceType.Pawn:
					return IsValidPawnMove(board, move, piece, lastMove);
				case PieceType.Knight:
					return IsValidKnightMove(move);
				case PieceType.Bishop:
					return IsValidBishopMove(board, move);
				case PieceType.Rook:
					return IsValidRookMove(board, move);
				case PieceType.Queen:
					return IsValidQueenMove(board, move);
				case PieceType.King:
					return IsValidKingMove(board, move, piece);
				default:
					return false;
			}
		}

		// ============================================
		// 4. 폰 이동 규칙
		// ============================================

		private static bool IsValidPawnMove(
			ChessPiece[,] board,
			ChessMove move,
			ChessPiece piece,
			ChessMove? lastMove)
		{
			int direction = piece.Color == PlayerColor.White ? 1 : -1;
			int startRow = piece.Color == PlayerColor.White ? 1 : 6;

			int dx = move.ToX - move.FromX;
			int dy = move.ToY - move.FromY;

			// 1. 전진 1칸
			if (dx == 0 && dy == direction)
			{
				return board[move.ToX, move.ToY].Type == PieceType.None;
			}

			// 2. 전진 2칸 (첫 이동)
			if (dx == 0 && dy == 2 * direction && move.FromY == startRow)
			{
				int middleY = move.FromY + direction;
				return board[move.FromX, middleY].Type == PieceType.None &&
					   board[move.ToX, move.ToY].Type == PieceType.None;
			}

			// 3. 대각선 공격
			if (Mathf.Abs(dx) == 1 && dy == direction)
			{
				ChessPiece target = board[move.ToX, move.ToY];

				// 일반 공격
				if (target.Type != PieceType.None && target.Color != piece.Color)
					return true;

				// 앙파상
				if (IsValidEnPassant(board, move, piece, lastMove))
					return true;
			}

			return false;
		}

		// ============================================
		// 5. 나이트 이동 규칙
		// ============================================

		private static bool IsValidKnightMove(ChessMove move)
		{
			int dx = Mathf.Abs(move.ToX - move.FromX);
			int dy = Mathf.Abs(move.ToY - move.FromY);

			return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
		}

		// ============================================
		// 6. 비숍 이동 규칙
		// ============================================

		private static bool IsValidBishopMove(ChessPiece[,] board, ChessMove move)
		{
			int dx = Mathf.Abs(move.ToX - move.FromX);
			int dy = Mathf.Abs(move.ToY - move.FromY);

			// 대각선인가?
			if (dx != dy)
				return false;

			// 경로에 장애물이 없는가?
			return IsPathClear(board, move);
		}

		// ============================================
		// 7. 룩 이동 규칙
		// ============================================

		private static bool IsValidRookMove(ChessPiece[,] board, ChessMove move)
		{
			// 직선인가?
			if (move.FromX != move.ToX && move.FromY != move.ToY)
				return false;

			// 경로에 장애물이 없는가?
			return IsPathClear(board, move);
		}

		// ============================================
		// 8. 퀸 이동 규칙
		// ============================================

		private static bool IsValidQueenMove(ChessPiece[,] board, ChessMove move)
		{
			// 비숍처럼 or 룩처럼
			return IsValidBishopMove(board, move) || IsValidRookMove(board, move);
		}

		// ============================================
		// 9. 킹 이동 규칙
		// ============================================

		private static bool IsValidKingMove(
			ChessPiece[,] board,
			ChessMove move,
			ChessPiece piece)
		{
			int dx = Mathf.Abs(move.ToX - move.FromX);
			int dy = Mathf.Abs(move.ToY - move.FromY);

			// 1. 일반 이동 (1칸)
			if (dx <= 1 && dy <= 1)
				return true;

			// 2. 캐슬링 (킹사이드, 퀸사이드)
			if (dy == 0 && dx == 2)
				return IsValidCastling(board, move, piece);

			return false;
		}

		// ============================================
		// 10. 특수 규칙: 앙파상
		// ============================================

		private static bool IsValidEnPassant(
			ChessPiece[,] board,
			ChessMove move,
			ChessPiece piece,
			ChessMove? lastMove)
		{
			if (!lastMove.HasValue)
				return false;

			// 마지막 이동이 폰의 2칸 전진이었는가?
			ChessMove last = lastMove.Value;
			ChessPiece lastPiece = board[last.ToX, last.ToY];

			if (lastPiece.Type != PieceType.Pawn)
				return false;

			int lastDy = Mathf.Abs(last.ToY - last.FromY);
			if (lastDy != 2)
				return false;

			// 목표 위치가 적 폰의 뒤쪽인가?
			int direction = piece.Color == PlayerColor.White ? 1 : -1;

			return move.ToX == last.ToX &&
				   move.ToY == last.ToY + direction &&
				   move.FromY == last.ToY;
		}

		// ============================================
		// 11. 특수 규칙: 캐슬링
		// ============================================

		private static bool IsValidCastling(
			ChessPiece[,] board,
			ChessMove move,
			ChessPiece king)
		{
			// 킹이 한 번도 안 움직였는가?
			if (king.MoveCount > 0)
				return false;

			int rookX = move.ToX > move.FromX ? 7 : 0; // 킹사이드 or 퀸사이드
			ChessPiece rook = board[rookX, move.FromY];

			// 룩이 존재하고 안 움직였는가?
			if (rook.Type != PieceType.Rook || rook.MoveCount > 0)
				return false;

			// 사이에 기물이 없는가?
			int step = move.ToX > move.FromX ? 1 : -1;
			for (int x = move.FromX + step; x != rookX; x += step)
			{
				if (board[x, move.FromY].Type != PieceType.None)
					return false;
			}

			// 킹이 지나가는 모든 칸이 공격받지 않는가?
			for (int x = move.FromX; x != move.ToX + step; x += step)
			{
				if (IsSquareUnderAttack(board, x, move.FromY, king.Color))
					return false;
			}

			return true;
		}

		// ============================================
		// 12. 경로 확인 (비숍, 룩, 퀸)
		// ============================================

		private static bool IsPathClear(ChessPiece[,] board, ChessMove move)
		{
			int dx = move.ToX - move.FromX;
			int dy = move.ToY - move.FromY;

			int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
			int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

			int x = move.FromX + stepX;
			int y = move.FromY + stepY;

			while (x != move.ToX || y != move.ToY)
			{
				if (board[x, y].Type != PieceType.None)
					return false;

				x += stepX;
				y += stepY;
			}

			return true;
		}

		// ============================================
		// 13. 체크 판정
		// ============================================

		/// <summary>
		/// 특정 색상의 킹이 체크 상태인가?
		/// </summary>
		public static bool IsKingInCheck(ChessPiece[,] board, PlayerColor kingColor)
		{
			// 킹 위치 찾기
			(int kingX, int kingY) = FindKing(board, kingColor);

			if (kingX == -1)
				return false; // 킹이 없음 (테스트용)

			// 킹이 공격받는가?
			return IsSquareUnderAttack(board, kingX, kingY, kingColor);
		}

		/// <summary>
		/// 특정 칸이 공격받는가?
		/// </summary>
		private static bool IsSquareUnderAttack(
			ChessPiece[,] board,
			int x, int y,
			PlayerColor defenderColor)
		{
			PlayerColor attackerColor = defenderColor == PlayerColor.White
				? PlayerColor.Black
				: PlayerColor.White;

			// 모든 적 기물이 이 칸을 공격할 수 있는가?
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					ChessPiece piece = board[i, j];
					if (piece.Type == PieceType.None || piece.Color != attackerColor)
						continue;

					ChessMove testMove = new ChessMove
					{
						FromX = (byte)i,
						FromY = (byte)j,
						ToX = (byte)x,
						ToY = (byte)y
					};

					// 기본 검증 없이 순수 이동 규칙만 체크
					if (IsValidPieceMoveIgnoreCheck(board, testMove, piece))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// 체크 무시하고 순수 이동만 검증 (재귀 방지용)
		/// </summary>
		private static bool IsValidPieceMoveIgnoreCheck(
			ChessPiece[,] board,
			ChessMove move,
			ChessPiece piece)
		{
			switch (piece.Type)
			{
				case PieceType.Pawn:
					return IsValidPawnAttack(move, piece);
				case PieceType.Knight:
					return IsValidKnightMove(move);
				case PieceType.Bishop:
					return IsValidBishopMove(board, move);
				case PieceType.Rook:
					return IsValidRookMove(board, move);
				case PieceType.Queen:
					return IsValidQueenMove(board, move);
				case PieceType.King:
					int dx = Mathf.Abs(move.ToX - move.FromX);
					int dy = Mathf.Abs(move.ToY - move.FromY);
					return dx <= 1 && dy <= 1;
				default:
					return false;
			}
		}

		private static bool IsValidPawnAttack(ChessMove move, ChessPiece piece)
		{
			int direction = piece.Color == PlayerColor.White ? 1 : -1;
			int dx = Mathf.Abs(move.ToX - move.FromX);
			int dy = move.ToY - move.FromY;

			return dx == 1 && dy == direction;
		}

		// ============================================
		// 14. 자살수 방지
		// ============================================

		/// <summary>
		/// 이동 후 자신의 킹이 체크 상태가 되는가?
		/// </summary>
		private static bool WouldBeInCheck(
			ChessPiece[,] board,
			ChessMove move,
			PlayerColor currentPlayer)
		{
			// 가상으로 이동 실행
			ChessPiece[,] tempBoard = (ChessPiece[,])board.Clone();
			ExecuteMove(tempBoard, move);

			// 체크인가?
			return IsKingInCheck(tempBoard, currentPlayer);
		}

		// ============================================
		// 15. 체크메이트 / 스테일메이트 판정
		// ============================================

		/// <summary>
		/// 현재 플레이어가 체크메이트 상태인가?
		/// </summary>
		public static bool IsCheckmate(ChessPiece[,] board, PlayerColor currentPlayer)
		{
			if (!IsKingInCheck(board, currentPlayer))
				return false;

			return !HasLegalMoves(board, currentPlayer);
		}

		/// <summary>
		/// 현재 플레이어가 스테일메이트 상태인가?
		/// </summary>
		public static bool IsStalemate(ChessPiece[,] board, PlayerColor currentPlayer)
		{
			if (IsKingInCheck(board, currentPlayer))
				return false;

			return !HasLegalMoves(board, currentPlayer);
		}

		/// <summary>
		/// 합법적인 이동이 하나라도 있는가?
		/// </summary>
		private static bool HasLegalMoves(ChessPiece[,] board, PlayerColor currentPlayer)
		{
			for (int fromX = 0; fromX < 8; fromX++)
			{
				for (int fromY = 0; fromY < 8; fromY++)
				{
					ChessPiece piece = board[fromX, fromY];
					if (piece.Type == PieceType.None || piece.Color != currentPlayer)
						continue;

					for (int toX = 0; toX < 8; toX++)
					{
						for (int toY = 0; toY < 8; toY++)
						{
							ChessMove testMove = new ChessMove
							{
								FromX = (byte)fromX,
								FromY = (byte)fromY,
								ToX = (byte)toX,
								ToY = (byte)toY
							};

							if (IsValidMove(board, testMove, currentPlayer))
								return true;
						}
					}
				}
			}

			return false;
		}

		// ============================================
		// 16. 헬퍼 함수
		// ============================================

		private static bool IsInBounds(int x, int y)
		{
			return x >= 0 && x < 8 && y >= 0 && y < 8;
		}

		private static (int x, int y) FindKing(ChessPiece[,] board, PlayerColor color)
		{
			for (int x = 0; x < 8; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					ChessPiece piece = board[x, y];
					if (piece.Type == PieceType.King && piece.Color == color)
						return (x, y);
				}
			}
			return (-1, -1);
		}

		private static void ExecuteMove(ChessPiece[,] board, ChessMove move)
		{
			board[move.ToX, move.ToY] = board[move.FromX, move.FromY];
			board[move.FromX, move.FromY] = new ChessPiece { Type = PieceType.None };
		}

		/// <summary>
		/// 가능한 모든 합법적 이동 반환 (AI용)
		/// </summary>
		public static List<ChessMove> GetAllLegalMoves(
			ChessPiece[,] board,
			PlayerColor currentPlayer)
		{
			List<ChessMove> moves = new List<ChessMove>();

			for (int fromX = 0; fromX < 8; fromX++)
			{
				for (int fromY = 0; fromY < 8; fromY++)
				{
					ChessPiece piece = board[fromX, fromY];
					if (piece.Type == PieceType.None || piece.Color != currentPlayer)
						continue;

					for (int toX = 0; toX < 8; toX++)
					{
						for (int toY = 0; toY < 8; toY++)
						{
							ChessMove move = new ChessMove
							{
								FromX = (byte)fromX,
								FromY = (byte)fromY,
								ToX = (byte)toX,
								ToY = (byte)toY
							};

							if (IsValidMove(board, move, currentPlayer))
								moves.Add(move);
						}
					}
				}
			}

			return moves;
		}
	}
}
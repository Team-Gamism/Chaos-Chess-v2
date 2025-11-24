using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ChessGame : MonoBehaviour
{
	private const int BOARD_SIZE = 8;

	private ChessPiece[,] board = new ChessPiece[BOARD_SIZE, BOARD_SIZE];
	private PlayerColor currentColor = PlayerColor.White;
	private GameState gameState;

	[SerializeField] private float gap = 0.1f;
	[SerializeField] private float size = 1f;
	[SerializeField] private Vector3 offset;
	[SerializeField] private Sprite[] sprites;
	[SerializeField] private Sprite boardSprite;

	[SerializeField] private Material white;
	[SerializeField] private Material black;

	private void Awake()
	{
		Initialize();
	}


	private void Initialize()
	{
		for(int x = 0; x < BOARD_SIZE; x++)
		{
			for(int y = 0; y < BOARD_SIZE; y++)
			{
				board[x, y] = new ChessPiece { Type = PieceType.None };
			}
		}

		SetupPieces(PlayerColor.White, 7, 6);
		SetupPieces(PlayerColor.Black, 0, 1);

		spawnObjects();
	}

	private void SetupPieces(PlayerColor color, int backRow, int pawnRow)
	{
		for(int x = 0; x < BOARD_SIZE; x++)
		{
			board[x, pawnRow].Type = PieceType.Pawn;
			board[x, pawnRow].Color = color;
			board[x, backRow].Color = color;
		}

		board[0, backRow].Type = PieceType.Rook;
		board[1, backRow].Type = PieceType.Knight;
		board[2, backRow].Type = PieceType.Bishop;
		board[3, backRow].Type = PieceType.Queen;
		board[4, backRow].Type = PieceType.King;
		board[5, backRow].Type = PieceType.Bishop;
		board[6, backRow].Type = PieceType.Knight;
		board[7, backRow].Type = PieceType.Rook;
	}

	private void spawnObjects()
	{
		GameObject pieces = new GameObject("Pieces");
		GameObject boards = new GameObject("Boards");

		pieces.transform.SetParent(transform);
		boards.transform.SetParent(transform);

		for (int x = 0; x < BOARD_SIZE; x++)
		{
			for (int y = 0; y < BOARD_SIZE; y++)
			{
				GameObject boardObj = new GameObject("board" + x + " " + y);
				GameObject obj = new GameObject("piece"+ x + " " + y);
				obj.transform.SetParent(pieces.transform);
				boardObj.transform.SetParent(boards.transform);

				SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
				SpriteRenderer boardSR = boardObj.AddComponent<SpriteRenderer>();

				sr.sprite = sprites[(int)board[x, y].Type];
				boardSR.sprite = boardSprite;
				boardSR.material = (x + y) % 2 == 0 ? white : black;

				Vector3 pos = new Vector3(x * gap, y * gap, 0f);
				obj.transform.position = pos + offset;
				obj.transform.localScale = Vector3.one * size;

				boardObj.transform.position = pos + offset;
				boardObj.transform.localScale = Vector3.one * size;

			}
		}
	}
}

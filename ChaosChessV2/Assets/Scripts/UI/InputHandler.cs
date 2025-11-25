using ChaosChess.Core;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
	private ChessMove move;
	private VisualChessPiece piece;

	[SerializeField] private ChessGame game;
	[SerializeField] private bool isSelected = false;

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
						// 터치한 오브젝트가 말일 경우에만 선택
						piece = hit.collider.GetComponent<VisualChessPiece>();
						if (piece != null) isSelected = true;

						Vector2 vec = hit.transform.position;
						vec = game.GetCoordFromTransform(vec);
						Debug.Log($"{vec.x} {vec.y}");
						move.FromX = (byte)vec.x;
						move.FromY = (byte)vec.y;

					}
					else if (hit.collider.CompareTag("Board"))
					{
						// 터치한 위치를 무조건 도착 지점으로 설정
						Vector2 vec = hit.transform.position;
						vec = game.GetCoordFromTransform(vec);
						Debug.Log($"{vec.x} {vec.y}");
						move.ToX = (byte)vec.x;
						move.ToY = (byte)vec.y;

						// 이동을 시도하고 성공/실패 여부에 관계없이 선택 해제
						game.TryMove(move, piece);
						piece = null;
						isSelected = false;
					}
				}
				// ...
			}
		}
	}
}

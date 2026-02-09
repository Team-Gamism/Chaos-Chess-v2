using Fusion;
using UnityEngine;

public class GameSession : NetworkBehaviour
{
    public static GameSession Instance;

    [Networked] public int CurrentTurnPlayerID { get; set; }
    [Networked, Capacity(64)] public NetworkArray<TileState> BoardStates { get; }

	public override void Spawned() => Instance = this;

    public void SwitchTurn()
    {
		CurrentTurnPlayerID = (CurrentTurnPlayerID == 0) ? 1 : 0;

		// 타일 봉쇄 턴 감소 로직
		for (int i = 0; i < 64; i++)
		{
			if (BoardStates[i].IsBlocked)
			{
				var state = BoardStates[i];
				state.RemainingTurns--;
				if (state.RemainingTurns <= 0) state.IsBlocked = false;
				BoardStates.Set(i, state);
			}
		}
	}

    void CheckViectoryCondition()
    {

    }
}

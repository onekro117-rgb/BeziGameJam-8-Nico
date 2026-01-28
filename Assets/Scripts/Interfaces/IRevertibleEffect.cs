using UnityEngine;

public interface IRevertibleEffect
{
    void Revert(GameManager gameManager);
    string DebugName { get; }
}


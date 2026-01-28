using UnityEngine;

[CreateAssetMenu(menuName = "Modifiers/Bad/Mirror Controls")]
public class MirrorControlsModifier : ModifierData
{
    public override IRevertibleEffect Apply(GameManager gameManager)
    {
        if (gameManager == null) return null;

        var pm = gameManager.PlayerMovement;
        if (pm == null)
        {
            Debug.LogError("MirrorControlsModifier: No encuentro GameManager.PlayerMovement.");
            return null;
        }

        // Stack-safe: cada Apply hace Push, cada Revert hace Pop
        pm.PushInvertHorizontal();

        return new MirrorEffect();
    }

    private class MirrorEffect : IRevertibleEffect
    {
        public string DebugName => "Mirror Controls";

        public void Revert(GameManager gameManager)
        {
            if (gameManager == null) return;

            var pm = gameManager.PlayerMovement;
            if (pm == null) return;

            pm.PopInvertHorizontal();
        }
    }
}




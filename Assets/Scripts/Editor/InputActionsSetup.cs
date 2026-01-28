using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class InputActionsSetup
{
    static InputActionsSetup()
    {
        EditorApplication.delayCall += SetupInputActions;
    }
    
    [MenuItem("Tools/Setup Magic Input Actions")]
    public static void SetupMagicInputActionsMenu()
    {
        SetupInputActions();
        Debug.Log("✅ <b>Input Actions configurado manualmente!</b>");
    }

    private static void SetupInputActions()
    {
        string assetPath = "Assets/Inputs/PlayerInputActions.inputactions";
        InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);
        
        if (inputActions == null)
        {
            Debug.LogWarning($"InputActions asset not found at {assetPath}");
            return;
        }

        InputActionMap playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogWarning("Player action map not found");
            return;
        }

        bool needsSave = false;

        
        if (playerMap.FindAction("Magic1") == null)
        {
            playerMap.AddAction("Magic1", InputActionType.Button);
            var magic1Action = playerMap.FindAction("Magic1");
            magic1Action.AddBinding("<Keyboard>/a");
            needsSave = true;
            Debug.Log("Added Magic1 action (A)");
        }
        
        if (playerMap.FindAction("Magic2") == null)
        {
            playerMap.AddAction("Magic2", InputActionType.Button);
            var magic2Action = playerMap.FindAction("Magic2");
            magic2Action.AddBinding("<Keyboard>/s");
            needsSave = true;
            Debug.Log("Added Magic2 action (S)");
        }
        
        if (playerMap.FindAction("Magic3") == null)
        {
            playerMap.AddAction("Magic3", InputActionType.Button);
            var magic3Action = playerMap.FindAction("Magic3");
            magic3Action.AddBinding("<Keyboard>/d");
            needsSave = true;
            Debug.Log("Added Magic3 action (D)");
        }
        
        var attackAction = playerMap.FindAction("Attack");
        if (attackAction != null && attackAction.bindings.Count > 0)
        {
            string currentPath = attackAction.bindings[0].effectivePath;
            if (currentPath != "<Keyboard>/f")
            {
                attackAction.ChangeBinding(0).WithPath("<Keyboard>/f");
                needsSave = true;
                Debug.Log("Changed Attack to F key");
            }
        }

        if (playerMap.FindAction("QTEButton1") == null)
        {
            playerMap.AddAction("QTEButton1", InputActionType.Button);
            var qteButton1Action = playerMap.FindAction("QTEButton1");
            qteButton1Action.AddBinding("<Keyboard>/q");
            needsSave = true;
            Debug.Log("Added QTEButton1 action");
        }

        if (playerMap.FindAction("QTEButton2") == null)
        {
            playerMap.AddAction("QTEButton2", InputActionType.Button);
            var qteButton2Action = playerMap.FindAction("QTEButton2");
            qteButton2Action.AddBinding("<Keyboard>/w");
            needsSave = true;
            Debug.Log("Added QTEButton2 action");
        }

        if (playerMap.FindAction("QTEButton3") == null)
        {
            playerMap.AddAction("QTEButton3", InputActionType.Button);
            var qteButton3Action = playerMap.FindAction("QTEButton3");
            qteButton3Action.AddBinding("<Keyboard>/e");
            needsSave = true;
            Debug.Log("Added QTEButton3 action");
        }

        if (playerMap.FindAction("QTEButton4") == null)
        {
            playerMap.AddAction("QTEButton4", InputActionType.Button);
            var qteButton4Action = playerMap.FindAction("QTEButton4");
            qteButton4Action.AddBinding("<Keyboard>/r");
            needsSave = true;
            Debug.Log("Added QTEButton4 action");
        }

        if (needsSave)
        {
            EditorUtility.SetDirty(inputActions);
            AssetDatabase.SaveAssets();
            Debug.Log("Input Actions updated successfully!");
        }
    }
}
#endif

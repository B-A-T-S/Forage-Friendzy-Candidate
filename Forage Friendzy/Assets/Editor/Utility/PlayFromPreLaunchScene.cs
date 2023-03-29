using UnityEditor;
using UnityEngine;

public class PlayFromPreLaunchScene : ScriptableObject
{
    [MenuItem("Edit/Play-Stop Build Index 0 %j")]
    public static void PlayFromBuildIndexZero()
    {
        if (EditorApplication.isPlaying == true)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorApplication.SaveCurrentSceneIfUserWantsTo();
        EditorApplication.OpenScene(EditorBuildSettings.scenes[0].path);
        EditorApplication.isPlaying = true;
    }
}
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
public class PlayFromScene : EditorWindow {

    [SerializeField] string lastScene = "";
    [SerializeField] int targetScene = 0;
    [SerializeField] string waitScene = null;
    [SerializeField] bool hasPlayed = false;

    static string[] sceneNames;
    static EditorBuildSettingsScene[] scenes;

    [MenuItem("Window/Play From Scene %l")]
    public static void Run()
    {
        EditorWindow.GetWindow<PlayFromScene>();
    }

    //On Enable, get the list of scenes from the build scenes array and populate an array with their names
    void OnEnable()
    {
        scenes = EditorBuildSettings.scenes;
        sceneNames = scenes.Select(x => AsSpacedCamelCase(Path.GetFileNameWithoutExtension(x.path))).ToArray();
    }
    
    //first available frame, check if the editor is not playing and load lastscene
    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            if (waitScene == null && !string.IsNullOrEmpty(lastScene))
            {
                EditorApplication.OpenScene(lastScene);
                lastScene = null;
            }
        }
    }

    void OnGUI()
    {
        //if we are in PLAY mode
        if (EditorApplication.isPlaying)
        {
            //if we are currently at the scene we wanted to load...
            if (EditorApplication.currentScene == waitScene)
            {
                waitScene = null;
            }
            //do not run any other code
            return;
        }

        //to reach here, we must be in EDIT mode
        
        //if the current scene is the selected one, transition to PLAY mode
        //wait scene is non-null only when the Play Button on the window is used
        if (EditorApplication.currentScene == waitScene)
        {
            EditorApplication.isPlaying = true;
        }

        //there are no scenes in the build index
        if (sceneNames == null) 
            return;

        //UI for selecting the scene to load
        targetScene = EditorGUILayout.Popup(targetScene, sceneNames);
        if (GUILayout.Button("Play"))
        {
            //remember current scene
            lastScene = EditorApplication.currentScene;
            //set waitScene to the scene selected via the popup
            waitScene = scenes[targetScene].path;
            //save and load the scene the user wanted
            EditorApplication.SaveCurrentSceneIfUserWantsTo();
            EditorApplication.OpenScene(waitScene);
        }
    }

    //returns given text in spaced camel case (AuthScene -> Auth Scene)
    public string AsSpacedCamelCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("A Deleted Scene Reference is present in the Build Index List. Please remove before using PlayFromScene.");
            return "";
        }
            
        System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length * 2);
        sb.Append(char.ToUpper(text[0]));
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                sb.Append(' ');
            sb.Append(text[i]);
        }
        return sb.ToString();
    }
} 

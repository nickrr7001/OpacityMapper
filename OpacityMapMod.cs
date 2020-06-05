using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

public class OpacityMapMod : EditorWindow
{
    private Texture2D _mainTexture;
    private Texture2D _opacityTexture;
    [MenuItem("Tools/OpacityMapper")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(OpacityMapMod));
    }
    private void OnGUI()
    {
        _mainTexture = (Texture2D)EditorGUILayout.ObjectField("mainTexture", _mainTexture, typeof(Texture2D), false);
        _opacityTexture = (Texture2D)EditorGUILayout.ObjectField("opacityTexture", _opacityTexture, typeof(Texture2D), false);
        if (GUILayout.Button("Apply Opacity"))
        {
            Color[] mainPixels = _mainTexture.GetPixels();
            Color[] opacityPixels = _opacityTexture.GetPixels();
            
            int threads = 8;
            int parts = mainPixels.Length / threads;
            Task[] tasks = new Task[threads];
            for (int i = 0; i < threads; i++)
            {
                var localThread = i;
                tasks[localThread] = Task.Run(() => 
                {
                    for (int j = 0; j < parts; j++)
                    {
                        mainPixels[j * (localThread+1)].a = opacityPixels[j * (localThread + 1)].grayscale;
                    }
                });
            }
            Task.WaitAll(tasks);
            Debug.Log(mainPixels[0].a);
            _mainTexture.SetPixels(mainPixels);
            _mainTexture.Apply();
            string path = Application.dataPath + "/" + "ModTexture.png";
            Debug.Log("Texture Saved to: " + path);
            File.WriteAllBytes(path, _mainTexture.EncodeToPNG());
        }
    }
}

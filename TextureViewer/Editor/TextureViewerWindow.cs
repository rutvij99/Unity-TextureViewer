/*
* Author - Rutvij
*/


using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace TextureViewer
{

    public class TextureViewerWindow : EditorWindow
    {
        public string folderPath;
        private List<Texture2D> textures = new List<Texture2D>();
        private Vector2 scrollPos;
        private int sortColumn = 0;
        private bool sortAscending = true;
        private string filterName = "";
        private int filterWidth = -1;
        private int filterHeight = -1;
        private string filterFormat = "";
        private string filterPoT = "";
        private int filterSize = -1;
        
        
        private List<Texture2D> filteredTextures = new List<Texture2D>();
        private bool isFilterApplied = false;



        [MenuItem("Tools/Texture Viewer")]
        public static void ShowWindow()
        {
            GetWindow<TextureViewerWindow>("Texture Viewer");
            
        }

        private void OnEnable()
        {
            minSize = new Vector2(800, 600);
            maxSize = new Vector2(800, 600);
        }


        private void OnGUI()
        {
            folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

            if (GUILayout.Button("Load Textures"))
            {
                if (textures == null) textures = new List<Texture2D>();
                textures.Clear();
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { folderPath });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                    if(texture != null)
                        textures.Add(texture);
                }
                filteredTextures = textures;
            }


            //scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Preview", EditorStyles.boldLabel, GUILayout.Width(100));

            if (GUILayout.Button("Name", GUILayout.Width(250)))
            {
                sortColumn = 0;
                sortAscending = !sortAscending;
            }
            if (GUILayout.Button("Width", GUILayout.Width(50)))
            {
                sortColumn = 1;
                sortAscending = !sortAscending;
            }
            if (GUILayout.Button("Height", GUILayout.Width(50)))
            {
                sortColumn = 2;
                sortAscending = !sortAscending;
            }
            if (GUILayout.Button("Format", GUILayout.Width(110)))
            {
                sortColumn = 3;
                sortAscending = !sortAscending;
            }
            if (GUILayout.Button("Size", GUILayout.Width(60)))
            {
                sortColumn = 4;
                sortAscending = !sortAscending;
            }
            if (GUILayout.Button("PoT/NPoT", GUILayout.Width(75)))
            {
                sortColumn = 4;
                sortAscending = !sortAscending;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            EditorGUILayout.BeginVertical();
            
            if (isFilterApplied)
            {
                if (GUILayout.Button("Clear Filter"))
                {
                    isFilterApplied = false;
                    filteredTextures = textures;
                }
            }
            else
            {
                if (GUILayout.Button("Apply Filter"))
                {
                    isFilterApplied = true;
                    filteredTextures = textures.Where(x =>
                    (string.IsNullOrEmpty(filterName) || x.name.ToLower().Contains(filterName.ToLower())) &&
                    (!string.IsNullOrEmpty(filterPoT) && (
                    (x.IsPowerOfTwo() && !filterPoT.ToLower().Contains("npot"))
                     || (!x.IsPowerOfTwo() && filterPoT.ToLower().Contains("npot"))
                    ))
                    && (filterWidth == -1 || x.width == filterWidth) &&
                    (filterHeight == -1 || x.height == filterHeight) &&
                    (string.IsNullOrEmpty(filterFormat) || x.format.ToString().ToLower().Contains(filterFormat.ToLower()))).ToList();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            filterName = EditorGUILayout.TextField("", filterName, GUILayout.Width(250));
            filterWidth = EditorGUILayout.IntField("", filterWidth, GUILayout.Width(50));
            filterHeight = EditorGUILayout.IntField("", filterHeight, GUILayout.Width(50));
            filterFormat = EditorGUILayout.TextField("", filterFormat, GUILayout.Width(110));
            filterSize = EditorGUILayout.IntField("", filterSize, GUILayout.Width(60));
            filterPoT = EditorGUILayout.TextField("", filterPoT, GUILayout.Width(75));

            EditorGUILayout.EndHorizontal();

            if (filteredTextures != null && filteredTextures.Count > 0)
            {
                switch (sortColumn)
                {
                    case 0:
                        if (sortAscending)
                            filteredTextures = filteredTextures.OrderBy(x => x.name).ToList();
                        else
                            filteredTextures = filteredTextures.OrderByDescending(x => x.name).ToList();
                        break;
                    case 1:
                        if (sortAscending)
                            filteredTextures = filteredTextures.OrderBy(x => x.width).ToList();
                        else
                            filteredTextures = filteredTextures.OrderByDescending(x => x.width).ToList();
                        break;
                    case 2:
                        if (sortAscending)
                            filteredTextures = filteredTextures.OrderBy(x => x.height).ToList();
                        else
                            filteredTextures = filteredTextures.OrderByDescending(x => x.height).ToList();
                        break;
                    case 3:
                        if (sortAscending)
                            filteredTextures = filteredTextures.OrderBy(x => x.format).ToList();
                        else
                            filteredTextures = filteredTextures.OrderByDescending(x => x.format).ToList();
                        break;
                    case 4:
                        if (sortAscending)
                            filteredTextures = filteredTextures.OrderBy(x => Profiler.GetRuntimeMemorySizeLong(x)).ToList();
                        else
                            filteredTextures = filteredTextures.OrderByDescending(x => Profiler.GetRuntimeMemorySizeLong(x)).ToList();
                        break;
                }
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                //EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < filteredTextures.Count; i++)
                {
                    if (filteredTextures[i]!= null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        //GUILayout.Space(25);
                        GUILayout.Box(filteredTextures[i], GUILayout.Width(25), GUILayout.Height(25));
                        if (GUILayout.Button("Select", GUILayout.Width(50)))
                        {
                            string path = AssetDatabase.GetAssetPath(filteredTextures[i]);
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                            EditorGUIUtility.PingObject(Selection.activeObject);
                        }
                        //GUILayout.FlexibleSpace();
                        GUILayout.Space(20);
                        GUILayout.Label(filteredTextures[i].name, GUILayout.Width(250));
                        GUILayout.Label(filteredTextures[i].width.ToString(), GUILayout.Width(50));
                        GUILayout.Label(filteredTextures[i].height.ToString(), GUILayout.Width(50));
                        GUILayout.Label(filteredTextures[i].format.ToString(), GUILayout.Width(110));
                        GUILayout.Label(EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySizeLong(filteredTextures[i])), GUILayout.Width(60));
                        GUILayout.Label(filteredTextures[i].IsPowerOfTwo()? "PoT": "NPoT", GUILayout.Width(75));
                        EditorGUILayout.EndHorizontal();
                    }
                }
                //EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label($"Total showing {filteredTextures.Count} of {textures.Count} textures found in \"{folderPath}\"");
                EditorGUILayout.EndHorizontal();

            }

            EditorGUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();
        }
    }

    public static class NPOTExtensions
    {
        public static bool IsPowerOfTwo(this int i)
        {
            return ((i & (i - 1)) == 0);
        }
        public static bool IsPowerOfTwo(this Texture t)
        {
            return t.width.IsPowerOfTwo() && t.height.IsPowerOfTwo() && t.height == t.width;
        }

    }
}
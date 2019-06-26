using UnityEngine;
using UnityEditor;
using System.Collections;

namespace WPMF {

				public class WorldMap2DAbout : EditorWindow {
								Texture2D _headerTexture;
								GUIStyle richLabelStyle;
								Vector2 readmeScroll = Misc.Vector2zero;
								string readmeText;

								public static void ShowAboutWindow () {
												float height = 550.0f;
												float width = 600.0f;

												Rect rect = new Rect (Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
												GetWindowWithRect<WorldMap2DAbout> (rect, true, "About World Political Map 2D Edition", true);
								}


								void OnEnable () {
												_headerTexture = Resources.Load<Texture2D> ("WPMF/EditorHeader");

												// load readme.txt
												readmeText = System.IO.File.ReadAllText (GetAssetPath () + "/README.txt");
								}

								void OnGUI () {
												if (richLabelStyle == null) {
																richLabelStyle = new GUIStyle (GUI.skin.label);
																richLabelStyle.richText = true;
																richLabelStyle.wordWrap = true;
												}

												EditorGUILayout.Separator ();
												GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
												GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
												GUI.skin.label.alignment = TextAnchor.MiddleLeft;  
												EditorGUILayout.Separator ();

												EditorGUILayout.Separator ();
												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label ("<b>World Political Map 2D Edition</b>\nCopyright (C) by Kronnect", richLabelStyle);
												EditorGUILayout.EndHorizontal ();
												EditorGUILayout.Separator ();
												GUILayout.Label ("Thanks for purchasing!");
												EditorGUILayout.Separator ();

												EditorGUILayout.BeginHorizontal ();
												GUILayout.FlexibleSpace ();
												readmeScroll = GUILayout.BeginScrollView (readmeScroll, GUILayout.Width (Screen.width * 0.95f));
												GUILayout.Label (readmeText, richLabelStyle);
												GUILayout.EndScrollView ();
												GUILayout.FlexibleSpace ();
												EditorGUILayout.EndHorizontal ();

												EditorGUILayout.Separator ();
												EditorGUILayout.Separator ();

												EditorGUILayout.BeginHorizontal ();
												if (GUILayout.Button ("Support Forum and more assets!", GUILayout.Height (40))) {
																Application.OpenURL ("http://kronnect.me");
												}
												if (GUILayout.Button ("Rate this Asset", GUILayout.Height (40))) {
																Application.OpenURL ("com.unity3d.kharma:content/43180");
												}
												if (GUILayout.Button ("Close Window", GUILayout.Height (40))) {
																Close ();
												}
												EditorGUILayout.EndHorizontal ();
												EditorGUILayout.Separator ();

								}



		
								string GetAssetPath () {
												// Proceed and restore
												string[] paths = AssetDatabase.GetAllAssetPaths ();
												for (int k = 0; k < paths.Length; k++) {
																if (paths [k].EndsWith ("WorldPoliticalMap2DEdition")) {
																				return paths [k];
																}
												}
												return "";
								}
				}

}
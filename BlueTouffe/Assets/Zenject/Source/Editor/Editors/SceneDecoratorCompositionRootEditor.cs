using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using ModestTree.Util;
using UnityEngine.SceneManagement;
using Zenject;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;
using ModestTree;

namespace Zenject
{
    [CustomEditor(typeof(SceneDecoratorCompositionRoot))]
    public class SceneDecoratorCompositionRootEditor : UnityEditor.Editor
    {
        List<ReorderableList> _propLists;

        public virtual void OnEnable()
        {
            _propLists = new List<ReorderableList>();

            var names = new string[]
            {
                "DecoratorInstallers",
                "PreInstallers",
                "PostInstallers"
            };

            var descriptions = new string[]
            {
                "List of installers that derive from DecoratorInstaller",
                "List of installers that are executed BEFORE the installers in the main scene",
                "List of installers that are executed AFTER the installers in the main scene",
            };

            Assert.IsEqual(descriptions.Length, names.Length);

            var infos = Enumerable.Range(0, names.Length).Select(i => new { Name = names[i], Description = descriptions[i] }).ToList();

            foreach (var info in infos)
            {
                var prop = serializedObject.FindProperty(info.Name);

                ReorderableList reorderableList = new ReorderableList(serializedObject, prop, true, true, true, true);
                _propLists.Add(reorderableList);

                var closedName = info.Name;
                var closedDesc = info.Description;

                reorderableList.drawHeaderCallback += rect =>
                {
                    GUI.Label(rect,
                        new GUIContent(closedName, closedDesc));
                };

                reorderableList.drawElementCallback += (rect, index, active, focused) =>
                {
                    rect.width -= 40;
                    rect.x += 20;
                    EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index), GUIContent.none, true);
                };
            }
        }

        SceneDecoratorCompositionRoot TryGetSceneDecoratorCompositionRoot()
        {
            return GameObject.FindObjectsOfType<SceneDecoratorCompositionRoot>().OnlyOrDefault();
        }

        SceneCompositionRoot TryGetSceneCompositionRoot()
        {
            return GameObject.FindObjectsOfType<SceneCompositionRoot>().OnlyOrDefault();
        }

        void SelectCompositionRoot(Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            var sceneCompRoot = rootObjects
                .SelectMany(x => x.GetComponentsInChildren<SceneCompositionRoot>()).FirstOrDefault();

            if (sceneCompRoot != null)
            {
                Selection.activeGameObject = sceneCompRoot.gameObject;
            }
            else
            {
                var decoratorCompRoot = rootObjects
                    .SelectMany(x => x.GetComponentsInChildren<SceneDecoratorCompositionRoot>()).FirstOrDefault();

                if (decoratorCompRoot != null)
                {
                    Selection.activeGameObject = decoratorCompRoot.gameObject;
                }
            }
        }

        string TryGetScenePath(string sceneName)
        {
            return UnityEditor.EditorBuildSettings.scenes.Select(x => x.path)
                .Where(x => Path.GetFileNameWithoutExtension(x) == sceneName).OnlyOrDefault();
        }

        void OpenNextScene(OpenSceneMode openMode)
        {
            var binder = target as SceneDecoratorCompositionRoot;
            var scenePath = TryGetScenePath(binder.SceneName);

            if (scenePath == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Could not find scene with name '{0}'.  Is it added to your build settings?".Fmt(binder.SceneName), "Ok");
            }
            else
            {
                if (openMode == OpenSceneMode.Single)
                {
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        return;
                    }
                }

                var scene = EditorSceneManager.OpenScene(scenePath, openMode);
                SelectCompositionRoot(scene);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }

            GUILayout.Space(5);

            var binder = target as SceneDecoratorCompositionRoot;

            EditorGUILayout.BeginHorizontal();
            {
                binder.SceneName = EditorGUILayout.TextField("Decorated Scene", binder.SceneName);

                GUILayout.Space(10);

                if (GUILayout.Button("Open", GUILayout.MaxWidth(50)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        OpenNextScene(OpenSceneMode.Single);
                    };
                }

                if (GUILayout.Button("Add", GUILayout.MaxWidth(50)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        OpenNextScene(OpenSceneMode.Additive);
                    };
                }
            }
            EditorGUILayout.EndHorizontal();

            foreach (var list in _propLists)
            {
                list.DoLayoutList();
            }

            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}

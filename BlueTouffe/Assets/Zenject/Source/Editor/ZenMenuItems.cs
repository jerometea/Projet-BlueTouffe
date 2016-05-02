#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModestTree.Util;
using UnityEditor;
using UnityEngine;
using ModestTree;
using Zenject.Internal;
using UnityEditor.SceneManagement;

namespace Zenject
{
    public static class ZenMenuItems
    {
        [MenuItem("Edit/Zenject/Validate Current Scene #%v")]
        public static void ValidateCurrentScene()
        {
            ProjectCompositionRoot.PersistentIsValidating = true;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Edit/Zenject/Help...")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/modesttree/zenject");
        }

        [MenuItem("GameObject/Zenject/Scene Composition Root", false, 9)]
        public static void CreateSceneCompositionRoot(MenuCommand menuCommand)
        {
            var root = new GameObject("SceneCompositionRoot").AddComponent<SceneCompositionRoot>();
            Selection.activeGameObject = root.gameObject;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Zenject/Decorator Composition Root", false, 9)]
        public static void CreateDecoratorCompositionRoot(MenuCommand menuCommand)
        {
            var root = new GameObject("DecoratorCompositionRoot").AddComponent<SceneDecoratorCompositionRoot>();
            Selection.activeGameObject = root.gameObject;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Zenject/Game Object Composition Root", false, 9)]
        public static void CreateGameObjectCompositionRoot(MenuCommand menuCommand)
        {
            var root = new GameObject("GameObjectCompositionRoot").AddComponent<GameObjectCompositionRoot>();
            Selection.activeGameObject = root.gameObject;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Edit/Zenject/Create Project Composition Root")]
        public static void CreateProjectCompositionRootInDefaultLocation()
        {
            var fullDirPath = Path.Combine(Application.dataPath, "Resources");

            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }

            CreateProjectCompositionRootInternal("Assets/Resources");
        }

        [MenuItem("Assets/Create/Zenject/Project Composition Root")]
        public static void CreateProjectCompositionRoot()
        {
            var dir = ZenUnityEditorUtil.TryGetSelectedFolderPathInProjectsTab();

            if (dir == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Could not find directory to place the '{0}.prefab' asset.  Please try again by right clicking in the desired folder within the projects pane."
                    .Fmt(ProjectCompositionRoot.ProjectCompRootResourcePath), "Ok");
                return;
            }

            var parentFolderName = Path.GetFileName(dir);

            if (parentFolderName != "Resources")
            {
                EditorUtility.DisplayDialog("Error",
                    "'{0}.prefab' must be placed inside a directory named 'Resources'.  Please try again by right clicking within the Project pane in a valid Resources folder."
                    .Fmt(ProjectCompositionRoot.ProjectCompRootResourcePath), "Ok");
                return;
            }

            CreateProjectCompositionRootInternal(dir);
        }

        static void CreateProjectCompositionRootInternal(string dir)
        {
            var prefabPath = (Path.Combine(dir, ProjectCompositionRoot.ProjectCompRootResourcePath) + ".prefab").Replace("\\", "/");
            var emptyPrefab = PrefabUtility.CreateEmptyPrefab(prefabPath);

            var gameObject = new GameObject();

            try
            {
                gameObject.AddComponent<ProjectCompositionRoot>();

                var prefabObj = PrefabUtility.ReplacePrefab(gameObject, emptyPrefab);

                Selection.activeObject = prefabObj;
            }
            finally
            {
                GameObject.DestroyImmediate(gameObject);
            }

            Debug.Log("Created new ProjectCompositionRoot at '{0}'".Fmt(prefabPath));
        }
    }
}
#endif



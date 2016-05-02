using System;
using System.IO;
using ModestTree;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public abstract class EditorWindowFacade : EditorWindow
    {
        [Inject]
        TickableManager _tickableManager = null;

        [Inject]
        InitializableManager _initializableManager = null;

        [Inject]
        DisposableManager _disposableManager = null;

        [Inject]
        GuiRenderableManager _guiRenderableManager = null;

        DiContainer _container;

        protected DiContainer Container
        {
            get
            {
                return _container;
            }
        }

        public virtual void OnEnable()
        {
            var windowName = this.GetType().Name;
            var resourcePath = "EditorWindows/{0}".Fmt(windowName);
            var compRoot = Resources.Load<EditorWindowCompositionRoot>(resourcePath);

            Assert.IsNotNull(compRoot,
                "Could not find EditorWindowCompositionRoot for window '{0}'!  Expected to find it at '{1}'", windowName, resourcePath);

            _container = new DiContainer();

            InstallBindings();

            compRoot.Initialize(_container, this);

            _initializableManager.Initialize();
        }

        public virtual void InstallBindings()
        {
            // Optional
        }

        public virtual void OnDisable()
        {
            if (_disposableManager != null)
            {
                _disposableManager.Dispose();
                _disposableManager = null;
            }
        }

        public virtual void Update()
        {
            if (_tickableManager != null)
            {
                _tickableManager.Update();
            }

            // Doesn't seem worth trying to detect changes, just redraw every frame
            Repaint();
        }

        public virtual void OnGUI()
        {
            if (_guiRenderableManager != null)
            {
                _guiRenderableManager.OnGui();
            }
        }
    }
}

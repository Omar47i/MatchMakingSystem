﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace deVoid.UIFramework
{
    /// <summary>
    /// This is the centralized access point for all things UI.
    /// All your calls should be directed at this.
    /// </summary>
    public class UIFrame : MonoBehaviour
    {
        [SerializeField] UISettings settings;

        [Tooltip("Set this to false if you want to manually initialize this UI Frame.")]
        [SerializeField] bool initializeOnAwake = true;
        
        private PanelUILayer panelLayer;
        private WindowUILayer windowLayer;

        public ScreenId CurrentWindow { get {
                if (windowLayer!=null&& windowLayer.CurrentWindow != null)
                    return windowLayer.CurrentWindow.ScreenId;
                else return (ScreenId)(-1);
            } }
        public IEnumerable<WindowHistoryEntry> WindowHistory
        {
            get
            {
                if (windowLayer == null || windowLayer.WindowHistory == null)  yield break;
                foreach (var window in windowLayer.WindowHistory) yield return window;
            }
        }

        public IEnumerable<WindowHistoryEntry> WindowQueue
        {
            get
            {
                if (windowLayer == null || windowLayer.WindowQueue == null)  yield break;
                foreach (var window in windowLayer.WindowQueue) yield return window;
            }
        }

        private Canvas mainCanvas;
        private GraphicRaycaster graphicRaycaster;

        /// <summary>
        /// The main canvas of this UI
        /// </summary>
        public Canvas MainCanvas {
            get {
                if (mainCanvas == null) {
                    mainCanvas = GetComponent<Canvas>();
                }

                return mainCanvas;
            }
        }

        /// <summary>
        /// The Camera being used by the Main UI Canvas
        /// </summary>
        public Camera UICamera {
            get { return MainCanvas.worldCamera; }
        }

        private void Awake() {
            if (initializeOnAwake) {
                Initialize();
                LoadSettings();

            }
        }
        private void LoadSettings()
        {
            foreach (var screen in settings.screens)
            {
                var screenInstance = Instantiate(screen.Prefab);
                var screenController = screenInstance.GetComponent<IUIScreenController>();

                if (screenController != null)
                {
                    RegisterScreen((ScreenId)Enum.Parse(typeof(ScreenId), screen.ScreenId), screenController, screenInstance.transform);
                    if (settings.deactivateScreenGOs && screenInstance.activeSelf)
                    {
                        screenInstance.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError("[UIConfig] Screen doesn't contain a ScreenController! Skipping " + screen.ScreenId);
                }
            }
            if (settings.screens.Count > 0)
                OpenWindow((ScreenId)Enum.Parse(typeof(ScreenId), settings.screens[0].ScreenId) );
        }
        /// <summary>
        /// Initializes this UI Frame. Initialization consists of initializing both the Panel and Window layers.
        /// Although literally all the cases I've had to this day were covered by the "Window and Panel" approach,
        /// I made it virtual in case you ever need additional layers or other special initialization.
        /// </summary>
        public virtual void Initialize() {
            if (panelLayer == null) {
                panelLayer = gameObject.GetComponentInChildren<PanelUILayer>(true);
                if (panelLayer == null) {
                    Debug.LogError("[UI Frame] UI Frame lacks Panel Layer!");
                }
                else {
                    panelLayer.Initialize();
                }
            }

            if (windowLayer == null) {
                windowLayer = gameObject.GetComponentInChildren<WindowUILayer>(true);
                if (panelLayer == null) {
                    Debug.LogError("[UI Frame] UI Frame lacks Window Layer!");
                }
                else {
                    windowLayer.Initialize();
                    windowLayer.RequestScreenBlock += OnRequestScreenBlock;
                    windowLayer.RequestScreenUnblock += OnRequestScreenUnblock;
                }
            }

            graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// Shows a panel by its id, passing no Properties.
        /// </summary>
        /// <param name="screenId">Panel Id</param>
        public void ShowPanel(ScreenId screenId) {
            panelLayer.ShowScreenById(screenId);
        }

        /// <summary>
        /// Shows a panel by its id, passing parameters.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        /// <param name="properties">Properties.</param>
        /// <typeparam name="T">The type of properties to be passed in.</typeparam>
        /// <seealso cref="IPanelProperties"/>
        public void ShowPanel<T>(ScreenId screenId, T properties) where T : IPanelProperties {
            panelLayer.ShowScreenById<T>(screenId, properties);
        }

        /// <summary>
        /// Hides the panel with the given id.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void HidePanel(ScreenId screenId) {
            panelLayer.HideScreenById(screenId);
        }

        /// <summary>
        /// Opens the Window with the given Id, with no Properties.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void OpenWindow(ScreenId screenId) {
            windowLayer.ShowScreenById(screenId);
        }

        /// <summary>
        /// Closes the Window with the given Id.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void CloseWindow(ScreenId screenId) {
            windowLayer.HideScreenById(screenId);
        }
        
        /// <summary>
        /// Closes the currently open window, if any is open
        /// </summary>
        public void CloseCurrentWindow() {
            if (windowLayer.CurrentWindow != null) {
                CloseWindow(windowLayer.CurrentWindow.ScreenId);    
            }
        }

        /// <summary>
        /// Opens the Window with the given id, passing in Properties.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        /// <param name="properties">Properties.</param>
        /// <typeparam name="T">The type of properties to be passed in.</typeparam>
        /// <seealso cref="IWindowProperties"/>
        public void OpenWindow<T>(ScreenId screenId, T properties) where T : IWindowProperties {
            windowLayer.ShowScreenById<T>(screenId, properties);
        }

        /// <summary>
        /// Searches for the given id among the Layers, opens the Screen if it finds it
        /// </summary>
        /// <param name="screenId">The Screen id.</param>
        public void ShowScreen(ScreenId screenId) {
            Type type;
            if (IsScreenRegistered(screenId, out type)) {
                if (type == typeof(IWindowController)) {
                    OpenWindow(screenId);
                }
                else if (type == typeof(IPanelController)) {
                    ShowPanel(screenId);
                }
            }
            else {
                Debug.LogError(string.Format("Tried to open Screen id {0} but it's not registered as Window or Panel!",
                    screenId));
            }
        }

        /// <summary>
        /// Registers a screen. If transform is passed, the layer will
        /// reparent it to itself. Screens can only be shown after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <param name="screenTransform">Screen transform. If not null, will be reparented to proper layer</param>
        public void RegisterScreen(ScreenId screenId, IUIScreenController controller, Transform screenTransform) {
            controller.Frame = this;
            IWindowController window = controller as IWindowController;
            if (window != null) {
                windowLayer.RegisterScreen(screenId, window);
                if (screenTransform != null) {
                    windowLayer.ReparentScreen(controller, screenTransform);
                }

                return;
            }

            IPanelController panel = controller as IPanelController;
            if (panel != null) {
                panelLayer.RegisterScreen(screenId, panel);
                if (screenTransform != null) {
                    panelLayer.ReparentScreen(controller, screenTransform);
                }
            }
        }

        /// <summary>
        /// Registers the panel. Panels can only be shown after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TPanel">The Controller type.</typeparam>
        public void RegisterPanel<TPanel>(ScreenId screenId, TPanel controller) where TPanel : IPanelController {
            panelLayer.RegisterScreen(screenId, controller);
        }

        /// <summary>
        /// Unregisters the panel.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TPanel">The Controller type.</typeparam>
        public void UnregisterPanel<TPanel>(ScreenId screenId, TPanel controller) where TPanel : IPanelController {
            panelLayer.UnregisterScreen(screenId, controller);
        }

        /// <summary>
        /// Registers the Window. Windows can only be opened after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TWindow">The Controller type.</typeparam>
        public void RegisterWindow<TWindow>(ScreenId screenId, TWindow controller) where TWindow : IWindowController {
            windowLayer.RegisterScreen(screenId, controller);
        }

        /// <summary>
        /// Unregisters the Window.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TWindow">The Controller type.</typeparam>
        public void UnregisterWindow<TWindow>(ScreenId screenId, TWindow controller) where TWindow : IWindowController {
            windowLayer.UnregisterScreen(screenId, controller);
        }

        /// <summary>
        /// Checks if a given Panel is open.
        /// </summary>
        /// <param name="panelId">Panel identifier.</param>
        public bool IsPanelOpen(ScreenId panelId) {
            return panelLayer.IsPanelVisible(panelId);
        }

        /// <summary>
        /// Hide all screens
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void HideAll(bool animate = true) {
            CloseAllWindows(animate);
            HideAllPanels(animate);
        }

        /// <summary>
        /// Hide all screens on the Panel Layer
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void HideAllPanels(bool animate = true) {
            panelLayer.HideAll(animate);
        }

        /// <summary>
        /// Hide all screens in the Window Layer
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void CloseAllWindows(bool animate = true) {
            windowLayer.HideAll(animate);
        }

        /// <summary>
        /// Checks if a given screen id is registered to either the Window or Panel layers
        /// </summary>
        /// <param name="screenId">The Id to check.</param>
        public bool IsScreenRegistered(ScreenId screenId) {
            if (windowLayer.IsScreenRegistered(screenId)) {
                return true;
            }

            if (panelLayer.IsScreenRegistered(screenId)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a given screen id is registered to either the Window or Panel layers,
        /// also returning the screen type
        /// </summary>
        /// <param name="screenId">The Id to check.</param>
        /// <param name="type">The type of the screen.</param>
        public bool IsScreenRegistered(ScreenId screenId, out Type type) {
            if (windowLayer.IsScreenRegistered(screenId)) {
                type = typeof(IWindowController);
                return true;
            }

            if (panelLayer.IsScreenRegistered(screenId)) {
                type = typeof(IPanelController);
                return true;
            }

            type = null;
            return false;
        }

        private void OnRequestScreenBlock() {
            if (graphicRaycaster != null) {
                graphicRaycaster.enabled = false;
            }
        }

        private void OnRequestScreenUnblock() {
            if (graphicRaycaster != null) {
                graphicRaycaster.enabled = true;
            }
        }
    }
}

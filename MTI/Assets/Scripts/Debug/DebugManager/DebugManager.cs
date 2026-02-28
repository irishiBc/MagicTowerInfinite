using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
// using UnityEngine.InputSystem;
// using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
// using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Game.QA.Shared
{
    public class DebugManager : DebugSingleton<DebugManager>
    {
        #region Private Fields

        private float touchHoldTime;
        private const float REQUIRED_HOLD_DURATION = 0.77f;
        private DebugLogConsole debugLogConsole;

        private int tapCount;
        private float lastTapTime;


        private const float REQUIRED_TAP_LAG = 0.77f;

        private Button specBtn;
        private TextMeshProUGUI specText;
        private Image specImg;
        private float deltaTime = 0f;
        private int lastErrSum = 0;

        #endregion


        #region Interface

        private void Start()
        {
            SyncTrigger();
            SyncMgr();
            CreateConsoleGameObject();
            
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(specImg.sprite.texture); //代码创建内容需手动销毁
        }

        protected override void Update()
        {
            base.Update();
            //CheckTrigger();
            ConsoleReminder();
        }

        #endregion

        #region Private Methods

        private bool IsReminderActive()
        {
            return debugLogConsole != null &&
                   !debugLogConsole.isShow && debugLogConsole.errSum > 0;
        }

        private void ConsoleReminder()
        {
            if (debugLogConsole != null)
            {
                if (IsReminderActive())
                {
                    deltaTime += Time.unscaledDeltaTime;

                    specImg.color = new Color(0.2f, 0.2f, 0.2f, 1);
                    float s = (Mathf.Sin(deltaTime * 6f) + 1f) * 0.5f;
                    Color c = Color.Lerp(
                        new Color(0.2f, 0.2f, 0.2f, 0f),
                        Color.red,
                        s
                    );

                    specImg.color = c;
                    if (lastErrSum != debugLogConsole.errSum)
                    {
                        lastErrSum = debugLogConsole.errSum;
                        specText.text = debugLogConsole.errSum < 1000 ? debugLogConsole.errSum.ToString() : "999+";
                    }
                }
                else
                {
                    deltaTime = 0;
                    specText.text = string.Empty;
                    specImg.color = new Color(0.2f, 0.2f, 0.2f, 0);
                }
            }
        }


        private void SyncMgr()
        {
            touchHoldTime = 0f;
            tapCount = 0;
            lastTapTime = 0f;

            _ = DebugCoroutineManager.Instance;
            _ = DebugEventManager.Instance;
            // Todo TcpServer默认不启动，需要时通过GM命令"开/关DebugTcp"启动。测试OK再改成默认启动
            // _ = DebugTcpServer.Instance;    
        }

        private void SyncTrigger()
        {
            GameObject mgrTrigger = new GameObject("MainPanelTrigger", typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            mgrTrigger.transform.SetParent(gameObject.transform, false);

            var debugCanvas = mgrTrigger.GetComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvas.sortingOrder = 1000;

            // CanvasGroup    
            var canvasGroup = mgrTrigger.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            // CanvasScaler
            CanvasScaler scaler = mgrTrigger.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referenceResolution = new Vector2(1080, 2400);

            GameObject btn = new GameObject("Btn", typeof(RectTransform));
            btn.transform.SetParent(mgrTrigger.transform, false);
            specBtn = btn.AddComponent<Button>();

            GameObject specTextArea = new GameObject("SpecialTextArea", typeof(RectTransform));
            specTextArea.transform.SetParent(btn.transform, false);
            specText = specTextArea.AddComponent<TextMeshProUGUI>();
            // 设置按钮位置和大小
            RectTransform specButtonRT = btn.GetComponent<RectTransform>();
            specButtonRT.anchorMin = Vector2.zero;
            specButtonRT.anchorMax = Vector2.one;
            specButtonRT.offsetMin = Vector2.zero;
            specButtonRT.offsetMax = Vector2.zero;
            specButtonRT.anchorMin = new Vector2(0f, 0.95f);
            specButtonRT.anchorMax = new Vector2(0.1f, 1f);
            // 添加按钮背景
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f)
            );
            specImg = btn.AddComponent<Image>();
            specImg.sprite = sprite;

            specBtn.targetGraphic = specImg;
            specBtn.onClick.AddListener(TryTriggerMainPanel);

            specText.text = string.Empty;
            specText.alignment = TextAlignmentOptions.Center;
            specText.fontSize = 32;
        }

        private void TryTriggerMainPanel()
        {
            if (tapCount == 0)
            {
                tapCount++;
            }
            else if (Time.time - lastTapTime < REQUIRED_TAP_LAG)
            {
                tapCount++;
                if (tapCount >= 3)
                {
                    // if (GMDebugView.Instance != null)
                    //     GMDebugView.Instance.ShowWindow();
                    // else
                    //     Debug.LogError("GMDebugView does not initiate right now, please wait...");
                    // if (IsReminderActive())
                    //     ShowDebugConsole();
                    // tapCount = 0;
                }
            }
            else
                tapCount = 1;

            lastTapTime = Time.time;
        }

        private void CreateConsoleGameObject()
        {
            var debugConsole = new GameObject("DebugConsole", typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            debugConsole.transform.SetParent(gameObject.transform);
            debugLogConsole = debugConsole.AddComponent<DebugLogConsole>();
        }

//         private void CheckTrigger()
//         {
// #if UNITY_EDITOR || UNITY_STANDALONE
//             if (GMDebugView.Instance == null)
//                 return;
//
//             if (Keyboard.current.backquoteKey.wasPressedThisFrame)
//             {
//                 if (GMDebugView.Instance.IsShow)
//                     GMDebugView.Instance.HideWindow();
//                 else
//                 {
//                     GMDebugView.Instance.ShowWindow();
//                     if (IsReminderActive())
//                         ShowDebugConsole();
//                 }
//             }
//
// #endif
//             if (!GMDebugView.Instance.IsShow)
//             {
//                 if (Touch.activeTouches.Count > 2)
//                 {
//                     int validTouchCount = 0;
//                     foreach (var touch in Touch.activeTouches)
//                     {
//                         TouchPhase phase = touch.phase;
//                         if (phase == TouchPhase.Stationary || phase == TouchPhase.Moved)
//                         {
//                             validTouchCount++;
//                         }
//                     }
//
//                     if (validTouchCount >= 2)
//                     {
//                         touchHoldTime += Time.deltaTime;
//                         if (touchHoldTime >= REQUIRED_HOLD_DURATION)
//                         {
//                             GMDebugView.Instance.ShowWindow();
//                             if (IsReminderActive())
//                                 ShowDebugConsole();
//                             touchHoldTime = 0f;
//                         }
//                     }
//                     else
//                     {
//                         touchHoldTime = 0f;
//                     }
//                 }
//                 else
//                     touchHoldTime = 0f;
//             }
//         }

        #endregion

        #region Public Methods

        public void ShowDebugConsole()
        {
            debugLogConsole?.SetShowConsole(true);
        }

        public void SetConsoleShowOnErrOccurs(bool showOnErr)
        {
            if (debugLogConsole != null)
                debugLogConsole.ShowOnErrOccurs = showOnErr;
        }
        

        #endregion

        #region Temp Useless

        public void ShowMainPanel()
        {
            var window = root.GetComponent<GUI4QAMainWindow>();
            if (window != null)
            {
                if (!window.enabled)
                {
                    ClearAllPanelComponents();
                    window.enabled = true;
                }
            }
            else
            {
                root.AddComponent<GUI4QAMainWindow>();
            }
        }

        public void CloseMainPanel()
        {
            ClearAllPanelComponents();
        }

        public void ClearAllPanelComponents()
        {
            var components = root.GetComponents<Component>();
            foreach (var c in components)
            {
                if (!(c is Transform || c is DebugManager || c is GUI4QAMainWindow)) // mainwindow也不删除
                    DestroyImmediate(c);
                if (c is GUI4QAMainWindow)
                    c.GetComponent<GUI4QAMainWindow>().enabled = false;
            }
        }

        public void SetComponent(Type T)
        {
            ClearAllPanelComponents();
            root.AddComponent(T);
        }

        public void SetComponent<T>() where T : Component
        {
            ClearAllPanelComponents();
            root.AddComponent<T>();
        }

        #endregion
    }
}


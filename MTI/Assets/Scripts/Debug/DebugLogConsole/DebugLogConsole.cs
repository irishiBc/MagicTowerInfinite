using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.QA
{
    public class DebugLogConsole : MonoBehaviour
    {
        #region Private Members

        private Canvas debugCanvas;
        private CanvasGroup canvasGroup;
        private TMP_Text logText;
        private ScrollRect scrollRect;
        private RectTransform viewport;

        private RectTransform content;

        //private Scrollbar verticalScrollbar;
        private int maxLines = 100; //存一百条
        private string[] logLines = new string[0];

        private bool openLog;
        private bool openLogWarning;
        private bool openLogError;


        private const string DEBUG_CANVAS_NAME = "DebugConsole";

        private Button logButton;
        private Button logWarningButton;
        private Button logErrButton;
        private Button copyButton;
        private Button popButton;
        private TMP_Text popButtonText;


        private float scrollViewVerticalMax;
        private float scrollViewVerticalMin;
        private float btnViewVerticalMax;
        private float btnViewVerticalMin;
        private float btnBottomViewVerticalMax;
        private float btnBottomViewVerticalMin;
        

        #endregion

        #region Public Members

        public bool ShowOnErrOccurs = true;
        public int errSum = 0;
        public bool isShow;

        #endregion

        #region Interface

        private void Awake()
        {
            openLog = false;
            openLogWarning = false;
            openLogError = true;

            scrollViewVerticalMax = 0.8f;
            scrollViewVerticalMin = 0.3f;
            btnViewVerticalMax = 0.84f;
            btnViewVerticalMin = 0.82f;
            btnBottomViewVerticalMax = 0.28f;
            btnBottomViewVerticalMin = 0.26f;

            CreateCanvas();
            Application.logMessageReceived += HandleLog;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 主界面初始化
        /// TODO 类似一些插件将其拆分成按钮和细致条目
        /// 按钮点选单个log
        /// 细致条目显示单个log的trace
        /// </summary>
        private void CreateCanvas()
        {
            GameObject canvasObject = GameObject.Find(DEBUG_CANVAS_NAME);
            if (canvasObject != null)
            {
                #region Basic Component

                debugCanvas = canvasObject.GetComponent<Canvas>();
                debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                debugCanvas.sortingOrder = 1000; // 设置为最高层级

                // CanvasGroup    
                canvasGroup = canvasObject.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = true; // 允许射线检测（点击交互）
                canvasGroup.interactable = true; // 允许交互

                // CanvasScaler
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                scaler.referenceResolution = new Vector2(1080, 2400);

                #endregion

                #region ScrollView

                // 创建    ScrollView
                GameObject scrollViewObject = new GameObject("DebugScrollView", typeof(RectTransform));
                scrollViewObject.transform.SetParent(canvasObject.transform, false);
                scrollRect = scrollViewObject.AddComponent<ScrollRect>();

                // 设置    ScrollView 尺寸
                RectTransform scrollRT = scrollViewObject.GetComponent<RectTransform>();
                scrollRT.anchorMin = new Vector2(0f, scrollViewVerticalMin);
                scrollRT.anchorMax = new Vector2(1f, scrollViewVerticalMax);
                scrollRT.offsetMin = Vector2.zero;
                scrollRT.offsetMax = Vector2.zero;

                #endregion

                #region View & Content

                // 创建    Viewport
                GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform));
                viewportObject.transform.SetParent(scrollViewObject.transform, false);
                viewport = viewportObject.GetComponent<RectTransform>();
                viewport.anchorMin = Vector2.zero;
                viewport.anchorMax = Vector2.one;
                viewport.offsetMin = Vector2.zero;
                viewport.offsetMax = Vector2.zero;

                // 添加    Mask 和    Image 组件
                viewportObject.AddComponent<Mask>();
                Image viewportImage = viewportObject.AddComponent<Image>();
                viewportImage.color = new Color(0, 0, 0, 0.7f); // 半透明黑色背景

                // 创建    Content
                GameObject contentObject = new GameObject("Content", typeof(RectTransform));
                contentObject.transform.SetParent(viewportObject.transform, false);
                content = contentObject.GetComponent<RectTransform>();
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(1, 1);
                content.pivot = new Vector2(0.5f, 1); // 从上往下增长
                content.sizeDelta = new Vector2(0, 0);

                // 创建    Text 组件
                GameObject textObject = new GameObject("LogText", typeof(RectTransform));
                textObject.transform.SetParent(contentObject.transform, false);
                logText = textObject.AddComponent<TextMeshProUGUI>();

                // 设置    Text 属性
                RectTransform textRT = textObject.GetComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = Vector2.zero;
                textRT.offsetMax = Vector2.zero;

                logText.font = TMP_Settings.defaultFontAsset;
                logText.fontSize = 35;
                logText.color = Color.white;
                logText.alignment = TextAlignmentOptions.TopLeft;
                logText.overflowMode = TextOverflowModes.ScrollRect;

                #endregion

                #region Scroll Bar

                // 配置    ScrollRect
                scrollRect.content = content;
                scrollRect.viewport = viewport;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;

                // 添加滚动条
                GameObject verticalScrollbarObject = new GameObject("VerticalScrollbar", typeof(RectTransform));
                verticalScrollbarObject.transform.SetParent(scrollViewObject.transform, false);
                Scrollbar verticalScrollbar = verticalScrollbarObject.AddComponent<Scrollbar>();

                // 设置滚动条属性
                RectTransform scrollbarRT = verticalScrollbarObject.GetComponent<RectTransform>();
                scrollbarRT.anchorMin = new Vector2(1, 0);
                scrollbarRT.anchorMax = new Vector2(1, 1);
                scrollbarRT.offsetMin = new Vector2(-18, 0);
                scrollbarRT.offsetMax = Vector2.zero;


                verticalScrollbar.direction = Scrollbar.Direction.BottomToTop;
                verticalScrollbar.targetGraphic = verticalScrollbarObject.AddComponent<Image>();

                var barColors = verticalScrollbar.colors;
                barColors.normalColor = new Color(0f, 0.1f, 0.1f, 0.9f);
                barColors.highlightedColor = new Color(0f, 0.1f, 0.1f, 0.9f);
                barColors.selectedColor = new Color(0f, 0f, 0f, 0.9f);
                barColors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                verticalScrollbar.colors = barColors;

                verticalScrollbar.handleRect =
                    new GameObject("Handle", typeof(RectTransform)).GetComponent<RectTransform>();
                verticalScrollbar.handleRect.SetParent(verticalScrollbarObject.transform, false);
                verticalScrollbar.handleRect.anchorMin = new Vector2(0, 0);
                verticalScrollbar.handleRect.anchorMax = new Vector2(1, 1);

                verticalScrollbar.handleRect.offsetMin = new Vector2(2, 2);
                verticalScrollbar.handleRect.offsetMax = new Vector2(-2, -2);

                verticalScrollbar.handleRect.GetComponent<RectTransform>().gameObject.AddComponent<Image>();
                // 直接重新启用    scrollbar 组件，强制它重新执行    OnEnable，=-=    
                verticalScrollbar.enabled = false;
                verticalScrollbar.enabled = true;
                scrollRect.verticalScrollbar = verticalScrollbar;

                #endregion

                #region Log Btn

                GameObject btnRoot = new GameObject("BtnRoot", typeof(RectTransform));
                btnRoot.transform.SetParent(canvasObject.transform, false);
                RectTransform btnRootRT = btnRoot.GetComponent<RectTransform>();
                btnRootRT.anchorMin = Vector2.zero;
                btnRootRT.anchorMax = Vector2.one;
                btnRootRT.offsetMin = Vector2.zero;
                btnRootRT.offsetMax = Vector2.zero;

                GameObject logButtonObject = new GameObject("LogBtn", typeof(RectTransform));
                logButtonObject.transform.SetParent(btnRoot.transform, false);
                logButton = logButtonObject.AddComponent<Button>();

                // 设置按钮位置和大小
                RectTransform logbuttonRT = logButtonObject.GetComponent<RectTransform>();
                logbuttonRT.anchorMin = new Vector2(0.05f, btnViewVerticalMin);
                logbuttonRT.anchorMax = new Vector2(0.15f, btnViewVerticalMax);

                // 添加按钮文本
                GameObject logBtnTextObject = new GameObject("Text", typeof(RectTransform));
                logBtnTextObject.transform.SetParent(logButtonObject.transform, false);
                Text logBtnText = logBtnTextObject.AddComponent<Text>();

                // 设置按钮文本属性
                RectTransform logBtnTextRT = logBtnTextObject.GetComponent<RectTransform>();
                logBtnTextRT.anchorMin = Vector2.zero;
                logBtnTextRT.anchorMax = Vector2.one;
                logBtnTextRT.offsetMin = Vector2.zero;
                logBtnTextRT.offsetMax = Vector2.zero;

                logBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                logBtnText.fontSize = 40;
                logBtnText.color = Color.white;
                logBtnText.alignment = TextAnchor.MiddleCenter;
                logBtnText.text = "Log";
                logBtnText.fontStyle = FontStyle.Bold;
                // 添加按钮背景
                logButton.targetGraphic = logButtonObject.AddComponent<Image>();
                logButton.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                // 绑定清除按钮事件
                logButton.onClick.AddListener(LogShifter);

                #endregion

                #region LogWarning Btn

                // 添加清除按钮
                GameObject logWarningButtonObject = new GameObject("LogWarning", typeof(RectTransform));
                logWarningButtonObject.transform.SetParent(btnRoot.transform, false);
                logWarningButton = logWarningButtonObject.AddComponent<Button>();

                // 设置按钮位置和大小
                RectTransform logWarningnRT = logWarningButtonObject.GetComponent<RectTransform>();
                logWarningnRT.anchorMin = new Vector2(0.25f, btnViewVerticalMin);
                logWarningnRT.anchorMax = new Vector2(0.35f, btnViewVerticalMax);

                // 添加按钮文本
                GameObject logWarningBtnTextObject = new GameObject("Text", typeof(RectTransform));
                logWarningBtnTextObject.transform.SetParent(logWarningButtonObject.transform, false);
                Text logWarningBtnText = logWarningBtnTextObject.AddComponent<Text>();

                // 设置按钮文本属性
                RectTransform logWarningBtnTextRT = logWarningBtnTextObject.GetComponent<RectTransform>();
                logWarningBtnTextRT.anchorMin = Vector2.zero;
                logWarningBtnTextRT.anchorMax = Vector2.one;
                logWarningBtnTextRT.offsetMin = Vector2.zero;
                logWarningBtnTextRT.offsetMax = Vector2.zero;

                logWarningBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                logWarningBtnText.fontSize = 40;
                logWarningBtnText.color = Color.white;
                logWarningBtnText.alignment = TextAnchor.MiddleCenter;
                logWarningBtnText.text = "Warning";
                logWarningBtnText.fontStyle = FontStyle.Bold;

                // 添加按钮背景
                logWarningButton.targetGraphic = logWarningButtonObject.AddComponent<Image>();
                logWarningButton.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                // 绑定清除按钮事件
                logWarningButton.onClick.AddListener(LogWarningShifter);

                #endregion

                #region logError Btn

                // 添加清除按钮
                GameObject logErrButtonObject = new GameObject("LogError", typeof(RectTransform));
                logErrButtonObject.transform.SetParent(btnRoot.transform, false);
                logErrButton = logErrButtonObject.AddComponent<Button>();

                // 设置按钮位置和大小
                RectTransform logErrBtnRT = logErrButtonObject.GetComponent<RectTransform>();
                logErrBtnRT.anchorMin = new Vector2(0.45f, btnViewVerticalMin);
                logErrBtnRT.anchorMax = new Vector2(0.55f, btnViewVerticalMax);

                // 添加按钮文本
                GameObject logErrBtnTextObject = new GameObject("Text", typeof(RectTransform));
                logErrBtnTextObject.transform.SetParent(logErrButtonObject.transform, false);
                Text logErrBtnText = logErrBtnTextObject.AddComponent<Text>();

                // 设置按钮文本属性
                RectTransform logErrBtnTextRT = logErrBtnTextObject.GetComponent<RectTransform>();
                logErrBtnTextRT.anchorMin = Vector2.zero;
                logErrBtnTextRT.anchorMax = Vector2.one;
                logErrBtnTextRT.offsetMin = Vector2.zero;
                logErrBtnTextRT.offsetMax = Vector2.zero;

                logErrBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                logErrBtnText.fontSize = 40;
                logErrBtnText.color = Color.white;
                logErrBtnText.alignment = TextAnchor.MiddleCenter;
                logErrBtnText.text = "Error";
                logErrBtnText.fontStyle = FontStyle.Bold;

                // 添加按钮背景
                logErrButton.targetGraphic = logErrButtonObject.AddComponent<Image>();
                logErrButton.targetGraphic.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                // 绑定清除按钮事件
                logErrButton.onClick.AddListener(LogErrorShifter);

                #endregion

                #region ClearButton

                // 添加清除按钮
                GameObject clearButtonObject = new GameObject("ClearButton", typeof(RectTransform));
                clearButtonObject.transform.SetParent(btnRoot.transform, false);
                Button clearButton = clearButtonObject.AddComponent<Button>();
                // 设置按钮位置和大小
                RectTransform buttonRT = clearButtonObject.GetComponent<RectTransform>();
                buttonRT.anchorMin = new Vector2(0.65f, btnViewVerticalMin);
                buttonRT.anchorMax = new Vector2(0.75f, btnViewVerticalMax);
                // 添加按钮文本
                GameObject buttonTextObject = new GameObject("Text", typeof(RectTransform));
                buttonTextObject.transform.SetParent(clearButtonObject.transform, false);
                TMP_Text buttonText = buttonTextObject.AddComponent<TextMeshProUGUI>();
                // 设置按钮文本属性
                RectTransform buttonTextRT = buttonTextObject.GetComponent<RectTransform>();
                buttonTextRT.anchorMin = Vector2.zero;
                buttonTextRT.anchorMax = Vector2.one;
                buttonTextRT.offsetMin = Vector2.zero;
                buttonTextRT.offsetMax = Vector2.zero;
                buttonText.font = TMP_Settings.defaultFontAsset;
                buttonText.fontSize = 40;
                buttonText.color = Color.white;
                buttonText.alignment = TextAlignmentOptions.Midline;
                buttonText.text = "清除";
                buttonText.fontStyle = FontStyles.Bold;
                // 添加按钮背景
                clearButton.targetGraphic = clearButtonObject.AddComponent<Image>();
                clearButton.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                // 绑定清除按钮事件
                clearButton.onClick.AddListener(ClearLog);

                #endregion

                #region CloseButton

                GameObject closeButtonObject = new GameObject("CloseButton", typeof(RectTransform));
                closeButtonObject.transform.SetParent(btnRoot.transform, false);
                Button closeButton = closeButtonObject.AddComponent<Button>();
                // 设置按钮位置和大小
                RectTransform closeButtonRT = closeButtonObject.GetComponent<RectTransform>();
                closeButtonRT.anchorMin = new Vector2(0.85f, btnViewVerticalMin);
                closeButtonRT.anchorMax = new Vector2(0.95f, btnViewVerticalMax);
                // 添加按钮文本
                GameObject closeButtonTextObject = new GameObject("Text", typeof(RectTransform));
                closeButtonTextObject.transform.SetParent(closeButtonObject.transform, false);
                TMP_Text closeButtonText = closeButtonTextObject.AddComponent<TextMeshProUGUI>();
                // 设置按钮文本属性
                RectTransform closeButtonTextRT = closeButtonTextObject.GetComponent<RectTransform>();
                closeButtonTextRT.anchorMin = Vector2.zero;
                closeButtonTextRT.anchorMax = Vector2.one;
                closeButtonTextRT.offsetMin = Vector2.zero;
                closeButtonTextRT.offsetMax = Vector2.zero;
                closeButtonText.font = TMP_Settings.defaultFontAsset;
                closeButtonText.fontSize = 40;
                closeButtonText.color = Color.white;
                closeButtonText.alignment = TextAlignmentOptions.Midline;
                closeButtonText.text = "关闭";
                closeButtonText.fontStyle = FontStyles.Bold;
                // 添加按钮背景
                closeButton.targetGraphic = closeButtonObject.AddComponent<Image>();
                closeButton.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                // 绑定清除按钮事件
                closeButton.onClick.AddListener(Close);

                #endregion

                #region CopyLog Button

                GameObject copyButtonObject = new GameObject("CopyButton", typeof(RectTransform));
                copyButtonObject.transform.SetParent(btnRoot.transform, false);
                copyButton = copyButtonObject.AddComponent<Button>();
                // 设置按钮位置和大小
                RectTransform copyButtonRT = copyButtonObject.GetComponent<RectTransform>();
                copyButtonRT.anchorMin = new Vector2(0.65f, btnBottomViewVerticalMin);
                copyButtonRT.anchorMax = new Vector2(0.75f, btnBottomViewVerticalMax);
                // 添加按钮文本
                GameObject copyButtonTextObject = new GameObject("Text", typeof(RectTransform));
                copyButtonTextObject.transform.SetParent(copyButtonObject.transform, false);
                TMP_Text copyButtonText = copyButtonTextObject.AddComponent<TextMeshProUGUI>();
                // 设置按钮文本属性
                RectTransform copyButtonTextRT = copyButtonTextObject.GetComponent<RectTransform>();
                copyButtonTextRT.anchorMin = Vector2.zero;
                copyButtonTextRT.anchorMax = Vector2.one;
                copyButtonTextRT.offsetMin = Vector2.zero;
                copyButtonTextRT.offsetMax = Vector2.zero;
                copyButtonText.font = TMP_Settings.defaultFontAsset;
                copyButtonText.fontSize = 40;
                copyButtonText.color = Color.white;
                copyButtonText.alignment = TextAlignmentOptions.Midline;
                copyButtonText.text = "复制";
                copyButtonText.fontStyle = FontStyles.Bold;
                // 添加按钮背景
                copyButton.targetGraphic = copyButtonObject.AddComponent<Image>();
                copyButton.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                // 绑定复制按钮事件
                copyButton.onClick.AddListener(CopyLog);

                #endregion


                //当前仅引擎环境生效

                #region Never Pop Button

                GameObject popButtonObject = new GameObject("PopButton", typeof(RectTransform));
                popButtonObject.transform.SetParent(btnRoot.transform, false);
                popButton = popButtonObject.AddComponent<Button>();
                // 设置按钮位置和大小
                RectTransform popButtonRT = popButtonObject.GetComponent<RectTransform>();
                popButtonRT.anchorMin = new Vector2(0.85f, btnBottomViewVerticalMin);
                popButtonRT.anchorMax = new Vector2(0.95f, btnBottomViewVerticalMax);
                // 添加按钮文本
                GameObject popButtonTextObject = new GameObject("Text", typeof(RectTransform));
                popButtonTextObject.transform.SetParent(popButtonObject.transform, false);
                popButtonText = popButtonTextObject.AddComponent<TextMeshProUGUI>();
                // 设置按钮文本属性
                RectTransform popButtonTextRT = popButtonTextObject.GetComponent<RectTransform>();
                popButtonTextRT.anchorMin = Vector2.zero;
                popButtonTextRT.anchorMax = Vector2.one;
                popButtonTextRT.offsetMin = Vector2.zero;
                popButtonTextRT.offsetMax = Vector2.zero;
                popButtonText.font = TMP_Settings.defaultFontAsset;
                popButtonText.fontSize = 40;
                popButtonText.color = Color.white;
                popButtonText.alignment = TextAlignmentOptions.Midline;
                popButtonText.text = "不再弹出";
                popButtonText.fontStyle = FontStyles.Bold;
                // 添加按钮背景
                popButton.targetGraphic = popButtonObject.AddComponent<Image>();
                popButton.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                // 绑定清除按钮事件
                popButton.onClick.AddListener(SetPop);

                #endregion

                //不直接显示在界面中
                SetShowConsole(false);
            }
        }

        #region Log Show Logic

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            // 添加新日志到缓冲区
            string logEntry = $"[{type}] {logString}\n";

            // 如果是错误或异常，添加堆栈跟踪
            if (type == LogType.Error || type == LogType.Exception)
            {
                errSum++;
                logEntry += $"{stackTrace}\n";
            }

            // 根据日志类型设置颜色
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    logEntry = $"<color=red>{logEntry}</color>\n";
                    if (ShowOnErrOccurs)
                    {
                        openLogError = true;
                        SetShowConsole(true);
                    }

                    break;
                case LogType.Warning:
                    logEntry = $"<color=yellow>{logEntry}</color>\n";
                    break;
                default:
                    logEntry = $"<color=white>{logEntry}</color>\n";
                    break;
            }

            // 更新日志缓冲区
            if (logLines.Length >= maxLines)
            {
                // 移除最早的日志行（日志被覆盖，需要重置广播计数）
                Array.Copy(logLines, 1, logLines, 0, logLines.Length - 1);
                logLines[^1] = logEntry;
      
            }
            else
            {
                // 追加新日志行
                Array.Resize(ref logLines, logLines.Length + 1);
                logLines[^1] = logEntry;
            }

            // 更新    UI
            UpdateLogText();

            if (type == LogType.Error || type == LogType.Exception)
            {
                // 仅Error自动滚动到底部
                scrollRect.verticalNormalizedPosition = 0f;
                Canvas.ForceUpdateCanvases();
            }
        }

        private void UpdateLogText()
        {
            // 确保在主线程更新    UI
            if (logText != null)
            {
                string[] rlt = Array.Empty<string>();
                rlt = logLines;
                if (!openLog)
                    rlt = rlt.Where(s => !s.Contains("[Log]")).ToArray();

                if (!openLogWarning)
                    rlt = rlt.Where(s => !s.Contains("[Warning]")).ToArray();

                if (!openLogError)
                    rlt = rlt.Where(s => !s.Contains("[Exception]") && !s.Contains("[Error]")).ToArray();

                logText.text = string.Join("", rlt);

                // 调整    Content 高度
                float preferredHeight = logText.preferredHeight;
                content.sizeDelta = new Vector2(content.sizeDelta.x, preferredHeight);
            }
        }

        public void SetShowConsole(bool show)
        {
            isShow = show;
            if (show) errSum = 0;
            canvasGroup.alpha = show ? 0.9f : 0f;
            canvasGroup.blocksRaycasts = show;
        }

        #endregion

 

        #region Btn CallBack

        private void CopyLog()
        {
            //string logStr = Shared.GMUtils.GetDebugUserInfo();
            const string pattern = @"<color=.*?>|</color>";
            var cleaned = Regex.Replace(logText.text, pattern, "");
            //     按行处理，去掉空行
            string[] lines = cleaned.Split('\n');
            var nonEmptyLines = lines
                .Select(l => l.Trim()) // 去除首尾空白
                .Where(l => l.Length > 0); // 非空行保留
            // logStr += string.Join("\n", nonEmptyLines);
            // GUIUtility.systemCopyBuffer = logStr;
        }

        private void SetPop()
        {
            ShowOnErrOccurs = !ShowOnErrOccurs;
            if (ShowOnErrOccurs)
            {
                popButton.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                popButtonText.color = Color.white;
            }
            else
            {
                popButton.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                popButtonText.color = Color.red;
            }
        }

        private void Close()
        {
            SetShowConsole(false);
        }

        private void ClearLog()
        {
            logLines = new string[0];
            logText.text = "";
            content.sizeDelta = new Vector2(content.sizeDelta.x, 0);
        }

        private void LogShifter()
        {
            openLog = !openLog;
            logButton.GetComponent<Image>().color =
                openLog ? new Color(1f, 1f, 1f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);

            UpdateLogText();
        }

        private void LogWarningShifter()
        {
            openLogWarning = !openLogWarning;
            if (openLogWarning)
                logWarningButton.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.2f, 0.8f);
            else
                logWarningButton.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            UpdateLogText();
        }

        private void LogErrorShifter()
        {
            openLogError = !openLogError;
            if (openLogError)
                logErrButton.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            else
                logErrButton.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            UpdateLogText();
        }

        #endregion

        #endregion
    }
}
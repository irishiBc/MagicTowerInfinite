using System;
using System.Collections.Generic;
using UnityEngine;
using Game.QA.Shared;

namespace Game.QA
{
    public class GUI4QATester : MonoBehaviour
    {
        #region private member

        /// <summary>
        /// scrollview初始位置
        /// </summary>
        private Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// 记录当前的屏幕分辨率
        /// </summary>
        private float curScreenWidth, curScreenHeight;

        private Texture2D toggleBackGround;

        #endregion

        #region public member

        protected float fontSize = 50f;

        /// <summary>
        /// 窗口宽度
        /// </summary>
        protected float WINDOW_WIDTH;

        /// <summary>
        /// 窗口高度
        /// </summary>
        protected float WINDOW_HEIGHT;

        /// <summary>
        /// 窗口名
        /// </summary>
        protected string windowName = "Welcome To QA Debug Window";


        protected float lineMaxRectX;

        protected float lineMaxRectY;

        protected float windowMaxRectX;

        /// <summary>
        /// 计算文本显示前，按钮或其他GUI占用的Y轴空间，用于定位文本位置，并将部分功能ui区域纳入Scrollview
        /// </summary>
        protected float windowMaxRectY;


        /// <summary>
        /// 按钮起始的相对高度
        /// </summary>
        protected float TITLE_HEIGHT;

        /// <summary>
        /// 单个小按钮的宽度
        /// </summary>
        protected float BUTTON_WIDTH;

        /// <summary>
        /// 单个小按钮的宽度
        /// </summary>
        protected float BUTTON_HEIGHT;

        /// <summary>
        /// 单个微型按钮的宽度
        /// </summary>
        protected float TINY_BUTTON_WIDTH;


        protected float PURE_SPACE_WIDTH;

        protected float PURE_SPACE_HEIGHT;


        /// <summary>
        /// 窗口的文本显示内容，一般集中在此处管理
        /// </summary>
        protected GUIContent content = new GUIContent();


        /// <summary>
        /// 窗口文字的显示风格
        /// </summary>
        protected GUIStyle fontStyle = GUIStyle.none;


        /// <summary>
        /// 标题按钮字典
        /// </summary>
        protected Dictionary<string, Action> buttonName2Actions;

        #endregion

        #region interface

        protected virtual void Awake()
        {
            curScreenWidth = Screen.width;
            curScreenHeight = Screen.height;

            lineMaxRectX = 0f;
            lineMaxRectY = 0f;

            fontStyle.fontSize = (int)CalculateDesiredFontSize(fontSize);
            fontStyle.alignment = TextAnchor.UpperLeft;
            fontStyle.normal.textColor = Color.cyan;
            fontStyle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");


            toggleBackGround = MakeTexture(60, 60, new Color(0.8f, 0.1f, 0.1f, 0.6f));
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Start()
        {
            OnStart();
        }

        /// <summary>
        /// update
        /// </summary>
        private void Update()
        {
            OnUpdate();
        }

        /// <summary>
        /// 子类复写用初始化
        /// </summary>
        protected virtual void OnEnable()
        {
        }

        /// <summary>
        /// update
        /// </summary>
        protected virtual void OnDestroy()
        {
            OnScriptDestroy();
        }

        /// <summary>
        /// gui
        /// </summary>
        private void OnGUI()
        {
            FinalWindowGUI();
        }

        /// <summary>
        /// 子类复写用初始化
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// 复写用update
        /// </summary>
        protected virtual void OnUpdate()
        {
        }


        /// <summary>
        /// 复写用update
        /// </summary>
        public virtual void OnScriptDestroy()
        {
        }

        /// <summary>
        /// 复写用gui
        /// kof 在默认界面上先搁一些当前常用的按钮和设置
        /// </summary>
        protected virtual void CustomGUIContent()
        {
        }


        /// <summary>
        /// 唯一的窗口
        /// </summary>
        private void FinalWindowGUI()
        {
            OnScreenSizeChanged();
            GUI.Window(0, new Rect(0, 0.2f * curScreenHeight, WINDOW_WIDTH, WINDOW_HEIGHT)
                , FinalGUIContent, windowName);
        }

        /// <summary>
        /// GUIWindow 展示内容
        /// </summary>
        /// <param name="i"></param>
        private void FinalGUIContent(int i)
        {
            FinalGUI();
        }

        /// <summary>
        /// 默认按钮和content显示，content为空则不显示任何内容
        /// </summary>
        private void FinalGUI()
        {
            GUIStyle scrollStyle = new GUIStyle(GUI.skin.scrollView);
            scrollStyle.fixedHeight = GUI.skin.verticalScrollbar.fixedWidth; //+ WINDOW_HEIGHT * 0.02f;
            scrollStyle.fixedWidth = GUI.skin.verticalScrollbar.fixedWidth; // + WINDOW_WIDTH * 0.02f;

            // 配置滚动视图的边框
            scrollStyle.margin = new RectOffset(10, 10, 10, 10);
            scrollPosition =
                GUI.BeginScrollView(new Rect(WINDOW_WIDTH * 0.02f, 0,
                        WINDOW_WIDTH * 0.98f, WINDOW_HEIGHT * 0.98f), scrollPosition,
                    new Rect(0, 0, windowMaxRectX, windowMaxRectY),
                    false, false);

            windowMaxRectX = 0f;
            windowMaxRectY = 0f;

            #region GUI REGION

            CreateDefaultButton();
            CreateTitleButtonsInWindow();
            ShowDefaultLabel();
            CustomGUIContent();

            #endregion

            GUI.EndScrollView();
        }

        #endregion

        #region private methods

        /// <summary>
        /// 基础按钮，目前每个窗口必然有的按钮在这儿
        /// </summary>
        private void CreateDefaultButton()
        {
            CreateButtonByXY(WINDOW_WIDTH * 0.98f + scrollPosition.x - BUTTON_WIDTH - PURE_SPACE_WIDTH, TITLE_HEIGHT,
                "关闭",
                DebugManager.Instance.ClearAllPanelComponents);

            CreateButtonByXYAndNewLine(WINDOW_WIDTH * 0.98f + scrollPosition.x - (BUTTON_WIDTH + PURE_SPACE_WIDTH) * 2,
                TITLE_HEIGHT, "返回主界面",
                DebugManager.Instance.ShowMainPanel);
            AddWindowRectY(TITLE_HEIGHT); //这儿需要嗯算一次布局
        }

        /// <summary>
        /// 按照按钮数量按    buttonName2Actions 初始化顺序生成按钮
        /// 定义为标题处的按钮，目前纳入整体的Scollview，目测做两块Scrollview区域意义不大
        /// </summary>
        private void CreateTitleButtonsInWindow()
        {
            if (buttonName2Actions != null && buttonName2Actions.Count > 0)
            {
                List<string> tmpList = new List<string>(buttonName2Actions.Keys);
                for (int i = 0; i < buttonName2Actions.Count; i++)
                {
                    int column = (i + 1) % 3;
                    int row = Mathf.CeilToInt((i + 1) / 3);
                    if (column != 0)
                        CreateButton(tmpList[i], buttonName2Actions[tmpList[i]]);

                    else
                        CreateButtonAndNewLine(tmpList[i], buttonName2Actions[tmpList[i]]);

                    if (i == buttonName2Actions.Count - 1)
                    {
                        if ((i + 1) % 3 != 0)
                            AddWindowRectY(BUTTON_HEIGHT + PURE_SPACE_HEIGHT);
                    }
                }
            }

            AddWindowRectY(PURE_SPACE_HEIGHT); //做一个明显的间隔
            ResetLineRectMaxX();
        }

        /// <summary>
        /// 展示默认content
        /// </summary>
        private void ShowDefaultLabel()
        {
            var size = fontStyle.CalcSize(content);
            GUI.Label(new Rect(0, windowMaxRectY, size.x,
                size.y), content, fontStyle);
        }

        #region Screen Size Concerned

        /// <summary>
        /// 屏幕size发生改变时的gui数据更新
        /// </summary>
        private void OnScreenSizeChanged()
        {
            if (Screen.width != curScreenWidth || Screen.height != curScreenHeight)
            {
                curScreenWidth = Screen.width;
                curScreenHeight = Screen.height;
            }

            RefreshWindowSize();
        }

        /// <summary>
        /// 更新窗口大小和对应的基础gui数据
        /// </summary>
        private void RefreshWindowSize()
        {
            WINDOW_WIDTH = curScreenWidth;
            WINDOW_HEIGHT = curScreenHeight * 0.75f;
            TITLE_HEIGHT = curScreenHeight * 0.03f;
            BUTTON_WIDTH = curScreenWidth * 0.3f;
            BUTTON_HEIGHT = curScreenHeight * 0.04f;
            TINY_BUTTON_WIDTH = curScreenWidth * 0.06f;
            PURE_SPACE_WIDTH = curScreenWidth * 0.01f;
            PURE_SPACE_HEIGHT = curScreenHeight * 0.01f;
        }

        #endregion

        #endregion

        #region public methods

        #region Calc GUI Size

        /// <summary>
        /// 根据一个基准的fontSize值计算出在当前分辨率下的字体fontsize
        /// </summary>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        protected float CalculateDesiredFontSize(float fontSize)
        {
            // 在这里编写计算字体大小的逻辑，可以根据不同的条件进行计算
            // 例如，您可以使用屏幕分辨率和基准字体大小来调整字体大小
            float screenSizeFactor = curScreenWidth / 1080; // 假设基准分辨率为1920x1080
            float desiredSize = fontSize * screenSizeFactor;

            return desiredSize;
        }


        /// <summary>
        /// windowMaxRectY的加减
        /// </summary>
        /// <returns></returns>
        protected void AddWindowRectY(float yDelta)
        {
            if (yDelta > lineMaxRectY) windowMaxRectY += yDelta;
            else windowMaxRectY += lineMaxRectY + PURE_SPACE_HEIGHT;
            lineMaxRectY = 0f;
        }


        protected void AddLineRectX(float xDelta)
        {
            lineMaxRectX += xDelta;
            if (lineMaxRectX >= windowMaxRectX)
                windowMaxRectX = lineMaxRectX;
        }

        protected void SetLineRectMaxX(float x)
        {
            if (x >= lineMaxRectX)
                lineMaxRectX = x;
            if (lineMaxRectX >= windowMaxRectX)
                windowMaxRectX = lineMaxRectX;
        }


        protected void SetLineRectMaxY(float y)
        {
            if (y >= lineMaxRectY)
                lineMaxRectY = y; // y轴不换行，只记录数据
        }


        /// <summary>
        /// windowMaxRectX设置
        /// </summary>
        /// <returns></returns>
        protected void ResetLineRectMaxX()
        {
            lineMaxRectX = 0f;
        }

        #endregion

        #region Create Button

        /// <summary>
        /// 小按钮的相对位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Rect CreateButtonRectByXY(float x, float y)
        {
            AddLineRectX(BUTTON_WIDTH + PURE_SPACE_WIDTH);
            SetLineRectMaxY(BUTTON_HEIGHT);
            return new Rect(x, y, BUTTON_WIDTH, BUTTON_HEIGHT);
        }


        /// <summary>
        /// 小按钮的相对位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Rect CreateTinyButtonRectByXY(float x, float y)
        {
            AddLineRectX(TINY_BUTTON_WIDTH + PURE_SPACE_WIDTH);
            SetLineRectMaxY(BUTTON_HEIGHT);
            return new Rect(x, y, TINY_BUTTON_WIDTH, BUTTON_HEIGHT);
        }

        /// <summary>
        /// 创建一个微型按钮，不改动布局
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="btnName"></param>
        /// <param name="cb"></param>
        protected void CreateTinyButtonByXY(float x, float y,
            string btnName, Action cb = null, GUIStyle style = null)
        {
            if (style != null)
            {
                float standard = 25f;
                style.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateTinyButtonRectByXY(x, y), btnName, style))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
            else
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                float standard = 25f;
                buttonStyle.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateTinyButtonRectByXY(x, y), btnName, buttonStyle))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个微型按钮，且改动布局
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="btnName"></param>
        /// <param name="cb"></param>
        protected void CreateTinyButtonByXYAndNewLine(float x, float y,
            string btnName, Action cb = null, GUIStyle style = null)
        {
            if (style != null)
            {
                float standard = 25f;
                style.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateTinyButtonRectByXY(x, y), btnName, style))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
            else
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                float standard = 25f;
                buttonStyle.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateTinyButtonRectByXY(x, y), btnName, buttonStyle))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }

            ResetLineRectMaxX();
            AddWindowRectY(BUTTON_HEIGHT + PURE_SPACE_HEIGHT);
        }

        /// <summary>
        /// 创建一个小按钮，不改动布局
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="btnName"></param>
        /// <param name="cb"></param>
        protected void CreateButtonByXY(float x, float y, string btnName, Action cb = null, GUIStyle style = null)
        {
            if (style != null)
            {
                float standard = 40f;
                style.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(x, y), btnName, style))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
            else
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                float standard = 40f;
                buttonStyle.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(x, y), btnName, buttonStyle))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 自动布局按钮
        /// </summary>
        /// <param name="btnName"></param>
        /// <param name="cb"></param>
        /// <param name="style"></param>
        protected void CreateButton(string btnName, Action cb = null, GUIStyle style = null)
        {
            if (style != null)
            {
                float standard = 40f;
                style.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(lineMaxRectX, windowMaxRectY), btnName, style))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
            else
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                float standard = 40f;
                buttonStyle.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(lineMaxRectX, windowMaxRectY), btnName, buttonStyle))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
        }


        /// <summary>
        /// 创建一个按钮，并且自动将布局推进到按钮下方
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="btnName"></param>
        /// <param name="cb"></param>
        protected void CreateButtonByXYAndNewLine(float x, float y, string btnName, Action cb = null,
            GUIStyle style = null)
        {
            if (style != null)
            {
                float standard = 40f;
                style.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(x, y), btnName, style))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
            else
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                float standard = 40f;
                buttonStyle.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(x, y), btnName, buttonStyle))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }

            ResetLineRectMaxX();
            AddWindowRectY(BUTTON_HEIGHT + PURE_SPACE_HEIGHT);
        }


        /// <summary>
        /// 自动布局一个按钮，并且自动将布局推进到按钮下方
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="btnName"></param>
        /// <param name="cb"></param>
        protected void CreateButtonAndNewLine(string btnName, Action cb = null, GUIStyle style = null)
        {
            if (style != null)
            {
                float standard = 40f;
                style.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(lineMaxRectX, windowMaxRectY), btnName, style))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }
            else
            {
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                float standard = 40f;
                buttonStyle.fontSize = (int)CalculateDesiredFontSize(standard);
                if (GUI.Button(CreateButtonRectByXY(lineMaxRectX, windowMaxRectY), btnName, buttonStyle))
                {
                    if (cb != null)
                    {
                        cb.Invoke();
                    }
                }
            }

            ResetLineRectMaxX();
            AddWindowRectY(BUTTON_HEIGHT + PURE_SPACE_HEIGHT);
        }

        #endregion

        #region Create Label

        /// <summary>
        /// 创造一个普通label，整个风格默认沿用content的形式
        /// </summary>
        /// <param name="label"></param>
        protected void CreateLabelWithoutNewLine(string label)
        {
            Vector2 size = fontStyle.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(lineMaxRectX, windowMaxRectY, size.x, size.y), label, fontStyle);

            SetLineRectMaxY(size.y);
            AddLineRectX(size.x + PURE_SPACE_WIDTH);
        }

        /// <summary>
        /// 创造一个普通label，整个风格默认沿用content的形式
        /// </summary>
        /// <param name="label"></param>
        protected void CreateLabelWithoutNewLine(string label, GUIStyle style)
        {
            Vector2 size = style.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(lineMaxRectX, windowMaxRectY, size.x, size.y), label, style);

            SetLineRectMaxY(size.y);
            AddLineRectX(size.x + PURE_SPACE_WIDTH);
        }

        /// <summary>
        /// 创造一个普通label，整个风格默认沿用content的形式
        /// </summary>
        /// <param name="label"></param>
        protected void CreateLabel(string label)
        {
            Vector2 size = fontStyle.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(lineMaxRectX, windowMaxRectY, size.x, size.y), label, fontStyle);

            AddLineRectX(size.x + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(size.y + PURE_SPACE_HEIGHT);
        }


        /// <summary>
        /// 创造一个普通label，整个风格默认沿用content的形式
        /// </summary>
        /// <param name="label"></param>
        protected void CreateLabel(string label, GUIStyle style)
        {
            Vector2 size = style.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(lineMaxRectX, windowMaxRectY, size.x, size.y), label, style);

            AddLineRectX(size.x + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(size.y + PURE_SPACE_HEIGHT);
        }

        /// <summary>
        /// 创造一个标题label
        /// </summary>
        /// <param name="label"></param>
        protected void CreateTitleLabel(string label)
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            float titleFontSize = 70;
            titleStyle.fontSize = (int)CalculateDesiredFontSize(titleFontSize);
            titleStyle.alignment = TextAnchor.UpperLeft;
            titleStyle.normal.textColor = new Color(1.0f, 0.843f, 0.0f);
            titleStyle.fontStyle = FontStyle.Bold;

            Vector2 size = titleStyle.CalcSize(new GUIContent(label));
            GUI.Label(new Rect(0, windowMaxRectY, size.x, size.y), label, titleStyle);

            AddLineRectX(size.x + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(size.y + PURE_SPACE_HEIGHT);
        }

        #endregion

        #region Create Field

        /// <summary>
        /// 创建一个输入文本的区域，并且自动将布局置入输入区域下方
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected string CreateTextField(string inputText, float x, float width)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);
            inputText = GUI.TextField(new Rect(x, windowMaxRectY, width,
                textFieldStyle.fontSize * 1.5f), inputText, textFieldStyle);

            SetLineRectMaxY(textFieldStyle.fontSize * 1.5f);
            AddLineRectX(width + PURE_SPACE_WIDTH);

            return inputText;
        }


        /// <summary>
        /// 自动创建一个输入文本的区域
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected string CreateTextField(string inputText, float width)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);
            inputText = GUI.TextField(new Rect(lineMaxRectX, windowMaxRectY, width,
                textFieldStyle.fontSize * 1.5f), inputText, textFieldStyle);

            SetLineRectMaxY(textFieldStyle.fontSize * 1.5f);
            AddLineRectX(width + PURE_SPACE_WIDTH);

            return inputText;
        }

        /// <summary>
        /// 创建一个输入文本的区域，并且自动将布局置入输入区域下方
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected string CreateTextFieldAndNewLine(string inputText, float x, float width)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);
            inputText = GUI.TextField(new Rect(x, windowMaxRectY, width,
                textFieldStyle.fontSize * 1.5f), inputText, textFieldStyle);

            AddLineRectX(width + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(textFieldStyle.fontSize * 1.5f + PURE_SPACE_HEIGHT);
            return inputText;
        }

        /// <summary>
        /// 自动创建一个输入文本的区域
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected string CreateTextFieldAndNewLine(string inputText, float width)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);
            inputText = GUI.TextField(new Rect(lineMaxRectX, windowMaxRectY, width,
                textFieldStyle.fontSize * 1.5f), inputText, textFieldStyle);

            AddLineRectX(width + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(textFieldStyle.fontSize * 1.5f + PURE_SPACE_HEIGHT);
            return inputText;
        }


        /// <summary>
        /// 创建一个输入int的区域，并且自动将布局置入输入区域下方
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected int CreateIntField(string inputText, float x, float width,
            int min = int.MinValue, int max = int.MaxValue)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);
            inputText = GUI.TextField(new Rect(x, windowMaxRectY, width,
                textFieldStyle.fontSize * 1.5f), inputText, textFieldStyle);


            SetLineRectMaxY(textFieldStyle.fontSize * 1.5f);
            AddLineRectX(width + PURE_SPACE_WIDTH);

            if (int.TryParse(inputText, out int rlt))
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
            else
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
        }


        /// <summary>
        /// 创建一个输入int的区域，并且自动将布局置入输入区域下方
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected int CreateIntField(string inputText, float width,
            int min = int.MinValue, int max = int.MaxValue)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);
            inputText = GUI.TextField(new Rect(lineMaxRectX, windowMaxRectY, width,
                textFieldStyle.fontSize * 1.5f), inputText, textFieldStyle);

            SetLineRectMaxY(textFieldStyle.fontSize * 1.5f);
            AddLineRectX(width + PURE_SPACE_WIDTH);

            if (int.TryParse(inputText, out int rlt))
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
            else
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
        }

        /// <summary>
        /// 创建一个输入文本的区域，并且自动将布局置入输入区域下方
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected int CreateIntFieldAndNewLine(string inputText, float x, float width,
            int min = int.MinValue, int max = int.MaxValue)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);

            inputText = GUI.TextField(new Rect(x, windowMaxRectY, width,
                textFieldStyle.fontSize), inputText, textFieldStyle);

            AddLineRectX(width + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(textFieldStyle.fontSize + PURE_SPACE_HEIGHT);
            if (int.TryParse(inputText, out int rlt))
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
            else
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
        }


        /// <summary>
        /// 创建一个输入文本的区域，并且自动将布局置入输入区域下方
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        protected int CreateIntFieldAndNewLine(string inputText, float width,
            int min = int.MinValue, int max = int.MaxValue)
        {
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            float standard = 40f;
            textFieldStyle.fontSize = (int)CalculateDesiredFontSize(standard);

            inputText = GUI.TextField(new Rect(lineMaxRectX, windowMaxRectY, width,
                textFieldStyle.fontSize), inputText, textFieldStyle);

            AddLineRectX(width + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(textFieldStyle.fontSize + PURE_SPACE_HEIGHT);
            if (int.TryParse(inputText, out int rlt))
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
            else
            {
                if (rlt >= max)
                    rlt = max;
                else if (rlt <= min)
                    rlt = min;
                return rlt;
            }
        }

        #endregion

        #region CreateDropDown

        protected object CreateDropDown(GMDropDown dropDown)
        {
            Rect defaultRect = new Rect(lineMaxRectX, windowMaxRectY,
                curScreenWidth * 0.2f, curScreenHeight * 0.2f);
            if (dropDown != null)
            {
                dropDown.DrawDefaultDropDown(defaultRect);
                SetLineRectMaxY(dropDown.MaxRectY + PURE_SPACE_HEIGHT);
                AddLineRectX(dropDown.MaxRectX + PURE_SPACE_WIDTH);

                return dropDown.curObj;
            }
            else
            {
                Debug.LogWarning("当前下拉框生成异常");
                return null;
            }
        }

        protected object CreateDropDown(Rect dropRect, GMDropDown dropDown)
        {
            if (dropDown != null)
            {
                dropDown.DrawDefaultDropDown(dropRect);
                SetLineRectMaxY(dropDown.MaxRectY + PURE_SPACE_HEIGHT);
                AddLineRectX(dropDown.MaxRectX + PURE_SPACE_WIDTH);

                return dropDown.curObj;
            }
            else
            {
                Debug.LogWarning("当前下拉框生成异常");
                return null;
            }
        }

        protected object CreateDropDownAndNewLine(Rect dropRect, GMDropDown dropDown)
        {
            if (dropDown != null)
            {
                dropDown.DrawDefaultDropDown(dropRect);
                ResetLineRectMaxX();
                AddWindowRectY(dropDown.MaxRectY + PURE_SPACE_HEIGHT);

                return dropDown.curObj;
            }
            else
            {
                Debug.LogWarning("当前下拉框生成异常");
                return null;
            }
        }

        protected object CreateDropDownAndNewLine(GMDropDown dropDown)
        {
            Rect defaultRect = new Rect(lineMaxRectX, windowMaxRectY,
                curScreenWidth * 0.2f, curScreenHeight * 0.24f);
            if (dropDown != null)
            {
                dropDown.DrawDefaultDropDown(defaultRect);
                ResetLineRectMaxX();
                AddWindowRectY(dropDown.MaxRectY + PURE_SPACE_HEIGHT);
                return dropDown.curObj;
            }
            else
            {
                //Debug.LogWarning("当前下拉框生成异常");
                return null;
            }
        }

        #endregion

        #region DropDown

        public class GMDropDown
        {
            private Vector2 scrollPosition = Vector2.zero;

            private string[] options;
            private object[] _objs;
            private bool isDropdownVisible;
            private float maxRectX;
            private float maxRectY;

            /// <summary>
            /// 用来返回当前选择的值
            /// </summary>
            public object curObj;

            public int selectedOptionIndex;

            public Action<object> OnSelect;

            public string[] Options
            {
                get { return options; }
            }

            public object[] Objs
            {
                get { return _objs; }
            }

            public bool IsDropdownVisible
            {
                get { return isDropdownVisible; }
            }

            public float MaxRectX
            {
                get { return maxRectX; }
            }

            public float MaxRectY
            {
                get { return maxRectY; }
            }

            public GMDropDown()
            {
            }

            public GMDropDown(string[] opts, object[] objs)
            {
                options = opts;
                _objs = objs;
                if (_objs != null && _objs.Length > 0)
                    curObj = objs[0];
                else
                    curObj = null;
            }


            /// <summary>
            /// 根据一个基准的fontSize值计算出在当前分辨率下的字体fontsize
            /// </summary>
            /// <param name="fontSize"></param>
            /// <returns></returns>
            private float CalculateDesiredFontSize(float fontSize)
            {
                // 在这里编写计算字体大小的逻辑，可以根据不同的条件进行计算
                float screenSizeFactor = Screen.width / 1080f; // 假设基准分辨率为1920x1080
                float desiredSize = fontSize * screenSizeFactor;

                return desiredSize;
            }


            public void SetCurIndexWithoutAction(int index)
            {
                selectedOptionIndex = index;
                curObj = _objs[index];
                isDropdownVisible = false;
            }

            public void SetCurIndex(int index)
            {
                selectedOptionIndex = index;
                curObj = _objs[index];
                isDropdownVisible = false;
                OnSelect?.Invoke(curObj);
            }

            /// <summary>
            /// 生成一个标准的GMDropDown
            /// </summary>
            /// <param name="dropdownRect"></param>
            public void DrawDefaultDropDown(Rect dropdownRect)
            {
                maxRectX = dropdownRect.width;
                Rect buttonRect = new Rect(dropdownRect.x, dropdownRect.y, dropdownRect.width,
                    dropdownRect.height / 6);
                GUIStyle selectOptionStyle = new GUIStyle(GUI.skin.button);
                selectOptionStyle.normal.textColor = Color.green;
                selectOptionStyle.fontSize = (int)CalculateDesiredFontSize(40f);
                selectOptionStyle.fontStyle = FontStyle.Bold;

                if (options != null && options.Length > 0)
                {
                    // 创建一个按钮模拟下拉选择框
                    if (GUI.Button(buttonRect, options[selectedOptionIndex], selectOptionStyle))
                    {
                        isDropdownVisible = !isDropdownVisible;
                    }
                }
                else
                {
                    Debug.LogWarning("当前下拉选项为空");
                }


                // 如果下拉选择框可见，则创建一个滚动区域显示选项列表
                if (isDropdownVisible)
                {
                    dropdownRect.y += buttonRect.height; //当前如此处理一下，避免和选择栏重合
                    GUI.Box(dropdownRect, ""); // 创建一个框以包围选项列表

                    GUIStyle scrollStyle = new GUIStyle(GUI.skin.scrollView);
                    scrollStyle.fixedHeight = Screen.height * 0.02f;
                    scrollStyle.fixedWidth = Screen.width * 0.02f;
                    // 使用ScrollView创建一个滚动区域，以容纳所有选项
                    scrollPosition = GUI.BeginScrollView(dropdownRect, scrollPosition, new Rect(0, 0,
                        dropdownRect.width, (options.Length + 1) * buttonRect.height));


                    GUIStyle optionStyle = new GUIStyle(GUI.skin.button);
                    optionStyle.normal.textColor = Color.white;
                    optionStyle.fontStyle = FontStyle.Bold;
                    optionStyle.fontSize = (int)CalculateDesiredFontSize(40f);

                    //TODO imgui是否能动态加载，卸载不必要的内容？
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (GUI.Button(new Rect(0f, (dropdownRect.height / 6) * i,
                                dropdownRect.width, dropdownRect.height / 6), options[i], optionStyle))
                        {
                            SetCurIndex(i);
                        }
                    }

                    GUI.EndScrollView();
                    maxRectY = dropdownRect.height + buttonRect.height;
                }

                else
                {
                    maxRectY = buttonRect.height;
                }
            }
        }

        #endregion

        #region CreateToggle

        protected bool CreateToggle(string label, bool boolean)
        {
            GUIStyle style = new GUIStyle(GUI.skin.toggle);
            style.fontSize = 35;
            style.hover.background = toggleBackGround;
            style.onHover.background = toggleBackGround;
            GUIContent content = new GUIContent(label);
            Vector2 size = style.CalcSize(content);
            bool rlt = GUI.Toggle(new Rect(lineMaxRectX, windowMaxRectY, size.x, size.y), boolean, content, style);

            SetLineRectMaxY(size.y);
            AddLineRectX(size.x + PURE_SPACE_WIDTH);
            return rlt;
        }


        protected bool CreateToggleAndNewLine(string label, bool boolean)
        {
            GUIStyle style = new GUIStyle(GUI.skin.toggle);
            style.fontSize = 35;
            style.hover.background = toggleBackGround;
            style.onHover.background = toggleBackGround;
            GUIContent content = new GUIContent(label);
            Vector2 size = style.CalcSize(content);
            bool rlt = GUI.Toggle(new Rect(lineMaxRectX, windowMaxRectY, size.x, size.y), boolean, content, style);

            AddLineRectX(size.x + PURE_SPACE_WIDTH);
            ResetLineRectMaxX();
            AddWindowRectY(size.y * 2 + PURE_SPACE_HEIGHT);
            return rlt;
        }

        #endregion


        /// <summary>
        /// 创建纹理的方法，不要用在update逻辑
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        protected Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        #endregion
    }
}



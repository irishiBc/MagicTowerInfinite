using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.QA.Shared;

namespace Game.QA
{
    public class GUI4QAMainWindow : GUI4QATester
    {
        #region private member

        /// <summary>
        /// 所有GUI4QATesterAttribute特性的类型
        /// </summary>
        private Type[] AllScripts;

        #region GM Concerned

        private int itemID;
        private int itemCount;

        private int equipID;
        private int equipLevel;

        private int userID;

        private string inputText;

        private int levelID;

        private int funcionID;

        private GMDropDown serverTypeDD;

        private Queue<string> gmCache;

        private GMDropDown gmCacheDD;

        private string singleCacheGm;

        #endregion

        #endregion

        #region interface

        protected override void Awake()
        {
            base.Awake();
            itemID = 1;
            itemCount = 100;
            
            equipLevel = 1;
            userID = 0;

            inputText = "ai 1 100";
            

            gmCache = new Queue<string>();
            singleCacheGm = string.Empty;
            RefreshGmCacheDropDown();
            
        }

        protected override void OnStart()
        {
            AllScripts = GetAllScripts(out string[] allScriptsNames);
            //AllScriptsNames = allScriptsNames;
            buttonName2Actions = new Dictionary<string, Action>();
            string[] allWindowsClass = Enum.GetNames(typeof(GUI4QATesterWindows));
            foreach (string c in allWindowsClass)
            {
                int index = Array.IndexOf(allScriptsNames, c);
                GUI4QATesterAttribute attr =
                    AllScripts[index].GetCustomAttribute(typeof(GUI4QATesterAttribute), true) as GUI4QATesterAttribute;
                buttonName2Actions.Add(attr.Name, () => { DefineScriptWindow(AllScripts[index]); });
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RefreshGmCacheDropDown();
        }

        protected override void CustomGUIContent()
        {
            base.CustomGUIContent();

            CreateLabelWithoutNewLine("GM指令");
            inputText = CreateTextField(inputText, WINDOW_WIDTH / 2f);
            CreateButtonAndNewLine("发送指令", () =>
            {
                GMUtils.TryCommands(inputText);
                if (gmCache.Count >= 5)
                {
                    if (gmCache.Contains(inputText))
                    {
                        return;
                    }

                    gmCache.Dequeue();
                    gmCache.Enqueue(inputText);
                }
                else
                {
                    if (gmCache.Contains(inputText))
                    {
                        return;
                    }

                    gmCache.Enqueue(inputText);
                }

                RefreshGmCacheDropDown();
            });

            if (gmCacheDD != null)
                CreateLabelWithoutNewLine("指令缓存");
            singleCacheGm = (string)CreateDropDownAndNewLine(gmCacheDD);

            CreateButton("查看游戏Log", () =>
            {
                DebugManager.Instance.ShowDebugConsole();
                DebugManager.Instance.CloseMainPanel();
            });

           
        }

        #endregion

        #region private methods

        private void RefreshGmCacheDropDown()
        {
            if (gmCache == null)
            {
                gmCacheDD = null;
                return;
            }

            int len = gmCache.Count;
            if (len > 0)
            {
                string[] strs = new string[len];
                object[] objs = new object[len];
                var list = gmCache.Reverse().ToList();
                for (int i = 0; i < len; i++)
                {
                    objs[i] = list[i];
                    strs[i] = list[i];
                }

                gmCacheDD = new GMDropDown(strs, objs);
                gmCacheDD.OnSelect += (obj) => { inputText = (string)obj; };
            }
            else
            {
                gmCacheDD = null;
            }
        }

        


        /// <summary>
        /// 获得所有特性的脚本类型
        /// </summary>
        /// <param name="allScriptsNames"></param>
        /// <returns></returns>
        private Type[] GetAllScripts(out string[] allScriptsNames)
        {
            Assembly asm = Assembly.GetAssembly(typeof(GUI4QATesterAttribute));
            Type[] types = asm.GetExportedTypes();
            Func<Attribute[], bool> IsGUI4QATester = o =>
            {
                foreach (Attribute a in o)
                {
                    if (a is GUI4QATesterAttribute)
                        return true;
                }

                return false;
            };

            Type[] allScripts =
                types.Where(o => { return IsGUI4QATester(Attribute.GetCustomAttributes(o, true)); }
                ).ToArray();

            allScriptsNames = new string[allScripts.Length];
            for (int i = 0; i < allScriptsNames.Length; i++)
            {
                allScriptsNames[i] = allScripts[i].Name;
            }

            return allScripts;
        }

        private void DefineScriptWindow(Type t)
        {
            DebugManager.Instance.SetComponent(t);
        }

        #endregion
    }


    /// <summary>
    /// 对应特性
    /// </summary>
    public class GUI4QATesterAttribute : Attribute
    {
        public string Name;

        public GUI4QATesterAttribute(string name)
        {
            Name = name;
        }
    }


    /// <summary>
    /// 只有配置在这儿的脚本名才会生成按钮
    /// </summary>
    public enum GUI4QATesterWindows
    {
        GUI4DebugServerTime,
    }
}
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]//针对哪个类型的property属性进行绘制

public class SceneNameDrawer : PropertyDrawer
{
    int sceneIndex = -1;//默认无场景

    GUIContent[] sceneNames;

    readonly string[] scenePathSplit = { "/", ".unity" };//用于切割字符串的方法

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0) return;
        //scenes是一个数组，如果数组的长度为0
        //return
        if (sceneIndex == -1)//一个都没选的时
            GetSceneNameArray(property);//获取所有场景

        int oldIndex = sceneIndex;//如果现在选中的序号等于 原来的 序号

        sceneIndex = EditorGUI.Popup(position,label,sceneIndex,sceneNames);//就把现在的序号给它

        if(oldIndex != sceneIndex)//点了一个新的序号

            //string数组里面对应的  现在选中的  序号 对应的文本
            //给特性 的 字符串值
            //牛的 意思就是一直都在画 选一下画一下
        property.stringValue = sceneNames[sceneIndex].text;
    }

    private void GetSceneNameArray(SerializedProperty property)
    {
        var scenes = EditorBuildSettings.scenes;//数组里面的某一个

        //初始化数组
        sceneNames = new GUIContent[scenes.Length];

        for(int i = 0;i < sceneNames.Length;i++)
        {
            string path = scenes[i].path;//每一个拿到路径
            string[] splitpath = path.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);

            string sceneName = "";

            if(splitpath.Length > 0)//如果拿到路径成功则splitpath一定 > 0
            {
                sceneName = splitpath[splitpath.Length - 1];
            }
            else
            {
                sceneName = "(Deleted Scene)";
            }
            sceneNames[i] = new GUIContent(sceneName);//把单独的、新的string场景写成string数组
        }
        if(sceneNames.Length ==0)//如果没有场景可以拿
        {
            sceneNames = new[] { new GUIContent("Check Your Build Settings") };
        }

        //一些猪脑过载的判断
        //如果说 属性 里面的 字符串 为空
        if(!string.IsNullOrEmpty(property.stringValue)) 
            {
                bool nameFound = false;//是否找到名字bool

                //循环string数组类型的变量（里面是拿到路径的场景）
                for (int i = 0; i < sceneNames.Length; i++)
                {
                    //如果说 数组里面包含的 文本 == 特性里面 字符串的 值
                    if (sceneNames[i].text == property.stringValue)
                    {
                        sceneIndex = i;//报告这是第几个场景
                        nameFound = true;//标记“已找到”
                        break;
                    }
                }
                if (nameFound == false)//如果没找到名字
                    sceneIndex = 0;//默认给第一个场景
            }
            else
            {
                sceneIndex = 0;
            }
        //场景数组中【序号】的文本 给 —— 特性里面 字符串的值
            property.stringValue = sceneNames[sceneIndex].text;            
    }
}
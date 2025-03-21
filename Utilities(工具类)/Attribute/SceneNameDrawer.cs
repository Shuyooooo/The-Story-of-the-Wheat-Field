using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]//����ĸ����͵�property���Խ��л���

public class SceneNameDrawer : PropertyDrawer
{
    int sceneIndex = -1;//Ĭ���޳���

    GUIContent[] sceneNames;

    readonly string[] scenePathSplit = { "/", ".unity" };//�����и��ַ����ķ���

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0) return;
        //scenes��һ�����飬�������ĳ���Ϊ0
        //return
        if (sceneIndex == -1)//һ����ûѡ��ʱ
            GetSceneNameArray(property);//��ȡ���г���

        int oldIndex = sceneIndex;//�������ѡ�е���ŵ��� ԭ���� ���

        sceneIndex = EditorGUI.Popup(position,label,sceneIndex,sceneNames);//�Ͱ����ڵ���Ÿ���

        if(oldIndex != sceneIndex)//����һ���µ����

            //string���������Ӧ��  ����ѡ�е�  ��� ��Ӧ���ı�
            //������ �� �ַ���ֵ
            //ţ�� ��˼����һֱ���ڻ� ѡһ�»�һ��
        property.stringValue = sceneNames[sceneIndex].text;
    }

    private void GetSceneNameArray(SerializedProperty property)
    {
        var scenes = EditorBuildSettings.scenes;//���������ĳһ��

        //��ʼ������
        sceneNames = new GUIContent[scenes.Length];

        for(int i = 0;i < sceneNames.Length;i++)
        {
            string path = scenes[i].path;//ÿһ���õ�·��
            string[] splitpath = path.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);

            string sceneName = "";

            if(splitpath.Length > 0)//����õ�·���ɹ���splitpathһ�� > 0
            {
                sceneName = splitpath[splitpath.Length - 1];
            }
            else
            {
                sceneName = "(Deleted Scene)";
            }
            sceneNames[i] = new GUIContent(sceneName);//�ѵ����ġ��µ�string����д��string����
        }
        if(sceneNames.Length ==0)//���û�г���������
        {
            sceneNames = new[] { new GUIContent("Check Your Build Settings") };
        }

        //һЩ���Թ��ص��ж�
        //���˵ ���� ����� �ַ��� Ϊ��
        if(!string.IsNullOrEmpty(property.stringValue)) 
            {
                bool nameFound = false;//�Ƿ��ҵ�����bool

                //ѭ��string�������͵ı������������õ�·���ĳ�����
                for (int i = 0; i < sceneNames.Length; i++)
                {
                    //���˵ ������������� �ı� == �������� �ַ����� ֵ
                    if (sceneNames[i].text == property.stringValue)
                    {
                        sceneIndex = i;//�������ǵڼ�������
                        nameFound = true;//��ǡ����ҵ���
                        break;
                    }
                }
                if (nameFound == false)//���û�ҵ�����
                    sceneIndex = 0;//Ĭ�ϸ���һ������
            }
            else
            {
                sceneIndex = 0;
            }
        //���������С���š����ı� �� ���� �������� �ַ�����ֵ
            property.stringValue = sceneNames[sceneIndex].text;            
    }
}
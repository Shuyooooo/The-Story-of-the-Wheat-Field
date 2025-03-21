namespace Mfarm.Save
{
    /// <summary>
    /// ��ȡ���ݡ��洢����(API)
    /// </summary>
    public interface Isavable
    {
        string GUID { get; }
        void RegisterSavable()
        {
            SaveLoadManager.Instance.RegisterSavable(this);
        }

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        /// <returns></returns>
        GameSaveData GenerateSaveData();

        /// <summary>
        /// �洢�������ݣ���ֵ��
        /// </summary>
        /// <param name="saveData"></param>
        void RestoreData(GameSaveData saveData);
    }
}

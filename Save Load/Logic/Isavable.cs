namespace Mfarm.Save
{
    /// <summary>
    /// 读取数据、存储数据(API)
    /// </summary>
    public interface Isavable
    {
        string GUID { get; }
        void RegisterSavable()
        {
            SaveLoadManager.Instance.RegisterSavable(this);
        }

        /// <summary>
        /// 读取所有数据
        /// </summary>
        /// <returns></returns>
        GameSaveData GenerateSaveData();

        /// <summary>
        /// 存储所有数据（赋值）
        /// </summary>
        /// <param name="saveData"></param>
        void RestoreData(GameSaveData saveData);
    }
}

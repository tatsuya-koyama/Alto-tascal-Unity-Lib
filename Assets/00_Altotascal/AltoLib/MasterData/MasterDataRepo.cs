using System.Collections.Generic;
using UnityEngine;

namespace AltoLib
{
    public interface IMasterDataRepo
    {
        bool LoadData();
        void PreprocessData();
    }

    public class MasterDataRepo<TDataTable, TSchema> : IMasterDataRepo
        where TDataTable : class, IMasterDataTable<TSchema>
        where TSchema : IMasterDataSchema
    {
        protected TDataTable dataTable;

        protected virtual string DataPath()
        {
            return "MasterData/" + typeof(TDataTable).Name;
        }

        public virtual bool LoadData()
        {
            dataTable = Resources.Load(DataPath()) as TDataTable;
            if (dataTable == null)
            {
                Debug.LogError("Master data load error : " + DataPath());
            }
            return (dataTable != null);
        }

        /// <summary>
        /// データを辞書にまとめるなど、ロード後の前処理を行いたい場合はここに書く
        /// </summary>
        public virtual void PreprocessData()
        {
        }

        //----------------------------------------------------------------------
        // 基本的なアクセサ
        //----------------------------------------------------------------------

        public List<TSchema> All()
        {
            return dataTable.records;
        }

        public virtual TSchema GetById(int id)
        {
            return All().Find(data => data.PrimaryId == id);
        }

        public virtual TSchema GetByKey(string key)
        {
            return All().Find(data => data.PrimaryKey == key);
        }
    }
}

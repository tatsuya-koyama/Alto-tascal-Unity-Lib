using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace AltoLib.Tests
{
    public enum MasterDataTableTestCategory
    {
        None,
        Attack,
        Support,
    }

    [Serializable]
    public class MasterDataTableTestSchema : IMasterDataSchema
    {
        public int id;
        public string text;
        public string customText;
        public MasterDataTableTestCategory category;
        public List<int> intValues;
        public List<string> stringValues;
        public List<MasterDataTableTestCategory> categoryValues;

        public int PrimaryId => id;
        public string PrimaryKey => null;
    }

    public class MasterDataTableForTest : MasterDataTable<MasterDataTableTestSchema>
    {
        protected override bool CustomImport(
            MasterDataTableTestSchema dataRecord,
            string key,
            string value
        )
        {
            if (key != nameof(MasterDataTableTestSchema.customText)) { return false; }
            dataRecord.customText = value;
            return true;
        }
    }

    public class MasterDataTableTest
    {
        [Test]
        public void Import_DecodesCommaAndLineBreakTags()
        {
            var table = ScriptableObject.CreateInstance<MasterDataTableForTest>();
            try
            {
                table.Import(new List<string>
                {
                    "id,text,customText",
                    "1,hello<comma>world<br>next,custom<comma>value<br>next",
                });

                Assert.That(table.records.Count, Is.EqualTo(1));
                Assert.That(table.records[0].text, Is.EqualTo("hello,world\nnext"));
                Assert.That(table.records[0].customText, Is.EqualTo("custom,value\nnext"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void Import_ConvertsEnumAndListFields()
        {
            var table = ScriptableObject.CreateInstance<MasterDataTableForTest>();
            try
            {
                table.Import(new List<string>
                {
                    "id,category,intValues,stringValues,categoryValues",
                    "1,Attack,1<comma>2<comma>3,cat<comma>dog,Attack<comma>Support",
                });

                Assert.That(table.records.Count, Is.EqualTo(1));
                Assert.That(
                    table.records[0].category,
                    Is.EqualTo(MasterDataTableTestCategory.Attack)
                );
                Assert.That(table.records[0].intValues, Is.EqualTo(new[] { 1, 2, 3 }));
                Assert.That(table.records[0].stringValues, Is.EqualTo(new[] { "cat", "dog" }));
                Assert.That(
                    table.records[0].categoryValues,
                    Is.EqualTo(new[]
                    {
                        MasterDataTableTestCategory.Attack,
                        MasterDataTableTestCategory.Support,
                    })
                );
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void Import_ConvertsEmptyCellToEmptyList()
        {
            var table = ScriptableObject.CreateInstance<MasterDataTableForTest>();
            try
            {
                table.Import(new List<string>
                {
                    "id,intValues",
                    "1,",
                });

                Assert.That(table.records[0].intValues, Is.Empty);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void Import_ThrowsWhenEnumValueIsUndefined()
        {
            var table = ScriptableObject.CreateInstance<MasterDataTableForTest>();
            try
            {
                Assert.Throws<FormatException>(() => table.Import(new List<string>
                {
                    "id,category",
                    "1,Unknown",
                }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void Import_ThrowsWhenListContainsEmptyElement()
        {
            var table = ScriptableObject.CreateInstance<MasterDataTableForTest>();
            try
            {
                Assert.Throws<FormatException>(() => table.Import(new List<string>
                {
                    "id,intValues",
                    "1,1<comma><comma>3",
                }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }

        [Test]
        public void Import_KeepsExistingRecordsWhenConversionFails()
        {
            var table = ScriptableObject.CreateInstance<MasterDataTableForTest>();
            try
            {
                table.Import(new List<string>
                {
                    "id,category",
                    "1,Attack",
                });

                Assert.Throws<FormatException>(() => table.Import(new List<string>
                {
                    "id,category",
                    "2,Unknown",
                }));

                Assert.That(table.records.Count, Is.EqualTo(1));
                Assert.That(table.records[0].id, Is.EqualTo(1));
                Assert.That(
                    table.records[0].category,
                    Is.EqualTo(MasterDataTableTestCategory.Attack)
                );
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(table);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using iAnywhere.Data.SQLAnywhere;

namespace FastSchemaProvider.Test
{
    [TestFixture]
    public class FastSchemaProviderTest
    {
        private static string SchemaSourceDSN = "paradat";
        private SAConnection con;

        [TestFixtureSetUp]
        public void TestBootStrapper()
        {
            con = new SAConnection("dsn=" + SchemaSourceDSN);
        }

        [TestFixtureTearDown]
        public void TestTearDown()
        {
            con.Close();
            con.Dispose();
            con = null;
        }

        [TestCase]
        public void OverallReadSchemaTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            provider.ReadSchema();
        }

        [TestCase]
        public void ColumnsTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockReadColumns();

            Assert.AreEqual(7093, count);
        }

        [TestCase]
        public void TablesTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockReadTables();

            Assert.AreEqual(668, count);
        }

        [TestCase]
        public void CombineColumnsAndTableTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            Assert.AreEqual(0, provider.MockCombineColumnsAndTabless());
        }

        [TestCase]
        public void ForeignKeyTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockReadForeignKeys();

            Assert.AreEqual(755, count);
        }

        [TestCase]
        public void InidzesTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockReadIndizes();

            Assert.AreEqual(0, count);
        }

        [TestCase]
        public void CombineForeignKeysAndTablesTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            Assert.AreEqual(0, provider.MockCombineForeignKeysAndTables());
        }

        [TestCase]
        public void CombineTriggersAndTablesTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockCombineTriggersAndTables();

            Assert.AreEqual(0, count);
        }

        [TestCase]
        public void ReadViewsTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockReadViews();

            Assert.AreEqual(30, count);
        }

        [TestCase]
        public void ReadProceduresTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);

            var count = provider.MockReadProcedures();

            Assert.AreEqual(322, count);
        }

        [TestCase]
        public void ToDiskTest()
        {
            FastSchemaProviderMock provider = new FastSchemaProviderMock(con);
            provider.ReadSchema();
            
            new FastSchemaSerializer().WriteToDisk(provider, @"c:\testdata.txt");
        }

        [TestCase]
        public void FromDiskTest()
        {
            var provider = new FastSchemaSerializer().ReadFromDisk(@"c:\testdata.txt");

            Assert.NotNull(provider);
        }
    }
}

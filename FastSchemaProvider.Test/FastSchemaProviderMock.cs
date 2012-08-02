using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace FastSchemaProvider.Test
{
    internal class FastSchemaProviderMock : SchemaProvider
    {
        public FastSchemaProviderMock(IDbConnection connection)
            : base(connection, false)
        {

        }

        public int MockReadColumns()
        {
            try
            {
                OpenConnection();
                return ReadColumns();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockReadTables()
        {
            try
            {
                OpenConnection();
                return ReadTables();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockCombineColumnsAndTabless()
        {
            try
            {
                OpenConnection();
                ReadColumns();
                ReadTables();

                return CombineColumnsAndTables();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockReadForeignKeys()
        {
            try
            {
                OpenConnection();

                return ReadForeignKeys();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockCombineForeignKeysAndTables()
        {
            try
            {
                OpenConnection();

                ReadTables();
                ReadForeignKeys();
                return CombineForeignKeysAndTables();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockReadIndizes()
        {
            try
            {
                OpenConnection();

                ReadTables();
                return ReadIndizes();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockReadTigger()
        {
            try
            {
                OpenConnection();

                return ReadTigger();
            }
            finally
            {
                CloseConnection();
            }
        }
  
        public int MockCombineTriggersAndTables()
        {
            try
            {
                OpenConnection();

                ReadTables();
                ReadTigger();
                return CombineTriggersAndTables();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockReadViews()
        {
            try
            {
                OpenConnection();

                return ReadViews();
            }
            finally
            {
                CloseConnection();
            }
        }

        public int MockReadProcedures()
        {
            try
            {
                OpenConnection();

                return ReadProcedures();
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}

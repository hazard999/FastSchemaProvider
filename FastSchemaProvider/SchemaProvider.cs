using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace FastSchemaProvider
{
    public class SchemaProvider
    {
        private const string ColumnSQL = "select st.table_name as TableName, " +
            "sc.column_name as name, " +
            "(case when sc.[default]='autoincrement' then 1 else 0 end) as is_identity, " +
            "sd.domain_name, " +
            "(case when sc.width=32767 then 0 else sc.width end) as max_length, " +
            "sc.scale as scale, " +
            "(case when sc.[nulls]='Y' then 1 else 0 end) as is_nullable, " +
            "sc.[default] as defaultvalue, " +
            "sc.pkey as primarykey " +
            "from syscolumn sc " +
            "join sysdomain sd on sd.domain_id=sc.domain_id " +
            "join systable st on st.table_id=sc.table_id " +
            "join sysuserperm sup on sup.user_id = st.creator " +
            "where sup.user_name='DBA' and st.table_type = 'BASE' " +
            "order by table_name, column_id";

        private const string TableSQL = "Select table_name from systable where table_type = 'BASE' and creator = 1";

        private const string ForeignKeySQL = "SELECT " +
"t1.table_name as strTableName, " +
"fk.role as strFKName, " +
"(select list(c.column_name) " +
"from sysforeignkey k " +
"join sysfkcol fc on k.foreign_table_id = fc.foreign_table_id " +
"AND fc.foreign_key_id = k.foreign_key_id " +
"join systable t1 on k.foreign_table_id = t1.table_id " +
"join syscolumn c on t1.table_id = c.table_id " +
"AND c.column_id = foreign_column_id " +
"where k.foreign_table_id = fk.foreign_table_id " +
"AND k.foreign_key_id = fk.foreign_key_id " +
") as strFKColumns, " +
"t2.table_name as strReferencedFKTable, " +
"(select list(c.column_name) " +
"from sysforeignkey k " +
"join sysfkcol fc on k.foreign_table_id = fc.foreign_table_id " +
"AND fc.foreign_key_id = k.foreign_key_id " +
"join systable t1 on k.primary_table_id = t1.table_id " +
"join syscolumn c on t1.table_id = c.table_id " +
"AND c.column_id = fc.primary_column_id " +
"where k.foreign_table_id = fk.foreign_table_id " +
"AND k.foreign_key_id = fk.foreign_key_id " +
"AND k.primary_table_id = fk.primary_table_id " +
") as strReferencedFKColumns, " +
"(select CASE COALESCE ( t.referential_action, 'U' ) " +
"WHEN 'C' THEN 'CASCADE' " +
"WHEN 'D' THEN 'SET DEFAULT' " +
"WHEN 'N' THEN 'SET NULL' " +
"WHEN 'U' THEN 'RESTRICT' " +
"END " +
"from SYSTRIGGER t " +
"where t.table_id = fk.primary_table_id " +
"and t.foreign_table_id= fk.foreign_table_id " +
"and foreign_key_id=fk.foreign_key_id and event = 'C' " +
") AS strUpdateAction, " +
"(select CASE COALESCE ( t.referential_action, 'UNKNOWN' ) " +
"WHEN 'C' THEN 'CASCADE' " +
"WHEN 'D' THEN 'SET DEFAULT' " +
"WHEN 'N' THEN 'SET NULL' " +
"ELSE 'RESTRICT' " +
"END " +
"from SYSTRIGGER t " +
"where table_id=fk.primary_table_id " +
"and foreign_table_id=fk.foreign_table_id " +
"and foreign_key_id=fk.foreign_key_id and event = 'D' " +
") AS strDeleteAction, " +
"fk.check_on_commit as strCheckOnCommit " +
"from sysforeignkey fk " +
"join sys.systable t1 on fk.foreign_table_id = t1.table_id " +
"join sys.systable t2 on fk.primary_table_id = t2.table_id " +
"join sys.sysuserperm u on u.user_id = t1.creator " +
"where u.user_name not in('sys','dbo')";

        private const string TriggerSQL = "select tname, trigname, trigdefn, event, trigtime from systriggers";
        
        private const string ViewSQL = "select svs.viewname, sv.view_def from sysview sv inner join systable t on view_object_id = object_id and table_type = 'VIEW' and t.creator = 1 inner join sysviews svs on svs.viewname = t.table_name";

        private const string ProcedureSQL = "select proc_name, proc_defn from sysprocedure where creator  = 1";

        private const string IndexSQL = "select table_name, index_name, [unique], column_name, sequence, [order] from SYSIDX " +
"inner join systable on SYSIDX.table_id = systable.table_id " +
"left outer join SYSIDXCOL on SYSIDXCOL.table_id = systable.table_id and SYSIDXCOL.index_id = SYSIDX.index_id " +
"inner join syscolumn on SYSIDXCOL.column_id = syscolumn.column_id and syscolumn.table_id = systable.table_id " +
"where SYSIDX.index_category = 3 and systable.table_type = 'BASE' and systable.creator = 1 " +
"order by table_name, index_name, sequence";

        private IDbConnection Connection;
        private IList<Column> Columns;
        private IList<ForeignKey> ForeignKeys;
        private IList<Trigger> Triggers;

        private bool WasSchemaAllreadyRead;

        public SchemaProvider()
        {
            Columns = new List<Column>();
            Tables = new List<Table>();
            ForeignKeys = new List<ForeignKey>();
            Triggers = new List<Trigger>();
            Views = new List<View>();
            Procedures = new List<Procedure>();
        }

        public SchemaProvider(IDbConnection connection, bool ReadImmediate = true)
            : this()
        {
            Connection = connection;

            if (ReadImmediate)
                ReadSchema();
        }

        public IList<Table> Tables { get; set; }
        public IList<View> Views { get; set; }
        public IList<Procedure> Procedures { get; set; }

        public void ReadSchema()
        {
            try
            {
                if (WasSchemaAllreadyRead)
                    return;

                OpenConnection();

                ReadColumns();
                ReadTables();

                CombineColumnsAndTables();

                ReadForeignKeys();
                CombineForeignKeysAndTables();

                ReadIndizes();

                ReadTigger();
                CombineTriggersAndTables();

                ReadViews();

                ReadProcedures();

                WasSchemaAllreadyRead = true;
            }
            finally
            {
                CloseConnection();
            }
        }

        public void ReeeadSchema()
        {
            WasSchemaAllreadyRead = false;
            ReadSchema();
        }

        protected int ReadColumns()
        {
            var command = Connection.CreateCommand();
            command.CommandText = ColumnSQL;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    Columns.Add(new Column(reader.GetString(1), reader.GetString(0), reader.GetBoolean(2), DbTypeLookup.GetSADbType(reader.GetString(3)) ?? SchemaDbType.Char, reader.GetInt32(4), reader.GetInt32(5), reader.GetBoolean(6), reader.GetValue(7) as string, (reader.GetValue(8) as string) == "Y"));
            }

            return Columns.Count;
        }

        protected int ReadTables()
        {
            var command = Connection.CreateCommand();
            command.CommandText = TableSQL;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                    Tables.Add(new Table(reader.GetString(0)));
            }

            return Tables.Count;
        }

        protected int CombineColumnsAndTables()
        {
            Tables.AsParallel().ForAll(table =>
            {
                var cols = Columns.Where(c => c.TableName == table.ActualName);
                table.Columns.AddRange(cols);

                table.BuildPrimaryKey();
            }
            );

            return Columns.Count - Tables.Sum(tab => tab.Columns.Count);
        }

        protected int ReadForeignKeys()
        {
            var command = Connection.CreateCommand();
            command.CommandText = ForeignKeySQL;

            IList<ReadForeignKey> readForeignKeys = new List<ReadForeignKey>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    readForeignKeys.Add(new ReadForeignKey()
                    {
                        TableName = reader.GetString(0),
                        ConstraintName = reader.GetString(1),
                        ColumnName = reader.GetString(2),
                        UniqueTableName = reader.GetString(3),
                        UniqueColumnName = reader.GetString(4),
                        UpdateAction = reader.GetValue(5) as string ?? "RESTRICT",
                        DeleteAction = reader.GetValue(6) as string ?? "RESTRICT",
                        CheckOnCommit = reader.GetString(7) == "Y"
                    });
                }
            }

            foreach (var readForeignKey in readForeignKeys)
                ForeignKeys.Add(new ForeignKey(readForeignKey.TableName, readForeignKey.ConstraintName, readForeignKey.ColumnName.Split(','), readForeignKey.UniqueTableName, readForeignKey.UniqueColumnName.Split(','), readForeignKey.UpdateAction, readForeignKey.DeleteAction, readForeignKey.CheckOnCommit));

            return ForeignKeys.Count;
        }

        protected int CombineForeignKeysAndTables()
        {
            foreach (var fk in ForeignKeys)
            {
                var table = Tables.Where(t => t.ActualName == fk.DetailTable).FirstOrDefault();
                table.ForeignKeys.Add(fk);
            }

            return ForeignKeys.Count - Tables.Sum(tab => tab.ForeignKeys.Count);
        }

        protected int ReadIndizes()
        {
            var command = Connection.CreateCommand();
            command.CommandText = IndexSQL;

            IList<ReadIndex> readIndizes = new List<ReadIndex>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    readIndizes.Add(new ReadIndex()
                    {
                        TableName = reader.GetString(0),
                        IndexName = reader.GetString(1),
                        IndexType = (IndexTypes)reader.GetInt32(2),
                        ColumnName = reader.GetString(3),
                        Sequence = reader.GetInt32(4),
                        Order = reader.GetString(5) == "A" ? Ordering.Ascending : Ordering.Descending
                    });
                }
            }

            var groups = readIndizes.GroupBy(index => new { index.TableName, index.IndexName, index.IndexType }).ToList();

            foreach (var group in groups)
            {
                var FoundTable = Tables.Where(table => table.ActualName == group.Key.TableName).FirstOrDefault();
                var index = new Index() { IndexName = group.Key.IndexName, IndexType = group.Key.IndexType };

                foreach (var col in group)
                    index.Columns.Add(new IndexColumn() { ColumnName = col.ColumnName, Order = col.Order, Sequence = col.Sequence });

                FoundTable.Indizes.Add(index);
            }

            return readIndizes.Count - Tables.Sum(tab => tab.Indizes.Sum(index => (index == null) ? 0 : index.Columns.Count));
        }

        protected int ReadTigger()
        {
            var command = Connection.CreateCommand();
            command.CommandText = TriggerSQL;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Triggers.Add(
                    new Trigger()
                    {
                        Table = reader.GetString(0),
                        Name = reader.GetString(1),
                        Definition = reader.GetString(2),
                        EventAsString = reader.GetString(3),
                        TimeAsString = reader.GetString(4)
                    }
                    );
                }
            }

            return Triggers.Count;
        }

        protected int CombineTriggersAndTables()
        {
            foreach (var Trigger in Triggers)
            {
                var FoundTable = Tables.Where(Table => Table.ActualName == Trigger.Table).FirstOrDefault();

                if (FoundTable != null)
                    FoundTable.Trigger.Add(Trigger);
            }

            return Triggers.Count - Tables.Sum(TableToSum => TableToSum.Trigger.Count);
        }

        protected int ReadViews()
        {
            var command = Connection.CreateCommand();
            command.CommandText = ViewSQL;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Views.Add(
                    new View()
                    {
                        Name = reader.GetString(0),
                        Definition = reader.GetString(1),
                    }
                    );
                }
            }

            return Views.Count;
        }

        protected int ReadProcedures()
        {
            var command = Connection.CreateCommand();
            command.CommandText = ProcedureSQL;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Procedures.Add(
                    new Procedure()
                    {
                        Name = reader.GetString(0),
                        Definition = reader.GetString(1),
                    }
                    );
                }
            }

            return Procedures.Count;
        }

        protected internal void OpenConnection()
        {
            Connection.Open();
        }

        protected internal void CloseConnection()
        {
            Connection.Close();
        }
    }
}



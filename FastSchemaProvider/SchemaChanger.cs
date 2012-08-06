using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace FastSchemaProvider
{
    public class SchemaChanger
    {
        SchemaProvider OldSchema;
        SchemaProvider NewSchema;
        IDbConnection ConnectionToOldDatabase;

        public SchemaChanger(SchemaProvider oldSchema, SchemaProvider newSchema, IDbConnection connectionToOldDatabase)
        {
            OldSchema = oldSchema;
            NewSchema = newSchema;
            ConnectionToOldDatabase = connectionToOldDatabase;
        }

        public void Upgrade()
        {
            try
            {
                ConnectionToOldDatabase.Open();

                IEnumerable<Table> TablesToDrop = SearchForTablesToDrop();

                DropTablesThatAreToMuch(TablesToDrop);

                IEnumerable<Table> MissingTables = SearchForMissingTables();

                CreateMissingTables(MissingTables);

                IDictionary<Table, Column> MissingColumns = SearchForMissingColumns();

                CreateMissingColumns(MissingColumns);

                IEnumerable<ColumnToChange> ColumnsToChange = SearchForColumnsToChange();

                ModifyColumns(ColumnsToChange);

                IDictionary<Table, Column> ColumnsToDelete = SearchForColumnsToDelete();

                DeleteColumnsThatAreToMuch(ColumnsToDelete);

                IDictionary<Table, IList<ForeignKey>> MissingForeignKeysFromMissingTables = GetForeignKeysFromMissingTables(MissingTables);

                CreateMissingForeignKeys(MissingForeignKeysFromMissingTables);

                IDictionary<Table, IList<Index>> MissingIndizes = SearchForMissingIndizes();

                CreateMissingIndizes(MissingIndizes);

                IDictionary<Table, IList<Index>> IndizesThatAreToMuch = SearchForIndizesThatAreToMuch();

                DropIndizesThatAreToMuch(IndizesThatAreToMuch);

                IDictionary<Table, IList<ForeignKey>> MissingForeignKeys = SearchForMissingForeignKeys();

                CreateMissingForeignKeys(MissingForeignKeys);
                
                IEnumerable<Procedure> ProceduresThatAreToMuch = SearchForProceduresToDrop();

                DropProceduresThatAreToMuch(ProceduresThatAreToMuch);

                IEnumerable<Procedure> MissingProcedures = SearchForMissingProcedure();

                CreateMissingProcedures(MissingProcedures);

                IEnumerable<Procedure> ProceduresToAlter = SearchProcedureToAlter();

                AlterProcedures(ProceduresToAlter);

                IEnumerable<View> ViewsThatAreToMuch = SearchForViewsToDrop();

                DropViewsThatAreToMuch(ViewsThatAreToMuch);

                IEnumerable<View> MissingViews = SearchForMissingViews();

                CreateMissingViews(MissingViews);

                IEnumerable<View> ViewsToAlter = SearchViewsToAlter();

                AlterViews(ViewsToAlter);
            }
            finally
            {
                ConnectionToOldDatabase.Close();
            }
        }

        private IEnumerable<Table> SearchForTablesToDrop()
        {
            foreach (var Table in OldSchema.Tables)
            {
                var TableToDrop = NewSchema.Tables.Where(oldTable => oldTable.ActualName == Table.ActualName).FirstOrDefault();

                if (TableToDrop == null)
                    yield return Table;
            }
        }

        private void DropTablesThatAreToMuch(IEnumerable<Table> TablesToDrop)
        {
            foreach (var TableToMuch in TablesToDrop)
            {
                var CommandString = String.Format("DROP TABLE [{0}]", TableToMuch.ActualName);

                ExecuteCommandOnOldDatabase(CommandString);
            }
        }

        private IEnumerable<Table> SearchForMissingTables()
        {
            foreach (var Table in NewSchema.Tables)
            {
                var TableToCreate = OldSchema.Tables.Where(oldTable => oldTable.ActualName == Table.ActualName).FirstOrDefault();

                if (TableToCreate == null)
                    yield return Table;
            }
        }

        private void CreateMissingTables(IEnumerable<Table> MissingTables)
        {
            foreach (var MissingTable in MissingTables)
            {
                var CommandString = String.Format("CREATE TABLE [{0}] ( ", MissingTable.ActualName);

                foreach (var Column in MissingTable.Columns)
                    CommandString += GetColumnDefinition(Column) + ",";

                if (MissingTable.PrimaryKey.Columns.Count > 0)
                {
                    CommandString += "PRIMARY KEY (";

                    foreach (var pkCol in MissingTable.PrimaryKey.Columns)
                        CommandString += String.Format("[{0}] ASC, ", pkCol);

                    CommandString = CommandString.TrimEnd(' ', ',');

                    CommandString += ") ";
                }
                else
                    CommandString = CommandString.TrimEnd(',');

                CommandString += ")";

                ExecuteCommandOnOldDatabase(CommandString);

                foreach (var Index in MissingTable.Indizes)
                    CreateIndexOrConstraint(MissingTable, Index);
            }
        }

        private IDictionary<Table, Column> SearchForMissingColumns()
        {
            IDictionary<Table, Column> Result = new Dictionary<Table, Column>();

            foreach (var TableInNewSchema in NewSchema.Tables)
            {
                var FoundTableInOldSchema = OldSchema.Tables.Where(oldTable => oldTable.ActualName == TableInNewSchema.ActualName).FirstOrDefault();

                if (FoundTableInOldSchema != null)
                {
                    foreach (var ColumnInNewSchema in TableInNewSchema.Columns)
                    {
                        var FoundColumn = FoundTableInOldSchema.Columns.Where(OldColumn => OldColumn.ActualName == ColumnInNewSchema.ActualName).FirstOrDefault();

                        if (FoundColumn == null)
                            Result.Add(TableInNewSchema, ColumnInNewSchema);
                    }
                }
            }

            return Result;
        }

        private void CreateMissingColumns(IDictionary<Table, Column> MissingColumns)
        {
            foreach (var MissingColumn in MissingColumns)
            {
                var CommandString = String.Format("ALTER TABLE [{0}] ADD ", MissingColumn.Key.ActualName);
                CommandString += GetColumnDefinition(MissingColumn.Value);

                ExecuteCommandOnOldDatabase(CommandString);
            }
        }

        private IEnumerable<ColumnToChange> SearchForColumnsToChange()
        {
            foreach (var TableFromNewSchema in NewSchema.Tables)
            {
                var FoundTableInOldSchema = OldSchema.Tables.Where(oldTable => oldTable.ActualName == TableFromNewSchema.ActualName).FirstOrDefault();

                if (FoundTableInOldSchema != null)
                {
                    foreach (var ColumnInNewSchema in TableFromNewSchema.Columns)
                    {
                        var FoundColumnInOldSchema = FoundTableInOldSchema.Columns.Where(OldColumn => OldColumn.ActualName == ColumnInNewSchema.ActualName).FirstOrDefault();

                        if (FoundColumnInOldSchema != null)
                            if (!ColumnInNewSchema.ColumnDefintionEquals(FoundColumnInOldSchema))
                                yield return new ColumnToChange() { Table = TableFromNewSchema, NewColumnDefinition = ColumnInNewSchema, OldColumnDefinition = FoundColumnInOldSchema };
                    }
                }
            }
        }

        private void ModifyColumns(IEnumerable<ColumnToChange> ColumnsToChange)
        {
            foreach (var ColumnToChange in ColumnsToChange)
            {
                var CommandString = String.Format("ALTER TABLE [{0}] ALTER ", ColumnToChange.Table.ActualName);
                CommandString += GetColumnDefinition(ColumnToChange.NewColumnDefinition);

                if (!string.IsNullOrEmpty(ColumnToChange.OldColumnDefinition.DefaultValue) && string.IsNullOrEmpty(ColumnToChange.NewColumnDefinition.DefaultValue))
                    CommandString += " DEFAULT NULL";

                ExecuteCommandOnOldDatabase(CommandString);
            }
        }

        private IDictionary<Table, Column> SearchForColumnsToDelete()
        {
            IDictionary<Table, Column> Result = new Dictionary<Table, Column>();

            foreach (var OldTable in OldSchema.Tables)
            {
                var FoundTableInNewSchema = NewSchema.Tables.Where(newTable => newTable.ActualName == OldTable.ActualName).FirstOrDefault();

                if (FoundTableInNewSchema != null)
                {
                    foreach (var OldColumn in OldTable.Columns)
                    {
                        var FoundColumn = FoundTableInNewSchema.Columns.Where(NewColumn => NewColumn.ActualName == OldColumn.ActualName).FirstOrDefault();

                        if (FoundColumn == null)
                            Result.Add(OldTable, OldColumn);
                    }
                }
            }

            return Result;
        }

        private void DeleteColumnsThatAreToMuch(IDictionary<Table, Column> ColumnsToDelete)
        {
            foreach (var ColumnToDelete in ColumnsToDelete)
            {
                var CommandString = String.Format("ALTER TABLE [{0}] DELETE [{1}]", ColumnToDelete.Key.ActualName, ColumnToDelete.Value.ActualName);

                ExecuteCommandOnOldDatabase(CommandString);
            }
        }

        private string GetColumnDefinition(Column Column)
        {
            string CommandString = String.Format("[{0}] ", Column.ActualName);

            if (Column.DataType.IsString())
            {
                if (Column.MaxLength == 0)
                    CommandString += String.Format("text");
                else
                    CommandString += String.Format("{0} ({1})", Column.DataType.GetRealDbType(), Column.MaxLength);
            }
            else
            {
                CommandString += String.Format("{0}", Column.DataType.GetRealDbType());

                if (Column.Scale > 0 || Column.IsScaleAble)
                    CommandString += String.Format("({0}, {1})", Column.MaxLength, Column.Scale);
            }

            if (!Column.IsNullAble)
                CommandString += " NOT";
            CommandString += " NULL";

            if (Column.IsIdentity)
                CommandString += " DEFAULT AUTOINCREMENT";
            else
                if (!String.IsNullOrEmpty(Column.DefaultValue))
                    CommandString += String.Format(" DEFAULT {0}", Column.DefaultValue);

            return CommandString;
        }

        private IDictionary<Table, IList<Index>> SearchForMissingIndizes()
        {
            IDictionary<Table, IList<Index>> Result = new Dictionary<Table, IList<Index>>();

            foreach (var table in NewSchema.Tables)
            {
                var FoundTable = OldSchema.Tables.Where(oldTable => oldTable.ActualName == table.ActualName).FirstOrDefault();

                if (FoundTable != null)
                {
                    foreach (var index in table.Indizes)
                    {
                        var FoundIndex = FoundTable.Indizes.Where(oldIndex => oldIndex.IndexName == index.IndexName).FirstOrDefault();

                        if (FoundIndex == null)
                        {
                            if (!Result.Keys.Contains(table))
                                Result.Add(table, new List<Index>());

                            Result[table].Add(index);
                        }
                    }
                }
            }

            return Result;
        }

        private void CreateMissingIndizes(IDictionary<Table, IList<Index>> missingIndizes)
        {
            foreach (var missing in missingIndizes)
                foreach (var missingIndex in missing.Value)
                    CreateIndexOrConstraint(missing.Key, missingIndex);
        }

     

        private void CreateIndexOrConstraint(Table table, Index missingIndex)
        {
            string CreateStatement = String.Empty;
            if (missingIndex.IndexType == IndexTypes.UnqiueConstraint)
            {
                CreateStatement = String.Format("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] UNIQUE ( ", table.ActualName, missingIndex.IndexName);

                foreach (var col in missingIndex.Columns.OrderBy(c => c.Sequence))
                {
                    CreateStatement += col.ColumnName;

                    if (col.Order == Ordering.Ascending)
                        CreateStatement += " ASC ";
                    else
                        CreateStatement += " DESC ";

                    CreateStatement += ", ";
                }

                CreateStatement = CreateStatement.TrimEnd(' ', ',');
                CreateStatement += ")";
            }
            else
            {
                CreateStatement = "CREATE ";

                if (missingIndex.IndexType == IndexTypes.Unqiue)
                    CreateStatement += "UNIQUE ";

                CreateStatement += "INDEX ";
                CreateStatement += String.Format("[{0}]", missingIndex.IndexName);
                CreateStatement += " ON ";

                CreateStatement += table.ActualName;
                CreateStatement += " ( ";

                foreach (var col in missingIndex.Columns.OrderBy(c => c.Sequence))
                {
                    CreateStatement += col.ColumnName;
                    if (col.Order == Ordering.Ascending)
                        CreateStatement += " ASC ";
                    else
                        CreateStatement += " DESC ";
                    CreateStatement += ", ";
                }

                CreateStatement = CreateStatement.TrimEnd(' ', ',');
                CreateStatement += ")";
            }

            ExecuteCommandOnOldDatabase(CreateStatement);
        }

        private IDictionary<Table, IList<Index>> SearchForIndizesThatAreToMuch()
        {
            IDictionary<Table, IList<Index>> Result = new Dictionary<Table, IList<Index>>();

            foreach (var TableInOldSchema in OldSchema.Tables)
            {
                var FoundTableInNewSchema = NewSchema.Tables.Where(NewTable => NewTable.ActualName == TableInOldSchema.ActualName).FirstOrDefault();

                if (FoundTableInNewSchema != null)
                {
                    foreach (var OldIndex in TableInOldSchema.Indizes)
                    {
                        var FoundIndex = FoundTableInNewSchema.Indizes.Where(NewIndex => NewIndex.IndexName == OldIndex.IndexName).FirstOrDefault();

                        if (FoundIndex == null)
                        {
                            if (!Result.Keys.Contains(TableInOldSchema))
                                Result.Add(TableInOldSchema, new List<Index>());

                            Result[TableInOldSchema].Add(OldIndex);
                        }
                    }
                }
            }

            return Result;

        }

        private void DropIndizesThatAreToMuch(IDictionary<Table, IList<Index>> indizesThatAreToMuch)
        {
            foreach (var ToMuch in indizesThatAreToMuch)
                foreach (var IndexToMuch in ToMuch.Value)
                    DropIndexOrConstraint(ToMuch.Key, IndexToMuch);
        }

        private void DropIndexOrConstraint(Table table, Index missingIndex)
        {
            string DropStatement = String.Empty;
            if (missingIndex.IndexType == IndexTypes.UnqiueConstraint)
                DropStatement = String.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}]", table.ActualName, missingIndex.IndexName);
            else
                DropStatement = String.Format("DROP INDEX [{1}]", table.ActualName, missingIndex.IndexName);

            ExecuteCommandOnOldDatabase(DropStatement);
        }

        private IDictionary<Table, IList<ForeignKey>> SearchForMissingForeignKeys()
        {
            IDictionary<Table, IList<ForeignKey>> Result = new Dictionary<Table, IList<ForeignKey>>();

            foreach (var table in NewSchema.Tables)
            {
                var FoundTable = OldSchema.Tables.Where(oldTable => oldTable.ActualName == table.ActualName).FirstOrDefault();

                if (FoundTable != null)
                {
                    foreach (var fkey in table.ForeignKeys)
                    {
                        var FoundKey = FoundTable.ForeignKeys.Where(oldFKey => oldFKey.Name == fkey.Name).FirstOrDefault();

                        if (FoundKey == null)
                        {
                            if (!Result.Keys.Contains(table))
                                Result.Add(table, new List<ForeignKey>());

                            Result[table].Add(fkey);
                        }
                    }
                }
            }

            return Result;
        }

        private IDictionary<Table, IList<ForeignKey>> GetForeignKeysFromMissingTables(IEnumerable<Table> missingTables)
        {
            IDictionary<Table, IList<ForeignKey>> Result = new Dictionary<Table, IList<ForeignKey>>();

            foreach (var table in missingTables)
            {
                foreach (var fkey in table.ForeignKeys)
                {
                    if (!Result.Keys.Contains(table))
                        Result.Add(table, new List<ForeignKey>());

                    Result[table].Add(fkey);
                }
            }

            return Result;
        }

        private void CreateMissingForeignKeys(IDictionary<Table, IList<ForeignKey>> missingForeignKeys)
        {
            foreach (var missing in missingForeignKeys)
                foreach (var missingForeignKey in missing.Value)
                {
                    var CreateStatement = String.Format("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] FOREIGN KEY ( ", missingForeignKey.DetailTable, missingForeignKey.Name);

                    foreach (var col in missingForeignKey.Columns)
                        CreateStatement += String.Format("[{0}], ", col);

                    CreateStatement = CreateStatement.TrimEnd(' ', ',');
                    CreateStatement += ") ";

                    CreateStatement += String.Format("REFERENCES {0} ( ", missingForeignKey.MasterTable);

                    foreach (var col in missingForeignKey.UniqueColumns)
                        CreateStatement += String.Format("[{0}], ", col);

                    CreateStatement = CreateStatement.TrimEnd(' ', ',');
                    CreateStatement += ") ";

                    CreateStatement += String.Format("ON UPDATE {0} ", missingForeignKey.UpdateAction);
                    CreateStatement += String.Format("ON DELETE  {0} ", missingForeignKey.DeleteAction);

                    if (missingForeignKey.CheckOnCommit)
                        CreateStatement += "CHECK ON COMMIT";

                    ExecuteCommandOnOldDatabase(CreateStatement);
                }
        }

        private IEnumerable<Procedure> SearchForProceduresToDrop()
        {
            foreach (var Procedure in OldSchema.Procedures)
            {
                var ProcedureToDrop = NewSchema.Procedures.Where(NewProcedure => NewProcedure.Name == Procedure.Name).FirstOrDefault();

                if (ProcedureToDrop == null)
                    yield return Procedure;
            }
        }

        private IEnumerable<Procedure> SearchForMissingProcedure()
        {
            foreach (var Procedure in NewSchema.Procedures)
            {
                var ProcedureToCreate = OldSchema.Procedures.Where(OldProcedure => OldProcedure.Name == Procedure.Name).FirstOrDefault();

                if (ProcedureToCreate == null)
                    yield return Procedure;
            }
        }
        
        private IEnumerable<Procedure> SearchProcedureToAlter()
        {
            foreach (var Procedure in NewSchema.Procedures)
            {
                var ProcedureToAlter = OldSchema.Procedures.Where(NewProcedure => NewProcedure.Name == Procedure.Name).FirstOrDefault();

                if (ProcedureToAlter != null)
                    if (!ProcedureToAlter.Definition.Equals(Procedure.Definition))
                        yield return Procedure;
            }
        }
        
        private void AlterProcedures(IEnumerable<Procedure> ProceduresToAlter)
        {
            DropProceduresThatAreToMuch(ProceduresToAlter);
            CreateMissingProcedures(ProceduresToAlter);
        }

        private void DropProceduresThatAreToMuch(IEnumerable<Procedure> ProceduresThatAreToMuch)
        {
            foreach (var ProcedureToMuch in ProceduresThatAreToMuch)
                ExecuteCommandOnOldDatabase(String.Format("DROP PROCEDURE [{0}]", ProcedureToMuch.Name));
        }

        private void CreateMissingProcedures(IEnumerable<Procedure> MissingProcedures)
        {
            foreach (var MissingProcedure in MissingProcedures)
                ExecuteCommandOnOldDatabase(MissingProcedure.Definition);
        }

        private IEnumerable<View> SearchForViewsToDrop()
        {
            foreach (var View in OldSchema.Views)
            {
                var ViewToDrop = NewSchema.Views.Where(NewView => NewView.Name == View.Name).FirstOrDefault();

                if (ViewToDrop == null)
                    yield return View;
            }
        }
        
        private IEnumerable<View> SearchForMissingViews()
        {
            foreach (var View in NewSchema.Views)
            {
                var ViewToCreate = OldSchema.Views.Where(OldView => OldView.Name == View.Name).FirstOrDefault();

                if (ViewToCreate == null)
                    yield return View;
            }
        }

        private IEnumerable<View> SearchViewsToAlter()
        {
            foreach (var View in NewSchema.Views)
            {
                var ViewToAlter = OldSchema.Views.Where(NewView => NewView.Name == View.Name).FirstOrDefault();

                if (ViewToAlter != null)
                    if (!ViewToAlter.Definition.Equals(View.Definition))
                        yield return View;
            }
        }

        private void AlterViews(IEnumerable<View> ViewsToAlter)
        {
            DropViewsThatAreToMuch(ViewsToAlter);
            CreateMissingViews(ViewsToAlter);
        }

        private void DropViewsThatAreToMuch(IEnumerable<View> ViewsThatAreToMuch)
        {
            foreach (var ViewToMuch in ViewsThatAreToMuch)
                ExecuteCommandOnOldDatabase(String.Format("DROP VIEW [{0}]", ViewToMuch.Name));
        }

        private void CreateMissingViews(IEnumerable<View> MissingViews)
        {
            foreach (var MissingView in MissingViews)
                ExecuteCommandOnOldDatabase(MissingView.Definition);
        }

        private void ExecuteCommandOnOldDatabase(string CommandString)
        {
            var createTableCommand = ConnectionToOldDatabase.CreateCommand();
            createTableCommand.CommandText = CommandString;
            createTableCommand.ExecuteNonQuery();
        }
    }
}
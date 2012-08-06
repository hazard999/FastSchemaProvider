using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FastSchemaProvider
{
    public class SchemaComparer
    {
        private SchemaProvider NewSchema;
        private SchemaProvider OldSchema;
        private SchemaComparerOptions Options;

        public SchemaDiff CompareSchematas(SchemaProvider newSchema, SchemaProvider oldSchema, SchemaComparerOptions options = null)
        {
            NewSchema = newSchema;
            OldSchema = oldSchema;

            Options = options;
            if (Options == null)
                Options = new SchemaComparerOptions();

            SchemaDiff diff = new SchemaDiff();

            diff.TablesToDrop = SearchForTablesToDrop();

            diff.TablesToCreate = SearchForMissingTables();

            return diff;
        }

        private IEnumerable<Table> SearchForTablesToDrop()
        {
            foreach (var Table in OldSchema.Tables)
            {
                var expression = new Func<Table, bool>(oldTable => oldTable.ActualName.ToLowerInvariant() == Table.ActualName.ToLowerInvariant());

                if (Options.OldSchemaTreatsViewsAsTable)
                    expression = new Func<Table, bool>(oldTable => oldTable.ActualName.ToLowerInvariant() == Table.ActualName.ToLowerInvariant() || !oldTable.ActualName.ToLowerInvariant().StartsWith(Options.OldSchemaTableAsViewPrefix));

                var TableToDrop = NewSchema.Tables.Where(expression).FirstOrDefault();
                
                if (TableToDrop == null)
                    yield return Table;
            }
        }

        private IEnumerable<Table> SearchForMissingTables()
        {
            foreach (var Table in NewSchema.Tables)
            {
                var TableToCreate = OldSchema.Tables.Where(oldTable => oldTable.ActualName.ToLowerInvariant() == Table.ActualName.ToLowerInvariant()).FirstOrDefault();

                if (TableToCreate == null)
                    yield return Table;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevExpress.ExpressApp.Utils;
using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using iAnywhere.Data.SQLAnywhere;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Core;
using FastSchemaProvider;
using DevExpress.Xpo.Metadata;
using DevExpress.Xpo;
using System.Runtime.InteropServices;
using DevExpress.Xpo.Helpers;
using DevExpress.Xpo.DB;

namespace FastXAFApplicationReader
{
    public class ApplicationReader
    {
        public SchemaProvider ReadApplication(string ApplicationName, string ApplicationFolder, string exeName, string XPOConnectionString, string DBConnectionString)
        {
            IList<string> moduleList = new List<string>(){
           "Para.Core.Modules.SystemModule.Win.dll",
           "Para.Model.Definition.dll",
           "Para.Modules.WorkflowLight.dll",
           "Para.Core.Modules.SpellCheckerModule.dll",
           "Para.Modules.App.dll",
           "Para.Modules.App.Win.dll",
           "Para.Modules.ABF.dll",
           "Para.Modules.BH.dll",
           "Para.Modules.RA.dll",
           "Para.Modules.FV.dll",
           "Para.Modules.Workflow.dll",
           "Para.Modules.Document.Activities.dll",
           "Para.Modules.Intern.Win.dll",
           "Para.Modules.MV.dll"
            };

            IList<Assembly> assemblies = new List<Assembly>();
            foreach (var file in moduleList)
                assemblies.Add(Assembly.LoadFile(ApplicationFolder + file));

            ControllersManager controllerManager = new ControllersManager();

            ApplicationModulesManager man = new ApplicationModulesManager(controllerManager, ApplicationFolder);
            man.AddModuleFromAssemblies(assemblies.Select(ass => ass.FullName).ToArray());

            var sec = new SecurityDummy();

            SAConnection conn = new SAConnection(DBConnectionString);
            conn.Open();

            XpoDefault.DataLayer = XpoDefault.GetDataLayer("XpoProvider=Asa;DataSourceName='paradat'", new ReflectionDictionary(), AutoCreateOption.None);

            XPObjectSpaceProvider provider = new XPObjectSpaceProvider(new ConnectionStringDataStoreProvider(XPOConnectionString));

            DesignerModelFactory dmf = new DesignerModelFactory();

            string ApplicationFileName = ApplicationFolder + exeName;
            string ApplicationConfigFileName = String.Format("{0}{1}.config", ApplicationFolder, exeName);

            var app = dmf.CreateApplicationByConfigFile(ApplicationConfigFileName, ApplicationFileName, ref ApplicationFolder);
            app.DatabaseVersionMismatch += app_DatabaseVersionMismatch;
            app.Setup("paraOffice", provider, man, sec);

            var dict = (app.CreateObjectSpace() as XPObjectSpace).Session.Dictionary;

            SchemaProvider schemaProvider = new SchemaProvider();
            foreach (var classinfo in dict.Classes.Cast<XPClassInfo>().Where(c => c.IsPersistent))
            {
                if (classinfo.Table.IsView)
                {
                    schemaProvider.Views.Add(new FastSchemaProvider.View() { Name = classinfo.Table.Name });
                    continue;
                }

                var table = new Table();
                table.ActualName = classinfo.Table.Name;

                foreach (var column in classinfo.Table.Columns)
                    table.Columns.Add(new Column() { ActualName = column.Name, DataType = column.ColumnType.ToSchemaDbType(), MaxLength = column.Size, IsPrimaryKeyColumn = column.IsKey, IsIdentity = column.IsIdentity });

                table.BuildPrimaryKey();

                foreach (var index in classinfo.Table.Indexes)
                {
                    var schemaIndex = new Index() { IndexName = index.Name, IndexType = index.IsUnique ? IndexTypes.Unqiue : IndexTypes.NonUnqiue };
                    int i = 1;
                    foreach (var col in index.Columns)
                    {
                        var indexCol = new IndexColumn();
                        indexCol.ColumnName = col;
                        indexCol.Sequence = i;
                        indexCol.Order = Ordering.Ascending;
                        i++;

                        schemaIndex.Columns.Add(indexCol);
                    }
                    table.Indizes.Add(schemaIndex);
                }

                foreach (var fk in classinfo.Table.ForeignKeys)
                {
                    var foreignKey = new ForeignKey();
                    foreignKey.Name = fk.Name;
                    foreignKey.Columns.AddRange(fk.Columns.Cast<string>());
                    foreignKey.DetailTable = classinfo.TableName;
                    foreignKey.MasterTable = fk.PrimaryKeyTable;
                    foreignKey.UniqueColumns.AddRange(fk.PrimaryKeyTableKeyColumns.Cast<string>());

                    table.ForeignKeys.Add(foreignKey);
                }

                schemaProvider.Tables.Add(table);
            }

            return schemaProvider;
        }

        void app_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e)
        {
            e.Handled = true;
        }
    }
}

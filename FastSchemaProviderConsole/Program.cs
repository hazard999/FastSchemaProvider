using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using iAnywhere.Data.SQLAnywhere;
using FastSchemaProvider;
using FastXAFApplicationReader;

namespace SQLAnywhereTest
{
    class Program
    {
        private static string SchemaSourceDSN = "paradat";
        private static string SchemaDestDSN = "SchemaDest";

        static void Main(string[] args)
        {
            var swOverall = new Stopwatch();

            swOverall.Start();

            DiffBetweenDataBaseAnProgramm();

            //ApplicationTest();
            //SchemaTest();

            swOverall.Stop();
            Console.WriteLine("Overall Time: " + swOverall.Elapsed);

            Console.ReadKey();
        }

        private static void SchemaTest()
        {
            SchemaProvider NewSchema = new SchemaProvider(new SAConnection("dsn=" + SchemaSourceDSN));

            SAConnection ConnectionToOldDatabase = new SAConnection("dsn=" + SchemaDestDSN);

            SchemaProvider OldSchema = new SchemaProvider(ConnectionToOldDatabase);

            new SchemaChanger(OldSchema, NewSchema, ConnectionToOldDatabase).Upgrade();
        }

        private static void ApplicationTest()
        {
            ApplicationReader reader = new ApplicationReader();

            var schema = reader.ReadApplication("paraOffice", @"C:\HG\ParaXtrem SBA\Output\", "paraOffice.exe", "XpoProvider=Asa;DataSourceName='paradat'", "dsn=" + SchemaSourceDSN);
        }

        private static void DiffBetweenDataBaseAnProgramm()
        {
            ApplicationReader reader = new ApplicationReader();

            var ApplicationSchema = reader.ReadApplication("paraOffice", @"C:\HG\ParaXtrem 1.7\Output\", "paraOffice.exe", "XpoProvider=Asa;DataSourceName='paradat'", "dsn=" + SchemaSourceDSN);
            SchemaProvider NewSchema = new SchemaProvider(new SAConnection("dsn=" + SchemaSourceDSN));

            SchemaComparer comparer = new SchemaComparer();
            var diff = comparer.CompareSchematas(NewSchema, ApplicationSchema, new SchemaComparerOptions() { OldSchemaTreatsViewsAsTable = true, OldSchemaTableAsViewPrefix = "V_" });

            Console.WriteLine("tables missin in database");
            foreach (var table in diff.TablesToDrop)
                Console.WriteLine(table.ActualName);

            Console.WriteLine("tables missin in application");
            foreach (var table in diff.TablesToCreate)
                Console.WriteLine(table.ActualName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using iAnywhere.Data.SQLAnywhere;
using FastSchemaProvider;

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

            SchemaProvider NewSchema = new SchemaProvider(new SAConnection("dsn=" + SchemaSourceDSN));
            
            SAConnection ConnectionToOldDatabase = new SAConnection("dsn=" + SchemaDestDSN);
            
            SchemaProvider OldSchema = new SchemaProvider(ConnectionToOldDatabase);

            new SchemaChanger(OldSchema, NewSchema, ConnectionToOldDatabase).Upgrade();

            swOverall.Stop();
            Console.WriteLine("Overall Time: " + swOverall.Elapsed);

            Console.ReadKey();
        }        
    }
}

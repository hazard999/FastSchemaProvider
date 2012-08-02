using System.Collections.Generic;
using System;
using System.Reflection;

namespace FastSchemaProvider
{   
    public static class DbTypeLookup
    {
        public static SchemaDbType? GetSADbType(string typeName)
        {
            SchemaDbType result;
            
            typeName = typeName.Replace(" ", string.Empty);

            if (!Enum.TryParse<SchemaDbType>(typeName, true, out result))
                return default(SchemaDbType);

            return result;
        }

        public static bool IsString(this SchemaDbType DbType)
        {
            if (DbType == SchemaDbType.Char)
                return true;

            if (DbType == SchemaDbType.VarChar)
                return true;

            if (DbType == SchemaDbType.NVarChar)
                return true;

            if (DbType == SchemaDbType.NChar)
                return true;

            if (DbType == SchemaDbType.Text)
                return true;

            if (DbType == SchemaDbType.NText)
                return true;

            return false;
        }

        public static string GetRealDbType(this Enum self)
        {
            FieldInfo fi = self.GetType().GetField(self.ToString());

            if (fi == null)
                return self.ToString();

            foreach (Attribute attr in Attribute.GetCustomAttributes(fi))
                if (attr.GetType() == typeof(RealDbTypeAttribute))
                    return ((RealDbTypeAttribute)attr).RealDbType;

            return fi.Name.ToLower();
        }
    }
}
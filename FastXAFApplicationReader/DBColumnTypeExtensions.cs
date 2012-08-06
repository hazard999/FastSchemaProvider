using System;
using FastSchemaProvider;
using DevExpress.Xpo.DB;

namespace FastXAFApplicationReader
{
    public static class DBColumnTypeExtensions
    {
        public static SchemaDbType ToSchemaDbType(this DBColumnType type)
        {
            switch(type)
            {
                case DBColumnType.Unknown:
                    return SchemaDbType.Char;
                case DBColumnType.Boolean:
                    return SchemaDbType.Bit;
                case DBColumnType.Byte:
                    return SchemaDbType.TinyInt;
                case DBColumnType.SByte:
                    return SchemaDbType.TinyInt;
                case DBColumnType.Char:
                    return SchemaDbType.Char;
                case DBColumnType.Decimal:
                    return SchemaDbType.Decimal;
                case DBColumnType.Double:
                    return SchemaDbType.Double;
                case DBColumnType.Single:
                    return SchemaDbType.Float;
                case DBColumnType.Int32:
                    return SchemaDbType.Integer;
                case DBColumnType.UInt32:
                    return SchemaDbType.UnsignedInt;
                case DBColumnType.Int16:
                    return SchemaDbType.SmallInt;
                case DBColumnType.UInt16:
                    return SchemaDbType.UnsignedSmallInt;
                case DBColumnType.Int64:
                    return SchemaDbType.BigInt;
                case DBColumnType.UInt64:
                    return SchemaDbType.UnsignedBigInt;
                case DBColumnType.String:
                    return SchemaDbType.VarChar;
                case DBColumnType.DateTime:
                    return SchemaDbType.DateTime;
                case DBColumnType.Guid:
                    return SchemaDbType.UniqueIdentifierStr;
                case DBColumnType.TimeSpan:
                    return SchemaDbType.DateTime;
                case DBColumnType.ByteArray:
                    return SchemaDbType.VarBinary;
            }

            return SchemaDbType.Char;
        }
    }
}

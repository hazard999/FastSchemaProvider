using System;

namespace FastSchemaProvider
{
    public enum SchemaDbType
    {
        BigInt = 1,
        Binary = 2,
        Bit = 3,
        Char = 4,
        Date = 5,
        DateTime = 6,
        Decimal = 7,
        Double = 8,
        Float = 9,
        Image = 10,
        Integer = 11,
        [RealDbType("long binary")]
        LongBinary = 12,
        [RealDbType("long nvarchar")]
        LongNVarchar = 13,
        [RealDbType("long varbit")]
        LongVarbit = 14,
        [RealDbType("long varchar")]
        LongVarchar = 15,
        Money = 16,
        NChar = 17,
        NText = 18,
        Numeric = 19,
        NVarChar = 20,
        Real = 21,
        SmallDateTime = 22,
        SmallInt = 23,
        SmallMoney = 24,
        SysName = 25,
        Text = 26,
        Time = 27,
        TimeStamp = 28,
        TinyInt = 29,
        UniqueIdentifier = 30,
        UniqueIdentifierStr = 31,
        UnsignedBigInt = 32,
        UnsignedInt = 33,
        UnsignedSmallInt = 34,
        VarBinary = 35,
        VarBit = 36,
        VarChar = 37,
        Xml = 38,
    }
}

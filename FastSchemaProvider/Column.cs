namespace FastSchemaProvider
{
    public class Column
    {
        public Column() { }

        public Column(string actualName, string tablename, bool isIdentity, SchemaDbType saDbType, int maxLength, int scale, bool nullAble, string defaultValue, bool PrimaryKeyColumn)
        {
            ActualName = actualName;
            DataType = saDbType;
            MaxLength = maxLength;
            Scale = scale;
            IsNullAble = nullAble;
            DefaultValue = defaultValue;
            IsIdentity = isIdentity;
            TableName = tablename;
            IsPrimaryKeyColumn = PrimaryKeyColumn;
        }

        public string ActualName { get; set; }

        public string TableName { get; set; }

        public bool IsIdentity { get; set; }

        public SchemaDbType DataType { get; set; }

        public bool IsBinary
        {
            get
            {
                return DataType == SchemaDbType.Binary || DataType == SchemaDbType.Image ||
                       DataType == SchemaDbType.VarBinary || DataType == SchemaDbType.LongBinary;
            }
        }

        public bool IsScaleAble
        {
            get
            {
                return DataType == SchemaDbType.Numeric;
            }
        }

        public bool IsNullAble { get; set; }

        public int MaxLength { get; set; }

        public int Scale { get; set; }

        public string DefaultValue { get; set; }

        public bool IsPrimaryKeyColumn { get; set; }

        public bool ColumnDefintionEquals(Column ToCompare)
        {
            if (ToCompare == null)
                return false;

            if (DataType != ToCompare.DataType)
                return false;

            if (IsNullAble != ToCompare.IsNullAble)
                return false;

            if (!IsBinary)
                if (MaxLength != ToCompare.MaxLength)
                    return false;

            if (Scale != ToCompare.Scale)
                return false;

            if (DefaultValue != ToCompare.DefaultValue)
                return false;

            return true;
        }
    }
}
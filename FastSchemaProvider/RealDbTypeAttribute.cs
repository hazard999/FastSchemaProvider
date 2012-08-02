using System;

namespace FastSchemaProvider
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class RealDbTypeAttribute : Attribute
    {
        private string _RealDbType;
        public string RealDbType
        {
            get { return _RealDbType; }
        }

        public RealDbTypeAttribute(string realDbType)
        {
            _RealDbType = realDbType;
        }
    }
}

using System;

namespace Infra.DataService.Protocol
{
    public class DeveloperError : Exception
    {
        public string ClassName { get; }
        public string MethodName { get; }
        public string Reason { get; }
        public string Fix { get; }

        public DeveloperError(string className = "", string methodName = "", 
            string reason = "", string fix = "")
            : base($"DeveloperError in {className}.{methodName}. {reason}. {fix}")
        {
            ClassName = className;
            MethodName = methodName;
            Reason = reason;
            Fix = fix;
        }
    }

    public class DataIntegrityError : Exception
    {
        public DataIntegrityError(string msg) : base(msg) { }
    }

    public class DataSymmetricityError : Exception
    {
        public DataSymmetricityError(string msg) : base(msg) { }
    }

    public interface IDataErrorGenerator
    {
        event Action<DataIntegrityError> DataIntegrityError;
        event Action<DataSymmetricityError> DataSymmetricityError;
    }
}

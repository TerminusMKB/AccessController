using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Errors
{
    public class DataErrorException : Exception
    {
        public DataErrorException() : base() { }
        public DataErrorException(string message) : base(message) { }
    }

    public class RequestErrorException : Exception
    {
        public RequestErrorException() : base() { }
        public RequestErrorException(string message) : base(message) { }
    }

    public class DataError {
        public String errorString;
        public String apiErrorType;

        public DataError setErrorString(String value) {
            errorString = value;
            return this;
        }
        public DataError setApiErrorType(String value) {
            apiErrorType = value;
            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDSync.core
{
    public class RuntimeError : Exception
    {
        public RuntimeError()
        {

        }


        public RuntimeError(int code, string message, string description = "")
        {
            this.code = code;
            this.message = message;
            this.description = description;
        }


        public override string ToString()
        {
            string str = $"code: {code}, msg: {message}\n\tdesc{description}";
            return base.ToString();
        }


        protected int code = -1;
        protected string message = "unknown error";
        protected string description = "";
    }

    public class ServiceAccountExhausted : RuntimeError
    {
        public ServiceAccountExhausted(string description = "")
        {
            this.code = 51;
            this.message = "service account exhausted";
            this.description = description;
        }
    }


    public class FileNotFound : RuntimeError
    {
        public FileNotFound(string description = "")
        {
            this.code = 52;
            this.message = "file not found";
            this.description = description;
        }
    }


    public class DriveOperationFail : RuntimeError
    {
        public DriveOperationFail(string description = "")
        {
            this.code = 53;
            this.message = "google drive operation fail";
            this.description = description;
        }
    }
}

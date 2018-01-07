using Newtonsoft.Json;
using System.Dynamic;

namespace HomeAutomation.Network.APIStatus
{
    public class ReturnStatus
    {
        public dynamic Object { get; set; }
        public ReturnStatus(int status)
        {
            Object = new ExpandoObject();
            Object.status = status;
        }
        public ReturnStatus()
        {
            Object = new ExpandoObject();
        }
        public ReturnStatus(CommonStatus status)
        {
            Object = new ExpandoObject();
            Object.status = status;
        }
        public ReturnStatus(CommonStatus status, string description)
        {
            Object = new ExpandoObject();
            Object.status = status;
            Object.description = description;
        }
        public string Json()
        {
            return JsonConvert.SerializeObject(Object);
        }
    }
    public enum CommonStatus
    {
        SUCCESS = 0,
        ERROR_NOT_FOUND = 404,
        ERROR_BAD_REQUEST = 400,
        ERROR_FORBIDDEN_REQUEST = 403,
        ERROR_NOT_IMPLEMENTED = 501
    }
}
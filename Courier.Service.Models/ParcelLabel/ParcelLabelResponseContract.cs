using System.Collections.Generic;

namespace Courier.Service.Models.ParcelLabel
{
    public class ParcelLabelResponseContract
    {
        public bool Success { get; set; }
        public string Message_Id { get; set; }
        public string Consignment_Id { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}

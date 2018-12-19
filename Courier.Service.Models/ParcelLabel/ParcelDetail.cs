namespace Courier.Service.Models.ParcelLabel
{
    public class ParcelDetail
    {
        public string Service_Code { get; set; }
        public string Return_Indicator { get; set; }
        public Dimensions Dimensions { get; set; }

        public ParcelDetail(string serviceCode)
        {
            Service_Code = serviceCode;
            Return_Indicator = "OUTBOUND";
            Dimensions = new Dimensions(15, 45, 35, 3);
        }
    }

    public class Dimensions
    {
        public int Length_Cm { get; set; }
        public int Width_Cm { get; set; }
        public int Height_Cm { get; set; }
        public int Weight_Kg { get; set; }

        public Dimensions(int length, int width, int height, int weight)
        {
            Length_Cm = length;
            Width_Cm = width;
            Height_Cm = height;
            Weight_Kg = weight;
        }
    }
}

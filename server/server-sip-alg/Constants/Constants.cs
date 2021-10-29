
namespace server_sip_alg
{
    public static class Constants
    {

        public const string PRODUCT_NAME = "test";

        public const int PORT_NUMBER = 5060;
        public const int NUM_MAX_CONNECTE = 150;

        //Request string
        public const int SIZE_MIN_PACKAGES_REQUEST = 640;
        public const int SIZE_FIRST_LINE_REQUEST = 33;

        public const int SIZE_READ_BUFFER = 2048;
        public const int SIZE_APLICE_BODY = 60;

        //Guard size package 
        public const int SIZE_REQUEST_MIN = 25;

        //Error
        public const int ERROR_NUM_UNEXPECTED = 10004;

        //Transport
        public const string UDP_TRANSPORT = "UDP";
        public const string TCP_TRANSPORT = "TCP";
    }
}

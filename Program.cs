using chachacha_server.HTTP;
using System.Text;

namespace chachacha_server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            HTTPProcessor processor = new HTTPProcessor("http://*:80/");
            processor.StartListening();
        }
    }
}

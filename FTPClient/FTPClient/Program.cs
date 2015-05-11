using System;
namespace FTPClient
{
    public struct Complex
    {
        public int real;
        public int imaginary;

        public Complex(int real, int imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }
        public static Complex operator +(Complex cc){
            return new Complex(cc.imaginary + 1,cc.real +1);
        }

        // Declare which operator to overload (+), the types 
        // that can be added (two Complex objects), and the 
        // return type (Complex):
        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.real + c2.real, c1.imaginary + c2.imaginary);
        }
        // Override the ToString method to display an complex number in the suitable format:
        public override string ToString()
        {
            return (String.Format("{0} + {1}i", real, imaginary));
        }

        public static void Main2()
        {
            Complex num1 = new Complex(2, 3);
            Complex num2 = new Complex(3, 4);

            // Add two Complex objects (num1 and num2) through the
            // overloaded plus operator:
            Complex sum = num1 + num2;

            // Print the numbers and the sum using the overriden ToString method:
            Console.WriteLine("First complex number:  {0}", num1);
            Console.WriteLine("Second complex number: {0}", num2);
            Console.WriteLine("The sum of the two numbers: {0}", sum);

        }
    }

    class Program
    {

        static void Main(string[] args)
        {

            //string filename1 = "bunny_20140725_12345678.jpg";
            //string filename2 = @"r5.jpg";
            //string destPath = @"D:\FTP\";

            //FTP
            //FtpClient client1 = new FtpClient();

            //client1.Open("10.27.68.155", 21);
            //client1.Logon("icftp", "icftp");
            //string[] list1 = client1.GetDirectoryList();


            //client1.GetFile(filename1, client1.currentLocalPath + filename1);
            //client1.SendFile(filename2, client1.currentLocalPath);
            //client1.GetFile(filename2, destPath);
            //client1.DeleteFile(filename2, false);
            //client1.Close();

            //Proxy + FTP
            //FtpClient client2 = new FtpClient("10.27.68.155", "8021");
            
            //client2.Open("10.27.68.155", 21);
            //client2.Logon("icftpproxy","bankpro","icftp", "icftp");
            //string[] list2 = client2.GetDirectoryList();
            //client2.GetFile(filename1, client2.currentLocalPath + filename1);
            //client2.SendFile(filename2, client2.currentLocalPath);
            //client2.GetFile(filename2, destPath);
            //client2.DeleteFile(filename2, false);
            //client2.Close();
        }
    }
}

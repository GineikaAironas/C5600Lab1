using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C5600Lab1
{
    class Program
    {
        static void Main(string[] args)
        {

            removeTest();
            return;
            String fileName = $@"data.dat";
            // 2 * sizeof(int) + sizeof(double) + 20 * sizeof(byte)  //Size of obj
            Console.WriteLine("a");
            using (var fs = new FileStream("Array.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                Console.WriteLine("B");
                var list = new DMListFile<int>(
                          fs,
                          (byte[] a) =>
                          {
                              return BitConverter.ToInt32(a, 0);
                          },
                          (int o) =>
                          {
                              return BitConverter.GetBytes(o);
                          },
                          4
                );
                Console.WriteLine("C");
                list.Add(5);
                Console.WriteLine("D");
                list.Add(1);
                list.Add(30);
                list.Add(69);
                list.Add(420);
                list.Add(1111);
                list.Add(456456);
                list.Add(333333);
                list.Add(24);
                for(int i = 0; i < 150; i++)
                {
                    list.Add(new Random().Next(-100, 1000));
                }
                Console.Read();
            }

            return;

            int n = 10;
            var mas = new DMArray<string>(n);
            //var rnd = new Random();
            var rnd = new Random();

            for (int i = 0; i < n; i++)
            {
                mas[i] = SymbolSequence(rnd.Next(0, 10), rnd);
            }
            Console.WriteLine("=========Generated values");
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(mas[i]);
            }
            using (var fs = new FileStream("Array.bin", FileMode.Create, FileAccess.ReadWrite))
            {
                var masf = new DMArrayFile<string>(
                      fs,
                      (byte[] a) =>
                      {
                          int length = BitConverter.ToInt32(a, 0);
                          return Encoding.UTF8.GetString(a.Skip(sizeof(int)).Take(length).ToArray());
                      },
                      (string s) =>
                      {
                          var data = Encoding.UTF8.GetBytes(s);
                          var fulldata = new byte[data.Length + sizeof(int)];
                          Array.Copy(BitConverter.GetBytes(data.Length), fulldata, sizeof(int));
                          Array.Copy(data, 0, fulldata, sizeof(int), data.Length);
                          return fulldata;
                      }, 10,
                          48
                      );
               
                for (int i = 0; i < n; i++)
                {
                    masf[i] = mas[i];
                }
                Console.WriteLine("=========Reading from file");
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine(masf[i]);
                }
                
                masf[1] = "BOSAS";
                masf[2] = "TEVAS";
               
                masf.Swap(0, 9);
               
                fs.Close();
            }
            using (var fs = new FileStream("Array.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var masf = new DMArrayFile<string>(
                          fs,
                          (byte[] a) => 
                          {
                              int length = BitConverter.ToInt32(a, 0);
                              return Encoding.UTF8.GetString(a.Skip(sizeof(int)).Take(length).ToArray());
                          },
                          (string s) =>
                          {
                              var data = Encoding.UTF8.GetBytes(s);
                              var fulldata = new byte[data.Length + sizeof(int)];
                              Array.Copy(BitConverter.GetBytes(data.Length), fulldata, sizeof(int));
                              Array.Copy(data, 0, fulldata, sizeof(int), data.Length);

                              return fulldata;
                          }
                );
                
                Console.WriteLine("=========Reading again from file (new DMArrayFile object)");
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine(masf[i]);
                }
               

                masf.BubleSort();
                
                Console.WriteLine("=========masf=sorted");
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine(masf[i]);
                }
                
                fs.Close();
            }

            var mas2 = new DMArray<int>(n);
            for (int i = 0; i < n; i++)
            {
                mas2[i] = (int)(100 * rnd.NextDouble());
            }
            Console.WriteLine("=========mas2");
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(mas2[i]);
            }
            mas2.BubleSort();
            Console.WriteLine("=========mas2=sorted");
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(mas2[i]);
            }
            Console.Read();
        }

        private static void removeTest()
        {
            using (var fs = new FileStream("rm.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                Console.WriteLine("B");
                var list = new DMListFile<int>(
                          fs,
                          (byte[] a) =>
                          {
                              return BitConverter.ToInt32(a, 0);
                          },
                          (int o) =>
                          {
                              return BitConverter.GetBytes(o);
                          },
                          4
                );
                Console.WriteLine("C");
                list.Add(5);
                Console.WriteLine("D");
                list.Add(1);
                list.Add(30);
                list.Add(69);
                list.Add(420);
                list.Add(1111);
                list.Add(456456);
                list.Add(333333);
                list.Add(24);
                list.Remove(2);
                list.Remove(0);
                list.Remove(list.Length-1);
                list.Add(666666);
                Console.Read();
            }

        }

        private static void lltest()
        {
            using (var fs = new FileStream("Array.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var list = new DMListFile<int>(
                          fs,
                          (byte[] a) =>
                          {
                              return BitConverter.ToInt32(a,0);
                          },
                          (int o) =>
                          {
                              return BitConverter.GetBytes(o);
                          },
                          4
                );
                list.Add(5);
                list.Add(1);
                list.Add(30);
                list.Add(24);
                Console.WriteLine(list[0]);
                Console.WriteLine(list[1]);
                Console.WriteLine(list[2]);
                Console.WriteLine(list[3]);
            }
        }
        static string SymbolSequence(int n, Random rnd)
        {
            const string str = "abcdefghijklmnoprst";
            string hlp = str[rnd.Next(0, str.Length)].ToString().ToUpper();
            for (int i = 0; i < n - 1; i++)
            {
                hlp += str[rnd.Next(0, n - 1)];
            }
            return hlp;
        }
    }
}

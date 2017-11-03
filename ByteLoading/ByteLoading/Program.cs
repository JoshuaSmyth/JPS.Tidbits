using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;


namespace ByteLoading
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct MyData
    {
        public const Int32 SIZE = 16;

        public Int32 FieldOne;

        public Int32 FieldTwo;

        public Int32 FieldThree;

        public Int32 FieldFour;

        public static bool operator== (MyData obj1, MyData obj2)
        {
            return (obj1.FieldOne == obj2.FieldOne 
                        && obj1.FieldTwo == obj2.FieldTwo 
                        && obj1.FieldThree == obj2.FieldThree
                        && obj1.FieldFour == obj2.FieldFour);
        }

        public static bool operator!= (MyData obj1, MyData obj2)
        {
            return !(obj1.FieldOne == obj2.FieldOne 
                        && obj1.FieldTwo == obj2.FieldTwo 
                        && obj1.FieldThree == obj2.FieldThree
                        && obj1.FieldFour == obj2.FieldFour);
        }

        public static bool operator== (MyData* obj1, MyData obj2)
        {
            return (obj1->FieldOne == obj2.FieldOne 
                        && obj1->FieldTwo == obj2.FieldTwo 
                        && obj1->FieldThree == obj2.FieldThree
                        && obj1->FieldFour == obj2.FieldFour);
        }

        public static bool operator!= (MyData* obj1, MyData obj2)
        {
            return (obj1->FieldOne != obj2.FieldOne 
                        && obj1->FieldTwo != obj2.FieldTwo 
                        && obj1->FieldThree != obj2.FieldThree
                        && obj1->FieldFour != obj2.FieldFour);
        }
    }

    class MyDataFixedCollection
    {
        readonly byte[] m_MyData;
        public MyDataFixedCollection(byte[] myData) {
            m_MyData = myData;
        }

        public unsafe MyData* GetMyData(int i) {
            var index = i * MyData.SIZE;
            fixed(byte* p = &m_MyData[index])
            {
                return (MyData*)p;
            }
        }
        public Int32 Length {
            get {
                return m_MyData.Length / MyData.SIZE;
            }
        }

        public unsafe MyData* this[int index]
        {  
            get {
                var ix = index * MyData.SIZE;
                fixed(byte* p = &m_MyData[ix])
                {
                    return (MyData*)p;
                }
            }
        }  
    }

    class Program
    {
        static unsafe void Main(string[] args)
        {
            const int length = 10000000;
            var data = GetRandomData(length); // 1 Million
            
            var d1 = UsingBinaryWriter(data);
            var d2 = UsingMarshalWriter(data);

            // Compare
            {
                var correct = true;
                for(int i=0;i<d1.Length;i++)
                {
                    if (d1[i] != d2[i])
                    {
                        correct = false;
                        break;
                    }
                }

                if (correct == false)
                {
                    Console.WriteLine("Serial Forms Don't Agree");
                }
            }

            // Reading
            {
                {
                    var d3 = ReadBytes(length,d1);
                    var correct = true;
                    for(int i=0;i<data.Length;i++)
                    {
                        
                        if (d3[i] != data[i])
                        {
                            correct = false;
                            break;
                        }
                    }
                    if (correct == false)
                    {
                        Console.WriteLine("Serial Forms Don't Agree");
                    }
                }

                {
                    var d4 = ReadByteCollection(length, d1);
                    var correct = true;
                    
                    for(int i=0;i<d4.Length;i++)
                    {
                        if (d4[i] != data[i])
                        {
                            correct = false;
                            break;
                        }
                    }
                    if (correct == false)
                    {
                        Console.WriteLine("Serial Forms Don't Agree");
                    }
                }
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        
        private static byte[] UsingMarshalWriter(MyData[] data) {
            var sw = new Stopwatch();
            sw.Start();
            var bytes = WriteDataMarshalCopy<MyData>(data);

            sw.Stop();
            Console.WriteLine("ByteCount: " + bytes.Length);
            Console.WriteLine("Time Take: " + sw.ElapsedMilliseconds + "ms");

            return bytes;
        }

        private static byte[] UsingBinaryWriter(MyData[] data) {
            Console.WriteLine("Using Binary Writer");
            var sw = new Stopwatch();
            sw.Start();
            var bytes = WriteData(data);

            sw.Stop();
            Console.WriteLine("ByteCount: " + bytes.Length);
            Console.WriteLine("Time Take: " + sw.ElapsedMilliseconds + "ms");

            return bytes;
        }

        static MyDataFixedCollection ReadByteCollection(Int32 length, byte[] src) {
            Console.WriteLine("Using Fixed Collection");
            var sw = new Stopwatch();
            sw.Start();

          
            Console.WriteLine("Time Take: " + sw.ElapsedMilliseconds + "ms");

            var rv = new MyDataFixedCollection(src);
              sw.Stop();

            return rv;
        }

        static MyData[] ReadBytes(Int32 length, byte[] src) {
            Console.WriteLine("Using Binary Reader");
            var sw = new Stopwatch();
            sw.Start();
            var rv = new MyData[length];
            using(var ms = new MemoryStream((src))) {
                using(var br = new BinaryReader(ms))
                {
                    for(int i=0;i<length;i++) {
                     rv[i].FieldOne = br.ReadInt32();
                     rv[i].FieldTwo = br.ReadInt32();
                     rv[i].FieldThree = br.ReadInt32();
                     rv[i].FieldFour = br.ReadInt32();
                    }
                }
            }
            sw.Stop();
            Console.WriteLine("Time Take: " + sw.ElapsedMilliseconds + "ms");

            return rv;
        }

        static Byte[]  WriteDataMarshalCopy<T>(T[] src) {

            var sizeOfData = Marshal.SizeOf(typeof(T));
            var length = sizeOfData * src.Length;
            var bytes = new byte[length];

            var handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(src, GCHandleType.Pinned);
                var ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        static Byte[]  WriteDataMarshalCopy(MyData[] src) {

            var sizeOfData = Marshal.SizeOf(typeof(MyData));
            var length = sizeOfData * src.Length;
            var bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(src, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }

        static Byte[] WriteData(MyData[] data) {
            
            using(var ms = new MemoryStream())
            {
                using(var bw = new BinaryWriter(ms))
                {
                    for(int i=0;i<data.Length;i++)
                    {
                        bw.Write(data[i].FieldOne);
                        bw.Write(data[i].FieldTwo);
                        bw.Write(data[i].FieldThree);
                        bw.Write(data[i].FieldFour);
                    }
                }
                return ms.ToArray();
            }
        }

        static MyData[] GetRandomData(Int32 length) {
            var r = new Random(42);
            var data = new MyData[length];
            for(int i=0;i<length;i++) {
                data[i].FieldOne = r.Next(0,Int32.MaxValue);
                data[i].FieldTwo = r.Next(0,Int32.MaxValue);
                data[i].FieldThree = r.Next(0,Int32.MaxValue);
                data[i].FieldFour = r.Next(0,Int32.MaxValue);
            }
            return data;
        }
    }
}

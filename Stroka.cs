using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ШаблонРедактора
{
    public class Stroka
    {
        public int Modul;
        public byte Msg;
        public int Int1;
        public int Int2;
        public byte Day;
        public byte Hour;
        public byte Min;
        public byte Sec;
        public int Sotr;
        public Stroka() { }
        public Stroka(int in1, int in2, byte dy)
        {
            Int1 = in1;
            Int2 = in2;
            Day = dy;
        }
    }
}

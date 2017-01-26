using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ШаблонРедактора;

class Editor
{
    //Константы программы
    public const string ProgramName = "BioCrack";
    public const string ProgramVersion = "0.4 - 09 октября 2013 года";
    public const string ProgramAutor = "Гордеев Сергей";
    public const string FileUnnamed = "Безымянный";
    public const string FileType = "Журнал Biosmart (*.bsm)|*.bsm|Все файлы (*.*)|*.*";
    static string ParametersFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SG\\BioCrack"; //Папка с конфигурацией программы
    static string ParametersFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\SG\\BioCrack\\config.cfg"; //Файл конфигурации программы
    //Параметры программы
    static public List<Sotrudnik> Sotrudniki = new List<Sotrudnik>();
    //Инициализация параметров программы
    public static void init()
    {
        try
        {
            //Пробуем загрузить настройки, если они были сохранены
            BinaryReader file = new BinaryReader(new FileStream(ParametersFile, FileMode.Open));
            WindowsPosirion.X = file.ReadInt32();
            WindowsPosirion.Y = file.ReadInt32();
            WindowsPosirion.Width = file.ReadInt32();
            WindowsPosirion.Heidht = file.ReadInt32();
            WindowsPosirion.Max = file.ReadBoolean();
            //Загружаем список сотрудников
            int s = file.ReadInt32();
            for (int i = 0; i < s; i++)
                Sotrudniki.Add(new Sotrudnik(file.ReadInt32(), file.ReadString()));
            file.Close();
        } catch { }
    }
    //Сохранение параметров программы
    public static void saveconfig()
    {
        try
        {
            Directory.CreateDirectory(ParametersFolder);
            BinaryWriter file = new BinaryWriter(new FileStream(ParametersFile, FileMode.Create));
            file.Write(WindowsPosirion.X);
            file.Write(WindowsPosirion.Y);
            file.Write(WindowsPosirion.Width);
            file.Write(WindowsPosirion.Heidht);
            file.Write(WindowsPosirion.Max);
            //Сохраняем список сотрудников
            file.Write(Sotrudniki.Count);
            foreach (Sotrudnik sotr in Sotrudniki)
            {
                file.Write(sotr.ID);
                file.Write(sotr.Name);
            }
            file.Close();
        }        catch { }
    }
    //Злобное сообщение об ошибке
    public static void Error(string message)
    {
        MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    //Не злобное сообщение
    public static void Message(string message)
    {
        MessageBox.Show(message, ProgramName);
    }
}

public class WindowsPosirion
{
    static public int X = 100;
    static public int Y = 100;
    static public int Width = 800;
    static public int Heidht = 800;
    static public bool Max = false;
}
//Класс проекта
public class Project
{
    public static string FileName;
    public static string EditName;
    public static bool Changed;
    //Данные проекта
    public List<Stroka> Stroks;
    //Копирование объекта
    public void Copy(Project Copy)
    {
        //Text = Copy.Text;
    }
    //Новый проект
    public void NewProject()
    {
        FileName = "";
        EditName = Editor.FileUnnamed;
        Changed = false;
        //Создание нового документа
        //stroka
    }
    //Сохранение проекта
    public bool Save()
    {
        try
        {
            bool[] Rec = new bool[Stroks.Count]; //Создаём массив "фалсов" чтоб знать, какие строки были записаны
            BinaryWriter file = new BinaryWriter(new FileStream(FileName, FileMode.Create));
            //for (int i = 0; i < Rec.Count(); i++) Rec[i] = false; //Показалось, или ошибка была из-за этого
            for (int j = 0; j < Stroks.Count; j++)
            {
                int min = 100000;
                int index = 0;
                for (int i = 0; i < Stroks.Count; i++)
                {
                    if (!Rec[i] & min>TimeToInt(Stroks[i].Hour,Stroks[i].Min,Stroks[i].Sec))
                    {
                        min = TimeToInt(Stroks[i].Hour, Stroks[i].Min, Stroks[i].Sec);
                        index = i;
                        
                    }
                }
                Rec[index] = true; //Или из-за этого, так как эта штука работала в цикле
                byte b = 0;
                //ID Модуля
                file.Write(Stroks[index].Modul);
                //Событие
                file.Write(Stroks[index].Msg);
                //Непонятная херня
                file.Write(Stroks[index].Int1);
                file.Write(Stroks[index].Int2);
                //День
                file.Write(Stroks[index].Day);
                file.Write(b);
                //Час                
                file.Write(Stroks[index].Hour);
                file.Write(b);
                //Минута
                file.Write(Stroks[index].Min);
                file.Write(b);
                //Секунда
                file.Write(Stroks[index].Sec);
                file.Write(b);
                //Три неиспользуемых байта
                file.Write(b);
                file.Write(b);
                //ID сотрудника
                file.Write(Stroks[index].Sotr);
            }
            file.Close();
            return true;
        }
        catch
        {
            Editor.Error("Произошла ошибка во время сохранения файла. Файл не сохранён.");
            return false;
        }
    }
    int TimeToInt(byte h, byte m, byte s) { return h * 3600 + m * 60 + s; }
    //Загрузка проекта
    public bool Open()
    {
        try
        {
            BinaryReader file = new BinaryReader(new FileStream(FileName, FileMode.Open));
            Stroks = new List<Stroka>();
            while (file.BaseStream.Position<file.BaseStream.Length)
            {
                Stroka strk = new Stroka();
                //ID Модуля
                strk.Modul = file.ReadInt32();
                //Событие
                strk.Msg = file.ReadByte();
                //Непонятная херня
                strk.Int1 = file.ReadInt32();
                strk.Int2 = file.ReadInt32();
                //День
                strk.Day = file.ReadByte();
                file.ReadByte();
                //Час                
                strk.Hour = file.ReadByte();
                file.ReadByte();
                //Минута
                strk.Min = file.ReadByte();
                file.ReadByte();
                //Секунда
                strk.Sec = file.ReadByte();
                file.ReadByte();
                //Три неиспользуемых байта
                file.ReadByte();
                file.ReadByte();
                //ID сотрудника
                strk.Sotr = file.ReadInt32();
                Stroks.Add(strk);

            }
            file.Close();
            return true;
        }
        catch
        {
            Editor.Error("Произошла ошибка при открытии файла.");
            return false;
        }
    }

}
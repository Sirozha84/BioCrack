using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ШаблонРедактора
{
    public partial class FormMain : Form
    {
        Project CurrentProject = new Project();
        List<Project> History = new List<Project>();
        int HistoryNumber;
        bool ProgramTextChange = true;
        System.Diagnostics.Process Help = new System.Diagnostics.Process();
        
        bool Filtered = true;
        List<int> Finded = new List<int>();


        public FormMain()
        {
            InitializeComponent();
            Editor.init();
            Left = WindowsPosirion.X;
            Top = WindowsPosirion.Y;
            Width = WindowsPosirion.Width;
            Height = WindowsPosirion.Heidht;
            if (WindowsPosirion.Max) WindowState = FormWindowState.Maximized; else WindowState = FormWindowState.Normal;
            menunew_Click(null, null);
            DrawSotrudniki();
        }
        //Создание нового файла
        private void menunew_Click(object sender, EventArgs e)
        {
            if (!SaveQuestion()) return;
            CurrentProject.NewProject();
            DrawDocument();
            ResetHistory();
            Change(true);
        }
        //Открытие файла
        void OpenFile()
        {
            if (!SaveQuestion()) return;
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = Editor.FileType;
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            Project.FileName = openFileDialog1.FileName;
            if (!CurrentProject.Open()) return;
            Project.EditName = openFileDialog1.FileName;
            DrawDocument();
            ResetHistory();
            Change(true);

            comboBox1.Items.Clear();
            List<String> kol = new List<string>();
            foreach (Stroka str in CurrentProject.Stroks)
            {
                if (!kol.Contains(str.Modul.ToString()))
                {
                    kol.Add(str.Modul.ToString());
                    comboBox1.Items.Add(str.Modul.ToString());
                }
            }
            /////////////////////////////////// Потом сделать добавление в комбобокс с сортировкой //////////////////////////////////////////////
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Вход");
            comboBox2.Items.Add("Выход");
            comboBox2.Items.Add("Ид. успешна");
            comboBox2.Items.Add("Ид. успешна (по карте)");
            toolsave.Enabled = true;
            menusave.Enabled = true;
            menusaveas.Enabled = true;

        }
        //Сохранение файла
        bool FileSave()
        {
            if (Project.EditName == Editor.FileUnnamed && !FileSaveAs()) return false;
            if (!CurrentProject.Save()) return false;
            Change(true);
            return true;
        }
        //Сохранение файла как
        bool FileSaveAs()
        {
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = Editor.FileType;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
            { 
                Project.FileName = saveFileDialog1.FileName;
                Project.EditName = saveFileDialog1.FileName; 
                Change(true); return true; 
            }
            return false;
        }
        //Отмена
        private void menuundo_Click(object sender, EventArgs e)
        {
            if (HistoryNumber < 2) return;
            HistoryNumber--;
            CurrentProject.Copy(History[HistoryNumber - 1]);
            DrawDocument();
        }
        //Возврат
        private void menuredo_Click(object sender, EventArgs e)
        {
            if (HistoryNumber == History.Count) return;
            HistoryNumber++;
            CurrentProject.Copy(History[HistoryNumber - 1]);
            DrawDocument();
        }
        //Вызов справки
        private void menuhelp_Click(object sender, EventArgs e)
        {
            try { HelpClose(); Help.StartInfo.FileName = "help.chm"; Help.Start(); }
            catch { Editor.Error("Файл справки не найден."); } 
        }
        //Закрытие программы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!SaveQuestion()) e.Cancel = true;
            if (WindowState == FormWindowState.Maximized) WindowsPosirion.Max = true;
            else WindowsPosirion.Max = false;
            Editor.saveconfig();
            HelpClose();
        }
        //Рисование документа
        void DrawDocument()
        {

            ProgramTextChange = true;
            if (CurrentProject.Stroks == null) return;

            listView1.BeginUpdate();
            listView1.Items.Clear();
            Finded.Clear();
            int s = 0;
            foreach (Stroka strk in CurrentProject.Stroks)
            {
                bool OK = false;
                if (Filtered)
                {
                    foreach (Sotrudnik sotr in Editor.Sotrudniki)
                        if (strk.Sotr == sotr.ID) OK = true;
                    //if (CurrentProject.Stroks[i].Sotr == Editor.Sotrudniki[listBox1.SelectedIndex].ID)
                    //OK = true;
                }
                else
                    OK = true;
                if (OK)
                {
                    listView1.Items.Add(strk.Modul.ToString());
                    //listView1.Items[listView1.Items.Count - 1].SubItems.Add(strk.Day.ToString());
                    listView1.Items[listView1.Items.Count - 1].SubItems.Add(MessageByCode(strk.Msg));
                    listView1.Items[listView1.Items.Count - 1].SubItems.Add(strk.Hour.ToString() + ":" + strk.Min.ToString("00") + ":" + strk.Sec.ToString("00"));
                    
                    bool nash = false;
                    foreach (Sotrudnik sotr in Editor.Sotrudniki)
                        if (!nash & sotr.ID == strk.Sotr)
                        {
                            listView1.Items[listView1.Items.Count - 1].SubItems.Add(sotr.Name);
                            nash = true;
                        }
                    listView1.Items[listView1.Items.Count - 1].SubItems.Add(strk.Sotr.ToString());
                    Finded.Add(s);
                }
                s++;
            }
            listView1.EndUpdate();
            label5.Text = "---";
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
        }
        //Возврат сотрудника по коду
        string SotrByCode(int id)
        {
            foreach (Sotrudnik sotr in Editor.Sotrudniki)
                if (sotr.ID == id) return sotr.Name;
            return id.ToString();
        }
        //Возврат события по коду
        string MessageByCode(int c)
        {
            switch (c)
            {
                case 1: return "Контроллер не отвечает";
                case 2: return "Сотрудник уволен";
                case 39: return "Идентификация неудачна (по карте)";
                case 40: return "Идентификация успешна (по карте)";
                case 55: return "Идентификация успешна";
                case 56: return "Идентификация неудачна";
                case 106: return "Перезапуск системы";
                case 114: return "Связь с сервером прервана";
                case 115: return "Связь с сервером восстановлена";
                case 151: return "Вход";
                case 152: return "Выход";
                case 225: return "Отсказ доступа (Anti-Pass-Back)";
                default: return c.ToString();
            }
        }
        //Рисование списка сотрудников
        void DrawSotrudniki()
        {
            listBox1.Items.Clear();
            foreach (Sotrudnik sotr in Editor.Sotrudniki)
                listBox1.Items.Add(sotr.ID.ToString() + " - " + sotr.Name);
        }
        //Рисование имени файла и программы
        void SetFormText()
        {
            string star = ""; 
            if (Project.Changed) star = "*";
            Text = System.IO.Path.GetFileNameWithoutExtension(Project.EditName) + star + " - " + Editor.ProgramName; 
        }
        //Регистрация изменений
        void Change(bool Reset) 
        { 
            if (Reset) Project.Changed = false; 
            else Project.Changed = true; 
            SetFormText();
            CreateUndo(); 
        }
        //Создание отмены
        void CreateUndo()
        {
            while (HistoryNumber < History.Count) History.RemoveAt(History.Count - 1);
            Project Copy = new Project();
            Copy.Copy(CurrentProject);
            History.Add(Copy);
            HistoryNumber++;
        }
        //регистрация изменений окна
        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            WindowsPosirion.X = Left;
            WindowsPosirion.Y = Top;
            WindowsPosirion.Width = Width;
            WindowsPosirion.Heidht = Height;
            WindowsPosirion.Max = false;
        }
        //Вопрос перед уничтожением проекта
        public bool SaveQuestion()
        {
            if (!Project.Changed) return true;
            switch (MessageBox.Show("Сохранить изменения в файле \"" + System.IO.Path.GetFileNameWithoutExtension(Project.EditName) + "\"?", "Файл изменён",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
            {
                case DialogResult.Yes: return FileSave();
                case DialogResult.No: return true;
                case DialogResult.Cancel: return false;
            }
            return false;
        }
        //Изменение в тексте (Главное что надо будет делать при изменении своих проектов, это регистрировать их с помощью Change(false);
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (ProgramTextChange) { ProgramTextChange = false; return; } //Наверное не понадобится
            //CurrentProject.Text = textBox1.Text; //Это тоже вряд ли понадобится
            Change(false);
        }
        //Прочие процедурки
        void ResetHistory() { History.Clear(); HistoryNumber = 0; }
        void HelpClose() { try { Help.Kill(); } catch { } }
        //Меню и панель инструментов
        private void menusave_Click(object sender, EventArgs e) { FileSave(); }
        private void menusaveas_Click(object sender, EventArgs e) { if (FileSaveAs()) FileSave(); }
        private void menuexit_Click(object sender, EventArgs e) { this.Close(); }
        private void menuabout_Click(object sender, EventArgs e) { FormAbout form = new FormAbout(); form.ShowDialog(); }
        private void toolnew_Click(object sender, EventArgs e) { menunew_Click(null, null); }
        private void toolopen_Click(object sender, EventArgs e) { menuopen_Click(null, null); }
        private void toolsave_Click(object sender, EventArgs e) { menusave_Click(null, null); }
        private void toolundo_Click(object sender, EventArgs e) { menuundo_Click(null, null); }
        private void toolredo_Click(object sender, EventArgs e) { menuredo_Click(null, null); }
        private void toolStripStatusLabel1_Click(object sender, EventArgs e) { System.Diagnostics.Process.Start("http://www.sg-software.ru"); }
        private void menuopen_Click(object sender, EventArgs e) { OpenFile(); }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Editor.Sotrudniki.Add(new Sotrudnik(Convert.ToInt32(textBox1.Text), textBox2.Text));
                textBox1.Text = "";
                textBox2.Text = "";
                DrawSotrudniki();
            }
            catch { }
        }
        //Выбор сотрудника
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            if (listBox1.SelectedIndex < 0) return; //Если хоть что-то выбрано, активируем кнопку для удаления
            button2.Enabled = true;
            if (comboBox1.Items.Count == 0) return; //Дальше ничего не делаем, если не загружен лог
            label5.Text = Editor.Sotrudniki[listBox1.SelectedIndex].Name;
            if (comboBox1.SelectedIndex < 0) comboBox1.Text = comboBox1.Items[0].ToString();
            if (comboBox2.SelectedIndex < 0) comboBox2.Text = "Вход";
            textBox3.Text = "0";
            textBox4.Text = "0";
            textBox5.Text = "0";
            button5.Enabled = true;
        }
        //Удаление сотрудника из списка
        private void button2_Click(object sender, EventArgs e)
        {
            Editor.Sotrudniki.RemoveAt(listBox1.SelectedIndex);
            DrawSotrudniki();
            button2.Enabled = false;
            button5.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Filtered = !Filtered;
            if (Filtered) button3.Text = "Показать всех";
            else button3.Text = "Показать только тех кто в этом списке";
            DrawDocument();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0) return;
            if (CurrentProject.Stroks.Count == 0)
            {
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                return;
            }
            label5.Text = SotrByCode(CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Sotr);
            comboBox1.Text = MessageByCode(CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Modul);
            switch (CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Msg)
            {
                case 40: comboBox2.Text = "Ид. успешна (по карте)"; break;
                case 55: comboBox2.Text = "Ид. успешна"; break;
                case 151: comboBox2.Text = "Вход"; break;
                case 152: comboBox2.Text = "Выход"; break;
            }
            comboBox2.Text = MessageByCode(CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Msg);
            textBox3.Text = CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Hour.ToString();
            textBox4.Text = CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Min.ToString();
            textBox5.Text = CurrentProject.Stroks[Finded[listView1.SelectedIndices[0]]].Sec.ToString();
            button4.Enabled = true;
            button5.Enabled = false;
            button6.Enabled = true;
        }
        //Сохранение изменений
        private void button4_Click(object sender, EventArgs e)
        {
            SaveStroka(Finded[listView1.SelectedIndices[0]]);
        }
        //Сохранение данных в строку
        void SaveStroka(int index)
        {
            //Меняем контроллер
            CurrentProject.Stroks[index].Modul = Convert.ToInt32(comboBox1.Text);
            //Меняем событие
            if (comboBox2.SelectedIndex == 0) CurrentProject.Stroks[index].Msg = 151;
            if (comboBox2.SelectedIndex == 1) CurrentProject.Stroks[index].Msg = 152;
            if (comboBox2.SelectedIndex == 2) CurrentProject.Stroks[index].Msg = 55;
            if (comboBox2.SelectedIndex == 3) CurrentProject.Stroks[index].Msg = 40;
            //Меняем время
            try { CurrentProject.Stroks[index].Hour = Convert.ToByte(textBox3.Text); }
            catch { CurrentProject.Stroks[index].Hour = 0; }
            try { CurrentProject.Stroks[index].Min = Convert.ToByte(textBox4.Text); }
            catch { CurrentProject.Stroks[index].Min = 0; }
            try { CurrentProject.Stroks[index].Sec = Convert.ToByte(textBox5.Text); }
            catch { CurrentProject.Stroks[index].Sec = 0; }
            if (CurrentProject.Stroks[index].Hour > 59) CurrentProject.Stroks[index].Hour = 59;
            if (CurrentProject.Stroks[index].Min > 59) CurrentProject.Stroks[index].Min = 59;
            if (CurrentProject.Stroks[index].Sec > 59) CurrentProject.Stroks[index].Sec = 59;
            Project.Changed = true;
            DrawDocument();
        }
        //Создаём новую строку
        private void button5_Click(object sender, EventArgs e)
        {
            if (CurrentProject.Stroks.Count == 0) return;
            CurrentProject.Stroks.Add(new Stroka(CurrentProject.Stroks[0].Int1, CurrentProject.Stroks[0].Int2, CurrentProject.Stroks[0].Day));
            CurrentProject.Stroks[CurrentProject.Stroks.Count - 1].Sotr = Editor.Sotrudniki[listBox1.SelectedIndex].ID;
            SaveStroka(CurrentProject.Stroks.Count - 1);
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = -1;
            listView1_SelectedIndexChanged(null, null);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Точно удалить?", "Удаление записи", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                CurrentProject.Stroks.RemoveAt(Finded[listView1.SelectedIndices[0]]);
                Project.Changed = true;
                DrawDocument();
            }
        }
    }
}

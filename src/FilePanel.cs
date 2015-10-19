using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using PsCon;

namespace FileManager
{
    class FilePanel
    {
        //========================== Static ==========================

        #region Parameters
        
        public static int PANEL_HEIGHT = 18;
        public static int PANEL_WIDTH = 120;

        #endregion

        //========================== Fields ==========================

        #region Panel location

        private int top;
        public int Top
        {
            get
            {
                return this.top;
            }
            set
            {
                if (0 <= value && value <= Console.WindowHeight - FilePanel.PANEL_HEIGHT)
                {
                    this.top = value;
                }
                else
                {
                    throw new Exception(String.Format("Выход координаты top ({0}) файловой панели за допустимое значение", value));
                }
            }
        }

        private int left;
        public int Left
        {
            get
            {
                return this.left;
            }
            set
            {
                if (0 <= value && value <= Console.WindowWidth - FilePanel.PANEL_WIDTH)
                {
                    this.left = value;
                }
                else
                {
                    throw new Exception(String.Format("Выход координаты left ({0}) файловой панели за пределы окна", value));
                }
            }
        }

        private int height = FilePanel.PANEL_HEIGHT;
        public int Height
        {
            get
            {
                return this.height;
            }
            set
            {
                if (0 < value && value <= Console.WindowHeight)
                {
                    this.height = value;
                }
                else
                {
                    throw new Exception(String.Format("Высота файловой панели {0} больше размера окна", value));
                }
            }
        }

        private int width = FilePanel.PANEL_WIDTH;
        public int Width
        {
            get
            {
                return this.width;
            }
            set
            {
                if (0 < value && value <= Console.WindowWidth)
                {
                    this.width = value;
                }
                else
                {
                    throw new Exception(String.Format("Ширина файловой панели {0} больше размера окна", value));
                }
            }
        }
        #endregion

        #region Panel state
        
        private string path;
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                DirectoryInfo di = new DirectoryInfo(value);
                if (di.Exists)
                {
                    this.path = value;
                }
                else
                {
                    throw new Exception(String.Format("Путь {0} не существует", value));
                }
            }
        }
        
        private int activeObjectIndex = 0;
        private int firstObjectIndex = 0;
        private int displayedObjectsAmount = PANEL_HEIGHT - 2;
        private bool active;
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                this.active = value;
            }
        }
        private bool discs;
        public bool isDiscs
        {
            get
            {
                return this.discs;
            }
        }
                                  
        #endregion
                
        private List<FileSystemInfo> fsObjects = new List<FileSystemInfo>();

        //========================== Methods ==========================

        #region Ctor

        public FilePanel()
        {
            this.SetDiscs();
        }
      
        public FilePanel(string path)
        {
            this.path = path;
            this.SetLists();
        }

        #endregion

        public FileSystemInfo GetActiveObject()
        {
            if (this.fsObjects != null && this.fsObjects.Count != 0)
            {
                return this.fsObjects[this.activeObjectIndex];
            }
            throw new Exception("Список объектов панели пуст");
        }

        public bool FindFile(string name)
        {
            int index = 0;
            foreach (FileSystemInfo file in this.fsObjects)
            {
                if (file != null && file.Name == name)
                {
                    this.activeObjectIndex = index;
                    if (this.activeObjectIndex > this.displayedObjectsAmount)
                    {
                        this.firstObjectIndex = activeObjectIndex;
                    }
                    this.UpdateContent(false);
                    return true;
                }
                index++;
            }
            return false;
        }

        #region Navigations
       
        public void KeyboardProcessing(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    this.ScrollUp();
                    break;
                case ConsoleKey.DownArrow:
                    this.ScrollDown();
                    break;
                case ConsoleKey.Home:
                    this.GoBegin();
                    break;
                case ConsoleKey.End:
                    this.GoEnd();
                    break;
                case ConsoleKey.PageUp:
                    this.PageUp();
                    break;
                case ConsoleKey.PageDown:
                    this.PageDown();
                    break;
                default:
                    break;
            }
        }
       
        private void ScrollDown()
        {
            if (this.activeObjectIndex >= this.firstObjectIndex + this.displayedObjectsAmount - 1)
            {
                this.firstObjectIndex += 1;
                if (this.firstObjectIndex + this.displayedObjectsAmount >= this.fsObjects.Count)
                {
                    this.firstObjectIndex = this.fsObjects.Count - this.displayedObjectsAmount;
                }
                this.activeObjectIndex = this.firstObjectIndex + this.displayedObjectsAmount - 1;
                this.UpdateContent(false);
            }

            else
            {
                if (this.activeObjectIndex >= this.fsObjects.Count - 1)
                {
                    return;
                }
                this.DeactivateObject(this.activeObjectIndex);
                this.activeObjectIndex++;
                this.ActivateObject(this.activeObjectIndex);
            }
        }

        private void ScrollUp()
        {
            if (this.activeObjectIndex <= this.firstObjectIndex)
            {
                this.firstObjectIndex -= 1;
                if (this.firstObjectIndex < 0)
                {
                    this.firstObjectIndex = 0;
                }
                this.activeObjectIndex = firstObjectIndex;
                this.UpdateContent(false);
            }
            else
            {
                this.DeactivateObject(this.activeObjectIndex);
                this.activeObjectIndex--;
                this.ActivateObject(this.activeObjectIndex);
            }
        }

        private void GoEnd()
        {
            if (this.firstObjectIndex + this.displayedObjectsAmount < this.fsObjects.Count)
            {
                this.firstObjectIndex = this.fsObjects.Count - this.displayedObjectsAmount;
            }
            this.activeObjectIndex = this.fsObjects.Count - 1;
            this.UpdateContent(false);
        }

        private void GoBegin()
        {
            this.firstObjectIndex = 0;
            this.activeObjectIndex = 0;
            this.UpdateContent(false);
        }

        private void PageDown()
        {
            if (this.activeObjectIndex + this.displayedObjectsAmount < this.fsObjects.Count)
            {
                this.firstObjectIndex += this.displayedObjectsAmount;
                this.activeObjectIndex += this.displayedObjectsAmount;
            }
            else
            {
                this.activeObjectIndex = this.fsObjects.Count - 1;
            }
            this.UpdateContent(false);
        }
     
        private void PageUp()
        {
            if (this.activeObjectIndex > this.displayedObjectsAmount)
            {
                this.firstObjectIndex -= this.displayedObjectsAmount;
                if (this.firstObjectIndex < 0)
                {
                    this.firstObjectIndex = 0;
                }

                this.activeObjectIndex -= this.displayedObjectsAmount;

                if (this.activeObjectIndex < 0)
                {
                    this.activeObjectIndex = 0;
                }
            }
            else
            {
                this.firstObjectIndex = 0;
                this.activeObjectIndex = 0;
            }
            this.UpdateContent(false);
        }

        #endregion

        #region Fill panels
       
        public void SetLists()
        {
            if (this.fsObjects.Count != 0)
            {
                this.fsObjects.Clear();
            }

            this.discs = false;

            DirectoryInfo levelUpDirectory = null;
            this.fsObjects.Add(levelUpDirectory);

            //Directories

            string[] directories = Directory.GetDirectories(this.path);
            foreach (string directory in directories)
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                this.fsObjects.Add(di);
            }

            //Files

            string[] files = Directory.GetFiles(this.path);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                this.fsObjects.Add(fi);
            }
        }

        public void SetDiscs()
        {
            if (this.fsObjects.Count != 0)
            {
                this.fsObjects.Clear();
            }

            this.discs = true;

            DriveInfo[] discs = DriveInfo.GetDrives();
            foreach (DriveInfo disc in discs)
            {
                if (disc.IsReady)
                {
                    DirectoryInfo di = new DirectoryInfo(disc.Name);
                    this.fsObjects.Add(di);
                }
            }
        }

        #endregion

        #region Display methods

        public void Show()
        {
            this.Clear();

            PsCon.PsCon.PrintFrameDoubleLine(this.left, this.top, this.width, this.height, ConsoleColor.White, ConsoleColor.Black);

            StringBuilder caption = new StringBuilder();
            if (this.discs)
            {
                caption.Append(' ').Append("Диски").Append(' ');
            }
            else
            {
                caption.Append(' ').Append(this.path).Append(' ');
            }
            PsCon.PsCon.PrintString(caption.ToString(), this.left + this.width / 2 - caption.ToString().Length / 2, this.top, ConsoleColor.White, ConsoleColor.Black);

            this.PrintContent();
        }

        public void Clear()
        {
            for (int i = 0; i < this.height; i++)
            {
                string space = new String(' ', this.width);
                Console.SetCursorPosition(this.left, this.top + i);
                Console.Write(space);
            }
        }

        private void PrintContent()
        {
            if (this.fsObjects.Count == 0)
            {
                return;
            }
            int count = 0;

            int lastElement = this.firstObjectIndex + this.displayedObjectsAmount;
            if (lastElement > this.fsObjects.Count)
            {
                lastElement = this.fsObjects.Count;
            }


            if (this.activeObjectIndex >= this.fsObjects.Count)
            {
                activeObjectIndex = 0;
            }

            for (int i = this.firstObjectIndex; i < lastElement; i++)
            {
                Console.SetCursorPosition(this.left + 1, this.top + count + 1);
                
                if (i == this.activeObjectIndex && this.active == true)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                }
                this.PrintObject(i);
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                count++;
            }
        }

        private void ClearContent()
        {
            for (int i = 1; i < this.height - 1; i++)
            {
                string space = new String(' ', this.width - 2);
                Console.SetCursorPosition(this.left + 1, this.top + i);
                Console.Write(space);
            }
        }

        private void PrintObject(int index)
        {
            if (index < 0 || this.fsObjects.Count <= index)
            {
                throw new Exception(String.Format("Невозможно вывести объект c индексом {0}. Выход индекса за диапазон", index));
            }

            int currentCursorTopPosition = Console.CursorTop;
            int currentCursorLeftPosition = Console.CursorLeft;

            if (!this.discs && index == 0)
            {
                Console.Write("..");
                return;
            }

            Console.Write("{0}", fsObjects[index].Name);
            Console.SetCursorPosition(currentCursorLeftPosition + this.width / 2, currentCursorTopPosition);
            if (fsObjects[index] is DirectoryInfo)
            {
                Console.Write("{0}", ((DirectoryInfo)fsObjects[index]).LastWriteTime);
            }
            else
            {
                Console.Write("{0}", ((FileInfo)fsObjects[index]).Length);
            }
        }

        public void UpdatePanel()
        {
            this.firstObjectIndex = 0;
            this.activeObjectIndex = 0;
            this.Show();
        }

        public void UpdateContent(bool updateList)
        {
            if (updateList)
            {
                this.SetLists();
            }
            this.ClearContent();
            this.PrintContent();
        }

        private void ActivateObject(int index)
        {
            int offsetY = this.activeObjectIndex - this.firstObjectIndex;
            Console.SetCursorPosition(this.left + 1, this.top + offsetY + 1);
            
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            
            this.PrintObject(index);
           
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private void DeactivateObject(int index)
        {
            int offsetY = this.activeObjectIndex - this.firstObjectIndex;
            Console.SetCursorPosition(this.left + 1, this.top + offsetY + 1);
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
           
            this.PrintObject(index);
        }

        #endregion
    }
}

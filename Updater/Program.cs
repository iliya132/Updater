using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.IO;
using System;
/// <summary>
/// Программа для обновления -консольное приложение.
/// В качестве аргументов принимает полные имена файлов для копирования/запуска
/// Копирование происходит в расположение updater.exe
/// Ключевые слова:
/// -g(get)
/// -r(run)
/// -k(kill)
/// Пример 
/// -kText.txt
/// -gC://ProgramFiles/Test.txt
/// -rTest.txt
/// </summary>
namespace Updater
{
    class Program
    {
        enum Operation{
            Get=0,
            Kill=1,
            Run=2
        }
        static void Main(string[] args)
        {
            Console.WindowWidth = 180;
            if (args.Length < 1) return;
            List<string> filesToCopy = new List<string>();
            List<string> filesToRun = new List<string>();
            List<string> filesToKill = new List<string>();
            Operation currentOperation = 0;
            string currentDirectoryStr = Directory.GetCurrentDirectory();

            #region Наполняем коллекции операций
            foreach (string line in args)
            {
                if (line.ToLower().Equals("-g")) { currentOperation = Operation.Get; continue; }
                if (line.ToLower().Equals("-k")) { currentOperation = Operation.Kill; continue; }
                if (line.ToLower().Equals("-r")) { currentOperation = Operation.Run; continue; }

                switch (currentOperation)
                {
                    case (Operation.Get):
                        filesToCopy.Add(line);
                        break;
                    case (Operation.Kill):
                        filesToKill.Add(line);
                        break;
                    case (Operation.Run):
                        filesToRun.Add(line);
                        break;
                }
            }
            #endregion

            #region KillProcesses
            foreach(string fileStr in filesToKill)
            {
                int count = 10;
                while (Process.GetProcessesByName(fileStr).Length > 0)
                {
                    Console.WriteLine($"Try to kill {fileStr} process");
                    Process.GetProcessesByName(fileStr).First().Kill();
                    Thread.Sleep(100);
                    count--;
                    if (count <= 0) { 
                        Console.WriteLine($"Unable to terminate {fileStr} process");
                        break;
                    }
                }
            }
            #endregion

            #region  Удаляем все файлы в директории
            foreach(string fileStr in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                // игнорируем файл updater.exe
                if (fileStr.ToLower().IndexOf("updater.exe") > -1) continue; 
                
                //все остальные файлы пытаемся удалить
                try
                {
                    File.Delete(fileStr);
                    Console.WriteLine($"Deleting {new FileInfo(fileStr).Name}");
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            #endregion

            #region Копируем новые файлы
            foreach(string fileStr in filesToCopy)
            {
                FileInfo fileInfo = new FileInfo(fileStr);
                Console.WriteLine($"Copy new file {fileInfo.Name}");
                try
                {
                    File.Copy(fileStr, $"{currentDirectoryStr}/{fileInfo.Name}");
                    //TODO: отобразить прогресс
                }
                catch { Console.WriteLine($"Unable to find {fileStr}"); }
            }
            #endregion

            #region Запускаем нужные программы
            foreach(string fileStr in filesToRun)
            {
                Process.Start(fileStr);
            }
            #endregion

        }
    }
}

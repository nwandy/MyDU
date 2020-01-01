using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MyDu
{
    class Program
    {
        static string searchPath = "";
        static string endString = "\r\n";
        static bool printFiles = false;
        static bool printTotal = false;
        static bool printHuman = false;
        static bool separatedirs = false;
        static bool printDetail = true;
        static bool printTime = false;
        static int maxLevel = 0;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {

                Console.WriteLine("MyDu command, short for My disk usage, is used to estimate file space usage.");
                Console.WriteLine("The du command can be used to track the files and directories which are consuming excessive amount of space on hard disk drive.");
                Console.WriteLine("Syntax:");
                Console.WriteLine("du [OPTION]... [FILE]...");
                Console.WriteLine("Options :");
                Console.WriteLine("-0, –null : end each output line with NULL");
                Console.WriteLine("-a, –all: write count of all files, not just directories");
                Console.WriteLine("-B, –block-size = SIZE : scale sizes to SIZE before printing on console");
                Console.WriteLine("-c, –total: produce grand total");
                Console.WriteLine("-d, –max-depth = N : print total for directory only if it is N or fewer levels below command line argument");
                Console.WriteLine("-h, –human-readable : print sizes in human readable format");
                Console.WriteLine("-S, -separate-dirs : for directories, don’t include size of subdirectories");
                Console.WriteLine("-s, –summarize : display only total for each directory");
                Console.WriteLine("–time : show time of last modification of any file or directory.");
                Console.WriteLine("–exclude = PATTERN : exclude files that match PATTERN");

            }
            else
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("-"))
                    {
                        switch (arg)
                        {
                            case "-0":
                            case "-null":
                                endString = "\0";
                                break;
                            case "-a":
                            case "-all":
                                printFiles = true;
                                break;
                            case "-c":
                            case "-total":
                                printTotal = true;
                                break;
                            case "-h":
                            case "-human":
                                printHuman = true;
                                break;
                            case "-S":
                            case "-separate-dirs":
                                separatedirs = true;
                                break;
                            case "-s":
                            case "-summarize":
                                printDetail = false;
                                break;
                            case "-time":
                                printTime = true;
                                break;
                        }
                        if (arg.StartsWith("-d"))
                        {
                            int.TryParse(arg.Replace("-d", ""), out maxLevel);
                        }
                        if (arg.StartsWith("-max-depth"))
                        {
                            int.TryParse(arg.Replace("-d", ""), out maxLevel);
                        }
                    }
                    else
                    {
                        searchPath = arg;
                    }
                }
                printSize(searchPath);
            }

        }
        static void printSize(string path)
        {
            var spin = new ConsoleSpinner();
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                IEnumerable<DirectoryInfo> ed = di.EnumerateDirectories();
                long totalsize = 0;

                foreach (var sd in ed)
                {
                    totalsize += getDirs(sd, true, 1, printDetail, 1);
                }

                totalsize += getDirs(di, false, 1, true, 0);
                if (printTotal)
                {
                    printConsole(totalsize, "Total", "");
                }
            }
            else
            { Console.Write("Path {0} not exists", path); }
        }
        static long getDirs(DirectoryInfo di, bool subs, int maxlevel, bool print, int actualLevel)
        {
            long size = 0;
            long total = 0;
            var spin = new ConsoleSpinner();
            if (separatedirs)
            {
                foreach (FileInfo fi in di.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    spin.Turn();
                    size += fi.Length;
                    if (printFiles)
                    { printConsole(fi.Length, fi.FullName, printTime ? fi.LastWriteTime.ToString() : ""); }
                }
                if (print)
                {
                    printConsole(size, di.FullName, printTime ? di.LastWriteTime.ToString() : "");
                }
                if (subs)
                {
                    if (maxLevel==0 || actualLevel < maxLevel)
                    {
                        foreach (DirectoryInfo sd in di.GetDirectories())
                        {
                            size += getDirs(sd, true, 0, print, actualLevel + 1);
                        }
                    }
                }
            }
            else
            {
                foreach (FileInfo fi in di.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    spin.Turn();
                    total += fi.Length;
                    if (subs)
                    {
                        size += fi.Length;
                    }
                    else
                    {
                        if (fi.DirectoryName == di.FullName)
                        { size += fi.Length; }
                    }
                    if (printFiles)
                    { printConsole(fi.Length, fi.FullName, printTime ? fi.LastWriteTime.ToString() : ""); }
                    
                }
                if (maxLevel == 0 || actualLevel < maxLevel)
                {
                    long subsize=0;
                    foreach (DirectoryInfo sd in di.GetDirectories())
                    {
                        subsize += getDirs(sd, true, 0, false, actualLevel+1);
                        total += subsize;
                        if (subs)
                        {
                            size += subsize;
                        }
                    }
                }
                if (print)
                {
                    printConsole(total, di.FullName, printTime ? di.LastWriteTime.ToString() : "");
                }
                //foreach (FileInfo fi in di.GetFiles("*", SearchOption.AllDirectories))
                //{
                //    spin.Turn();
                //    total += fi.Length;
                //    if (subs)
                //    {
                //        size += fi.Length;
                //    }
                //    else
                //    {
                //        if (fi.DirectoryName == di.FullName)
                //        { size += fi.Length; }
                //    }
                //    if (printFiles)
                //    { printConsole(fi.Length, fi.FullName, printTime ? fi.LastWriteTime.ToString() : ""); }
                //}
                //if (print)
                //{
                //    printConsole(total, di.FullName, printTime ? di.LastWriteTime.ToString() : "");
                //}

            }
            return size;
        }
        static void printConsole(long totalsize, string name, string time)
        {
            long size = 0;
            string type = "";

            if (printHuman)
            {
                if (totalsize > 1000 && totalsize < 1000000)
                {
                    size = totalsize / 1000;
                    type = "Kb";
                }
                else
                {
                    if (totalsize > 1000000 && totalsize < 1000000000)
                    {
                        size = totalsize / 1000000;
                        type = "Mb";
                    }
                    else
                    {
                        if (totalsize > 1000000000 && totalsize < 1000000000000)
                        {
                            size = totalsize / 1000000000;
                            type = "Gb";
                        }
                    }
                }
            }
            else
            {
                size = totalsize / 1000;
                type = "";
            }
            Console.Write(" {0}{1} {2} {3}{4}", size, type, time, name, endString);

        }
    }
    public class ConsoleSpinner
    {
        int counter;

        public void Turn()
        {
            counter++;
            switch (counter % 4)
            {
                case 0: Console.Write("/"); counter = 0; break;
                case 1: Console.Write("-"); break;
                case 2: Console.Write("\\"); break;
                case 3: Console.Write("|"); break;
            }
            Thread.Sleep(100);
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
    }
}

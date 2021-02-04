using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;

namespace FileManager
{
    class Program
    {
        [Flags]
        internal enum TimeType
        {
            Creation = 1 << 0,
            Access = 1 << 1,
            Write = 1 << 2
        }

        [Flags]
        internal enum FileType
        {
            File = 1 << 0,
            Directory = 1 << 1
        }

        internal class FileTimeOption
        {
            public DateTime Time { get; set; }

            public TimeType Type { get; set; } = TimeType.Creation;
            public string Filter { get; set; }
            public string Path { get; set; }
            public FileType FileType { get; set; } = FileType.File | FileType.Directory;
        }

        private static void FindFiles(string basePath, List<string> files, List<string> dirs, string filter = null)
        {
            var childFiles = string.IsNullOrWhiteSpace(filter) ? Directory.GetFiles(basePath) : Directory.GetFiles(basePath, filter);
            files.AddRange(childFiles);
            var childDirs = Directory.GetDirectories(basePath);
            if (childDirs.Any())
            {
                dirs.AddRange(childDirs);
                foreach (var dir in childDirs)
                {
                    FindFiles(dir, files, dirs, filter);
                }
            }
        }

        static void Main(string[] args)
        {
            var command = new RootCommand();

            var options = new Option[]
            {
                new Option<TimeType>(new string[] { "-t", "--type" }, "时间类型：1,创建时间;2,最后访问时间;4,最后修改时间."),
                new Option<string>(new string[] { "-f", "--filter" }, "文件过滤器"),
                new Option<string>(new string[] { "-p", "--path" }, "文件路径,默认当前路径"),
                new Option<FileType>(new string[] { "--file-type" }, "修改模式;1,文件；2,文件夹")
            };

            var createTimeCommand = new Command("time", "修改时间"){
                new Argument<string>("time", "时间"),
                options[0],
                options[1],
                options[2],
                options[3]
            };

            createTimeCommand.Handler = CommandHandler.Create<FileTimeOption>(opt =>
            {
                //Console.WriteLine($"type:{opt.Type},time:{opt.Time},filter:{opt.Filter},path:{opt.Path},dir:{opt.dir}");
                if (string.IsNullOrWhiteSpace(opt.Path))
                {
                    opt.Path = Directory.GetCurrentDirectory();
                }
                var paths = new List<string>();
                var dirs = new List<string>();
                if (File.Exists(opt.Path))
                {
                    paths.Add(opt.Path);
                }
                else if (Directory.Exists(opt.Path))
                {
                    dirs.Add(opt.Path);
                    FindFiles(opt.Path, paths, dirs, opt.Filter);
                }
                else
                {
                    Console.WriteLine("路径不存在");
                    return;
                }
                if (opt.FileType.HasFlag(FileType.File))
                {
                    Console.WriteLine($"找到 {paths.Count} 份文件");
                    foreach (var path in paths)
                    {
                        var fileInfo = new FileInfo(path);
                        if (opt.Type.HasFlag(TimeType.Creation))
                        {
                            fileInfo.CreationTime = opt.Time;
                        }
                        if (opt.Type.HasFlag(TimeType.Write))
                        {
                            fileInfo.LastWriteTime = opt.Time;
                        }
                        if (opt.Type.HasFlag(TimeType.Access))
                        {
                            fileInfo.LastAccessTime = opt.Time;
                        }
                    }
                }
                if (opt.FileType.HasFlag(FileType.Directory))
                {
                    Console.WriteLine($"找到 {dirs.Count} 份文件夹");
                    foreach (var dir in dirs)
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        if (opt.Type.HasFlag(TimeType.Creation))
                        {
                            dirInfo.CreationTime = opt.Time;
                        }
                        if (opt.Type.HasFlag(TimeType.Write))
                        {
                            dirInfo.LastWriteTime = opt.Time;
                        }
                        if (opt.Type.HasFlag(TimeType.Access))
                        {
                            dirInfo.LastAccessTime = opt.Time;
                        }
                    }
                }
            });

            command.AddCommand(createTimeCommand);

            command.Invoke(args);


            //if (args == null || args.Length == 0) return;
            //var cmd = args[0];
            //var paths = new List<string>();
            //if (args.Length == 1)
            //{
            //    var basePath = Directory.GetCurrentDirectory();
            //    FindFiles(basePath, paths);
            //}
            //else
            //{
            //    for (var i = 1; i < args.Length; i++)
            //    {
            //        paths.Add(args[i]);
            //    }
            //}
            //Console.WriteLine($"find file count:{paths.Count}");
            //switch (cmd)
            //{
            //    case "-dc":
            //    case "--create-date":
            //        foreach (var path in paths)
            //        {
            //            //:todo
            //        }
            //        break;
            //}
        }
    }
}

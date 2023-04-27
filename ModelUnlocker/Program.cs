using System;
using System.IO;
using System.Xml;
using CommandLine;
using CodeWalker.GameFiles;
using System.Linq;

namespace ModelUnlocker;

internal class Program
{
    private static int LOG_DEBUG = 0;
    private static int LOG_INFO = 1;
    private static int LOG_WARNING = 2;
    private static int LOG_ERROR = 3;
    private static int CURRENT_LOG_LVL = 1;
    
    public class Options {
        [Option('i', "input", Required = true, HelpText = "The yft file or folder of yft files to unlock")]
        public string Input { get; set; }

        [Option('o', "output", Required = false, HelpText = "The output folder of unlocked yft file(s)")]
        public string Output { get; set; }

        [Option('l', "loglevel", Required = false, HelpText = "Log level, 0: Debug, 1: Info, 2: Warning, 3: Error")]
        public int LogLevel { get; set; } = 1;

        [Option('r', "override", Required = false, HelpText = "Override exists file, always enable when output folder not specified")]
        public bool Override { get; set; }
    }
	
    private static void Main(string[] args)
	{
        // 解析命令行参数
        Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
        {
            if (o.LogLevel >= 0 && o.LogLevel <= 3) {
                CURRENT_LOG_LVL = o.LogLevel;
            }
            if (o.Input != null) {
                var inputFile = o.Input;
                var outputFile = o.Output;
                if (o.Output != null) {
                    o.Output = Path.GetFullPath(o.Output);
                    if (Directory.Exists(o.Output)) {
                        var fileName = Path.GetFileName(inputFile);
                        outputFile = o.Output + Path.DirectorySeparatorChar + fileName;
                    }
                } else {
                    outputFile = inputFile;
                    outputFile = Path.GetFullPath(outputFile);
                }
                inputFile = Path.GetFullPath(inputFile);
                inputFile = CleanArgs(inputFile);
                outputFile = CleanArgs(outputFile);

                PrintLog(LOG_DEBUG, String.Format("Finding files from: {0}", inputFile));
                
                if (File.Exists(inputFile)) {
                    PrintLog(LOG_INFO, String.Format("Unlocking file: {0}", inputFile));
                    var extension = Path.GetExtension(inputFile).Replace(".", "");
                    UnlockModel(extension, inputFile, outputFile, o.Override);
                    PrintLog(LOG_INFO, "Unlocking finished.");
                } else if (Directory.Exists(inputFile)) {
                    var files = Directory.GetFiles(inputFile, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".yft") || s.EndsWith(".ydr") || s.EndsWith(".ydd"));
                    var num = 0;
                    foreach (var file in files) {
                        num++;
                        outputFile = o.Output;
                        if (outputFile != null) {
                            outputFile = CleanArgs(outputFile);
                            if (Directory.Exists(outputFile)) {
                                var fileName = Path.GetFileName(file);
                                outputFile = outputFile + Path.DirectorySeparatorChar + fileName;
                                outputFile = Path.GetFullPath(outputFile);
                            } else {
                                PrintLog(LOG_ERROR, "Output folder not exists: " + outputFile);
                                return;
                            }
                        } else {
                            outputFile = file;
                            outputFile = Path.GetFullPath(outputFile);
                        }
                        outputFile = CleanArgs(outputFile);
                        PrintLog(LOG_INFO, String.Format("Unlocking file: {0}", file));
                        var extension = Path.GetExtension(file).Replace(".", "");
                        UnlockModel(extension, file, outputFile, o.Override);
                    }
                    PrintLog(LOG_INFO, String.Format("Unlocking finished, {0} files processed.", num));
                } else {
                    PrintLog(LOG_ERROR, "Input file not exists: " + inputFile);
                }
            }
        });
    }

    private static string CleanArgs(string arg) {
        if (arg.EndsWith('"')) {
            arg = arg.Substring(0, arg.Length - 1);
        }
        if (arg.EndsWith("'")) {
            arg = arg.Substring(0, arg.Length - 1);
        }
        if (arg.EndsWith("\\")) {
            arg = arg.Substring(0, arg.Length - 1);
        }
        if (arg.EndsWith("/")) {
            arg = arg.Substring(0, arg.Length - 1);
        }
        if (Path.DirectorySeparatorChar == '\\') {
            arg = arg.Replace("/", "\\");
        } else {
            arg = arg.Replace("\\", "/");
        }
        return arg;
    }

    private static string GetYftXml(string inputFile) {
        var fileEntry = new YftFile();
        try {
            PrintLog(LOG_DEBUG, String.Format("Reading data from {0}", inputFile));
            byte[] data = File.ReadAllBytes(inputFile);
            RpfFile.LoadResourceFile(fileEntry, data, 162u);
            RpfFileEntry rfe = fileEntry.RpfFileEntry;
            return MetaXml.GetXml(fileEntry, out var _, "");
        } catch (Exception) {
            PrintLog(LOG_ERROR, "Unable to convert file, is this a correct yft file?");
            return null;
        }
    }

    private static string GetYdrXml(string inputFile) {
        var fileEntry = new YdrFile();
        try {
            PrintLog(LOG_DEBUG, String.Format("Reading data from {0}", inputFile));
            byte[] data = File.ReadAllBytes(inputFile);
            RpfFile.LoadResourceFile(fileEntry, data, 162u);
            RpfFileEntry rfe = fileEntry.RpfFileEntry;
            return MetaXml.GetXml(fileEntry, out var _, "");
        } catch (Exception) {
            PrintLog(LOG_ERROR, "Unable to convert file, is this a correct ydr file?");
            return null;
        }
    }

    private static string GetYddXml(string inputFile) {
        var fileEntry = new YddFile();
        try {
            PrintLog(LOG_DEBUG, String.Format("Reading data from {0}", inputFile));
            byte[] data = File.ReadAllBytes(inputFile);
            RpfFile.LoadResourceFile(fileEntry, data, 162u);
            RpfFileEntry rfe = fileEntry.RpfFileEntry;
            return MetaXml.GetXml(fileEntry, out var _, "");
        } catch (Exception) {
            PrintLog(LOG_ERROR, "Unable to convert file, is this a correct ydd file?");
            return null;
        }
    }

    private static byte[] ConvertYft(string xml) {
        // 创建 XML 文档
        PrintLog(LOG_DEBUG, "Creating xml document");
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        // 将 XML 转回 Yft 实现解锁
        PrintLog(LOG_DEBUG, "Getting xml data from yft file");
        YftFile yft = XmlYft.GetYft(xmlDocument);
        if (yft.Fragment == null) {
            PrintLog(LOG_ERROR, "Unable to pack the file.");
            return null;
        }

        // 保存到数组
        return yft.Save();
    }

    private static byte[] ConvertYdr(string xml) {
        // 创建 XML 文档
        PrintLog(LOG_DEBUG, "Creating xml document");
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        // 将 XML 转回 Ydr 实现解锁
        PrintLog(LOG_DEBUG, "Getting xml data from ydr file");
        YdrFile ydr = XmlYdr.GetYdr(xmlDocument);
        if (ydr.Drawable == null) {
            PrintLog(LOG_ERROR, "Unable to pack the file.");
            return null;
        }

        // 保存到数组
        return ydr.Save();
    }

    private static byte[] ConvertYdd(string xml) {
        // 创建 XML 文档
        PrintLog(LOG_DEBUG, "Creating xml document");
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        // 将 XML 转回 Ydd 实现解锁
        PrintLog(LOG_DEBUG, "Getting xml data from ydd file");
        YddFile ydd = XmlYdd.GetYdd(xmlDocument);
        if (ydd.DrawableDict == null) {
            PrintLog(LOG_ERROR, "Unable to pack the file.");
            return null;
        }

        // 保存到数组
        return ydd.Save();
    }

    // 解锁
    private static bool UnlockModel(string type, string inputFile, string outputFile, bool overRide) {
        string xml = "";
        
        // 尝试将源文件读入内存
        switch(type) {
            case "yft":
                xml = GetYftXml(inputFile);
                break;
            case "ydr":
                xml = GetYdrXml(inputFile);
                break;
            case "ydd":
                xml = GetYddXml(inputFile);
                break;
            default:
                PrintLog(LOG_ERROR, "Unknown file type.");
                return false;
        }
        
        if (string.IsNullOrEmpty(xml)) {
            PrintLog(LOG_ERROR, "Cannot export original data.");
            return false;
        }
        
        byte[] array = null;
        
        switch(type) {
            case "yft":
                array = ConvertYft(xml);
                break;
            case "ydr":
                array = ConvertYdr(xml);
                break;
            case "ydd":
                array = ConvertYdd(xml);
                break;
        }

        // 尝试写入文件
        try {
            string newName = Path.GetFileNameWithoutExtension(outputFile);
            string newPath = Path.GetDirectoryName(outputFile);
            string newFile = newPath + Path.DirectorySeparatorChar + newName + "." + type;
            newFile = Path.GetFullPath(newFile);
            if (Directory.Exists(outputFile)) {
                newName = Path.GetFileNameWithoutExtension(inputFile);
                newFile = outputFile + Path.DirectorySeparatorChar + newName + "." + type;
                newFile = Path.GetFullPath(newFile);
            }
            if (overRide || !File.Exists(newFile)) {
                if (File.Exists(newFile)) {
                    File.Delete(newFile);
                }
                File.WriteAllBytes(newFile, array);
                PrintLog(LOG_DEBUG, String.Format("Write to {0} successful", newFile));
                return true;
            } else {
                PrintLog(LOG_ERROR, "Output file already exists.");
            }
        } catch (Exception e) {
            PrintLog(LOG_ERROR, e.Message);
            PrintLog(LOG_ERROR, "Unable to write the file.");
        }
        return false;
    }

    // 日志输出
    private static void PrintLog(int logLevel, string message) {
        if (CURRENT_LOG_LVL > logLevel) return;
        var time = DateTime.Now.ToString("HH:mm:ss");
        var levelList = new string[] { "DEBUG", "INFO", "WARNING", "ERROR" };
        var colorList = new ConsoleColor[] { ConsoleColor.Blue, ConsoleColor.White, ConsoleColor.Yellow, ConsoleColor.Red };
        var level = levelList[logLevel];
        var color = colorList[logLevel];
        if (logLevel != 1) {
            Console.ForegroundColor = color;
        }
        Console.WriteLine($"[{time}][{level}] {message}");
        Console.ResetColor();
    }
}

using NerdsTeaserBot.Modules.Models;
using Nerd_STF.File.Saving;
using Nerd_STF.Lists;
using System.IO;
using System.Reflection;

namespace NerdsTeaserBot.Misc
{
    public static class Data
    {
        internal static readonly string appPath = Assembly.GetExecutingAssembly().Location.Remove(Assembly.GetExecutingAssembly().Location.Length - Assembly.GetExecutingAssembly().GetName().Name.Length - 5);

        internal static TextFile log = new("", "");
        internal static BinaryFile<ConstData> consts = new(appPath + "/data/constant/const.ntb", new());
        internal static BinaryFile<MiscData> misc = new("", new());
        internal static BinaryFile<List<User>> users = new("", new());

        public static void LoadAll(ulong serverID)
        {
            consts.Load();

            string folder = appPath + "/data/files-" + serverID;

            log = TextFile.Load(folder + "/log.txt");
            misc = BinaryFile<MiscData>.Load(folder + "/misc.ntb");
            users = BinaryFile<List<User>>.Load(folder + "/users.ntb");
        }
        public static void SaveAll(ulong serverID)
        {
            consts.Save();

            string folder = appPath + "/data/files-" + serverID;
            Directory.CreateDirectory(folder);

            log.Path = folder + "/log.txt";
            misc.Path = folder + "/misc.ntb";
            users.Path = folder + "/users.ntb";
            if (log.Data != null) log.Save();
            if (misc.Data != null) misc.Save();
            if (users.Data is not null) users.Save();
        }
        public static void TryLoadAll(ulong serverID)
        {
            string folder = appPath + "/data/files-" + serverID;

            log.Path = folder + "/log.txt";
            misc.Path = folder + "/misc.ntb";
            users.Path = folder + "/users.ntb";
            if (File.Exists(consts.Path)) consts.Load();
            if (File.Exists(log.Path)) log = TextFile.Load(log.Path);
            if (File.Exists(misc.Path)) misc = BinaryFile<MiscData>.Load(misc.Path);
            if (File.Exists(users.Path)) users = BinaryFile<List<User>>.Load(users.Path);
        }
    }
}
using System.Diagnostics;

namespace Kukumberman.SaveSystem.Editor
{
    public static class SaveSystemEditorTools
    {
        [UnityEditor.MenuItem("Tools/Save System/Open directory")]
        private static void OpenDirectory()
        {
            // just an example how to open explorer with an already selected file
            /*
            string arguments;
            var path = FileSaveSystem.GetSavePath("save.json").Replace("/", @"\");
            if (!System.IO.File.Exists(path))
            {
                path = System.IO.Path.GetDirectoryName(path);
                arguments = $"\"{path}\"";
            }
            else
            {
                arguments = $"/select,\"{path}\"";
            }
            */
            string arguments = string.Format(
                "\"{0}\"",
                FileSaveSystem.Persistent.GetBaseDirectory().Replace("/", @"\")
            );

            var info = new ProcessStartInfo() { FileName = "explorer.exe", Arguments = arguments, };

            var process = Process.Start(info);
            process.WaitForExit();
            process.Close();
        }
    }
}

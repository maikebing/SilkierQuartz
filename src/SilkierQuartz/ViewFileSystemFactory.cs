using HandlebarsDotNet;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace SilkierQuartz
{
    public static class ViewFileSystemFactory
    {
        public static ViewEngineFileSystem Create(SilkierQuartzOptions options)
        {
            ViewEngineFileSystem fs;
            if (string.IsNullOrEmpty(options.ViewsRootDirectory))
            {
                fs = new EmbeddedFileSystem();
            }
            else
            {
                fs = new DiskFileSystem(options.ViewsRootDirectory);
            }

            return fs;
        }

        private class DiskFileSystem : ViewEngineFileSystem
        {
            string root;

            public DiskFileSystem(string root)
            {
                this.root = root;
            }

            public override string GetFileContent(string filename)
            {
                return File.ReadAllText(GetFullPath(filename));
            }

            protected override string CombinePath(string dir, string otherFileName)
            {
                return Path.Combine(dir, otherFileName);
            }

            public override bool FileExists(string filePath)
            {
                return File.Exists(GetFullPath(filePath));
            }

            string GetFullPath(string filePath)
            {
                return Path.Combine(root, filePath.Replace("partials/", "Partials/").Replace('/', Path.DirectorySeparatorChar));
            }
        }

        private class EmbeddedFileSystem : ViewEngineFileSystem
        {
            private EmbeddedFileProvider fs = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), "SilkierQuartz.Views");
            public override string GetFileContent(string filename)
            {
                string result = string.Empty;
                var fi = fs.GetFileInfo(GetFullPath(filename));
                using (var stream =fi.CreateReadStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        result = reader.ReadToEnd();
                    }
                }
                return result;
            }

            protected override string CombinePath(string dir, string otherFileName)
            {
                return Path.Combine(dir, otherFileName);
            }

            public override bool FileExists(string filePath)
            {
                return fs.GetFileInfo(GetFullPath(filePath)).Exists;
            }
            string GetFullPath(string filePath)
            {
                return filePath.Replace("partials/", "Partials/").Replace('/', Path.DirectorySeparatorChar);
            }

        }

    }
}

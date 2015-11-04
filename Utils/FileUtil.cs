/* Author: Cameron Block
 * Purpose: Create a state file system recursion algorithm which uses IEnumerables, 
 *      so it returns faster than using a List<>. Does not error when it has access 
 *      errors, does not return the offending documents/files. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace FileRecurse {

    public static class FileUtil {

        public static void RecursiveFileCopy(string src, string dest) {
            if (!Directory.Exists(src))
                throw new IOException("Directory \"" + src + "\" does not exist! ");

            if (!Directory.Exists(dest))
                throw new IOException("Directory \"" + dest + "\" does not exist! ");

            string lastFolderSrc = LastFolderInPath(src);

            string lastFolderDest = LastFolderInPath(dest);

            //create directory structure
            foreach (var dir in FileUtil.RecurseDirectories(src)){
                Directory.CreateDirectory(dest + "\\" + GetRightPartOfPath(dir, lastFolderSrc));
            }

            //copy file structure
            foreach (var file in FileUtil.RecurseFiles(src)) {
                string str = GetRightPartOfPath(file, lastFolderSrc);

                File.Copy(file, dest + "\\" + str, true);
            }
        }//end method

        public static string LastFolderInPath(string path) {
            int lastIndex = path.LastIndexOf("\\");
            return path.Substring(lastIndex + 1, path.Length - lastIndex - 1);
        }

        public static string GetRightPartOfPath(string path, string after) {
            bool isAfter = false;
            StringBuilder str = new StringBuilder();
            string[] tokens = SplitPath(path);

            for (int i = 0; i < tokens.Length; i++) {
                if (isAfter)
                    str.Append(tokens[i] + (i == tokens.Length - 1 ? "" : "\\"));

                if (tokens[i] == after)
                    isAfter = true;
            }

            return str.ToString();
        }//end method

        public static string LeftPartOfPath(string path, string after) {
            StringBuilder str = new StringBuilder();
            string[] tokens = SplitPath(path);

            for (int i = 0; i < tokens.Length; i++) {
                if (tokens[i] == after)
                    break;

                str.Append(tokens[i] + ( i == tokens.Length ? "" : "\\" ));
            }
            
            return str.ToString();
        }//end method

        public static String[] SplitPath(string path) {
            String[] pathSeparators = new String[] { "\\" };
            return path.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
        }

        public static IEnumerable<FileSystemInfo> RecurseFiles(this DirectoryInfo dir) {
            DirectoryStateMachine directories =
                new DirectoryStateMachine(new DirectoryInfo(dir.FullName));
            DirectoryInfo dirInfo = directories.GetNext();

            FileStateMachine files =
                new FileStateMachine(dirInfo);
            FileInfo fileInfo = files.GetNext();

            //recurse the files
            while (( fileInfo = files.GetNext() ) != null) {
                yield return fileInfo;
            }

            //recurse the directories
            while (( dirInfo = directories.GetNext() ) != null) {
                //yield return dirInfo;

                //the recursive part, is different for IEnumerables
                foreach (var subFile in new DirectoryInfo(dir.FullName + "\\" + dirInfo.Name).RecurseFiles())
                    yield return subFile;
            }
        }//end method
        
        //*** need for merge solutions program ***
        public static IEnumerable<string> RecurseFiles(string path) {
            DirectoryStateMachine directories =
                new DirectoryStateMachine(new DirectoryInfo(path));
            DirectoryInfo dirInfo;

            FileStateMachine files =
                new FileStateMachine(new DirectoryInfo(path));
            FileInfo fileInfo;

            //recurse the files
            while (( fileInfo = files.GetNext() ) != null) {
                yield return fileInfo.FullName;
            }

            //recurse the directories
            while (( dirInfo = directories.GetNext() ) != null) {
                //yield return dirInfo;

                //the recursive part, is different for IEnumerables
                foreach (var subFile in RecurseFiles(path + "\\" + dirInfo.Name))
                    yield return subFile.ToString();
            }
        }//end method

        //*** need for merge solutions program ***
        public static IEnumerable<string> RecurseDirectories(string path) {
            DirectoryStateMachine directories =
                new DirectoryStateMachine(new DirectoryInfo(path));
            DirectoryInfo dirInfo;

            //recurse the directories
            while (( dirInfo = directories.GetNext() ) != null) {
                yield return dirInfo.FullName;

                //the recursive part, is different for IEnumerables
                foreach (var subDir in from folder in RecurseDirectories(path + "\\" + dirInfo.Name) select new DirectoryInfo(folder))
                    yield return subDir.FullName;
            }
        }//end method

        public static IEnumerable<FileSystemInfo> RecurseFileSystem(string path) {
            DirectoryStateMachine directories =
                new DirectoryStateMachine(new DirectoryInfo(path));
            DirectoryInfo dirInfo = directories.GetNext();

            FileStateMachine files =
                new FileStateMachine(dirInfo);
            FileInfo fileInfo = files.GetNext();

            //recurse the files
            while (( fileInfo = files.GetNext() ) != null) {
                yield return fileInfo;
            }

            //recurse the directories
            while (( dirInfo = directories.GetNext() ) != null) {
                yield return dirInfo;

                //the recursive part, is different for IEnumerables
                foreach (var subDir in RecurseFileSystem(path + "\\" + dirInfo.Name))
                    yield return subDir;
            }
        }//end method

        public static IEnumerable<FileSystemInfo> RecurseFileSystem(string path, 
            Predicate<FileSystemInfo> filter) {

            DirectoryStateMachine directories =
                new DirectoryStateMachine(new DirectoryInfo(path), filter);
            DirectoryInfo dirInfo = directories.GetNext();

            FileStateMachine files =
                new FileStateMachine(dirInfo, filter);
            FileInfo fileInfo = files.GetNext();

            //recurse the files
            while (( fileInfo = files.GetNext() ) != null) {
                if (filter.Invoke(fileInfo))
                    yield return fileInfo;
            }

            //recurse the directories
            while (( dirInfo = directories.GetNext() ) != null) {
                if(filter.Invoke(dirInfo))
                    yield return dirInfo;

                //the recursive part, is different for IEnumerables
                foreach (var subDir in RecurseFileSystem(path + "\\" + dirInfo.Name, filter))
                    yield return subDir;
            }
        }//end method
    
    }//end class

}//end namespace

/* Author: Cameron Block
 * Purpose: Create a state file system recursion algorithm which uses IEnumerables, so it returns faster than using a List<>. 
 *      Does not error when it has access errors, does not return the offending documents/files. 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileRecurse {

    //Keeps track of the Files on the current level, allows us to return in an IEnumerable fashion
    public class FileStateMachine {

        private DirectoryInfo Directory;
        private FileInfo[] files;
        private Predicate<FileInfo> filter;

        private int i = 0;

        public FileStateMachine(DirectoryInfo directory) {
            this.Directory = directory;
            this.filter = (dirInfo) => true;
        }

        public FileStateMachine(DirectoryInfo directory, Predicate<FileInfo> filter) {
            this.Directory = directory;
            this.filter = filter;
        }

        public FileInfo GetNext() {
            try {
                //execute first time
                if ( i == 0 )
                    this.files = Directory.GetFiles();

                if ( i < files.Length )
                    try {
                        if ( filter == null || filter.Invoke(files[i]) ) {
                            return files[i++];
                        }
                    }
                    catch ( Exception ) { }
            }
            catch ( Exception ) { }

            return null;

        }//end method

    }//end class

    //Keeps track of the directories on the current level, allows us to return in an IEnumerable fashion
    public class DirectoryStateMachine {

        private DirectoryInfo Directory;
        private DirectoryInfo[] directories;
        private Predicate<DirectoryInfo> filter;

        private int i = 0;

        public DirectoryStateMachine(DirectoryInfo directory) {
            this.Directory = directory;
            this.filter = (fileInfo) => true;
        }

        public DirectoryStateMachine(DirectoryInfo directory, Predicate<DirectoryInfo> filter) {
            this.Directory = directory;
            this.filter = filter;
        }

        public DirectoryInfo GetNext() {
            try {
                if ( i == 0 )
                    this.directories = Directory.GetDirectories();

                if ( i <= directories.Length )
                    try {
                        if ( filter == null || filter.Invoke(directories[i]) ) {
                            return directories[i++];
                        }
                    }
                    catch ( Exception ) { }
            }
            catch ( Exception ) { }

            return null;

        }//end method

    }//end class

}//end namespace

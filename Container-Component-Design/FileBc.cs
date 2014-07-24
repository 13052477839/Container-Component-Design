// Copyright 2011 Google Inc
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace ServiceContainer
{
    public class FileBc : IFileBc
    {
        private IDirectoryBc directoryBc;
        private bool disposed;
        private FileStream fileStream;

        public void OpenFile(string fileName, FileMode fileMode)
        {
            OpenFile(AppDomain.CurrentDomain.BaseDirectory, fileName, fileMode);
        }

        public void OpenFile(string path, string fileName, FileMode fileMode)
        {
            string combine = Path.Combine(path, fileName);
            fileStream = new FileStream(combine, fileMode);
        }

        public void Write(string content)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(content);
            fileStream.Write(buffer, 0, buffer.Length);
        }

        public void CloseFile()
        {
            fileStream.Flush();
            fileStream.Close();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
            disposed = true;
        }

        public ISite Site { get; set; }

        public event EventHandler Disposed;

        private void InitComponents()
        {
            directoryBc = Site.GetService(typeof(IDirectoryBc)) as IDirectoryBc;
            if (directoryBc == null)
            {
                throw new ComponentNotFoundException("Missing IDirectoryBc component!");
            }
        }
    }
}
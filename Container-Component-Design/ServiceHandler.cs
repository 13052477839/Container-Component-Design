﻿// Copyright 2011 Google Inc
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

using System.IO;

namespace ServiceContainer
{
    public class ServiceHandler : IServiceHandler
    {
        private readonly IFileBc fileBc;
        private IDirectoryBc directoryBc;

        public ServiceHandler(IServiceContainer context)
        {
            fileBc = context.GetService(typeof(IFileBc)) as IFileBc;
            if (fileBc == null)
            {
                throw new ComponentNotFoundException("IFileBc not found");
            }

            directoryBc = context.GetService(typeof(IDirectoryBc)) as IDirectoryBc;
            if (directoryBc == null)
            {
                throw new ComponentNotFoundException("IDirectoryBc not found");
            }
        }

        public void OpenFile(string fileName, FileMode fileMode)
        {
            fileBc.OpenFile(fileName, fileMode);
        }

        public void OpenFile(string path, string fileName, FileMode fileMode)
        {
            fileBc.OpenFile(path, fileName, fileMode);
        }

        public void Write(string content)
        {
            fileBc.Write(content);
        }

        public void CloseFile()
        {
            fileBc.CloseFile();
        }
    }
}
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

namespace ServiceContainer
{
    public class ServiceSite : ISite
    {
        private readonly IComponent component;
        private readonly IServiceContainer container;
        private String name;

        internal ServiceSite(IComponent component, IServiceContainer container, String name)
        {
            this.component = component;
            this.container = container;
            this.name = name;
        }

        // The component sited by this component site.
        public IComponent Component
        {
            get { return component; }
        }

        // The container in which the component is sited.
        public IContainer Container
        {
            get { return container; }
        }

        public Object GetService(Type service)
        {
            return ((service == typeof(ISite)) ? this : container.GetService(service));
        }

        // Indicates whether the component is in design mode.
        public bool DesignMode
        {
            get { return false; }
        }

        // The name of the component.
        //
        public String Name
        {
            get { return name; }
            set
            {
                if (value != null && name != null && value.Equals(name))
                {
                    return;
                }

                container.ValidateName(component, value);
                name = value;
            }
        }
    }
}
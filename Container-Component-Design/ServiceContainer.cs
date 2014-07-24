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
using System.Linq;

namespace ServiceContainer
{
    public class ServiceContainer : IServiceContainer
    {
        private readonly object syncObj = new Object();
        private bool checkedFilter;
        private ComponentCollection components;
        private ContainerFilterService filter;
        private int siteCount;
        private ISite[] sites;

        public virtual void Add(IComponent component)
        {
            Add(component, null);
        }

        public virtual void Add(IComponent component, String name)
        {
            lock (syncObj)
            {
                if (component == null)
                {
                    return;
                }

                ISite site = component.Site;

                if (site != null && site.Container == this)
                {
                    return;
                }

                if (sites == null)
                {
                    sites = new ISite[4];
                }
                else
                {
                    // Validate that new components
                    // have either a null name or a unique one.
                    //
                    ValidateName(component, name);

                    if (sites.Length == siteCount)
                    {
                        var newSites = new ISite[siteCount * 2];
                        Array.Copy(sites, 0, newSites, 0, siteCount);
                        sites = newSites;
                    }
                }

                if (site != null)
                {
                    site.Container.Remove(component);
                }

                ISite newSite = CreateSite(component, name);
                sites[siteCount++] = newSite;
                component.Site = newSite;
                components = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public object GetService(Type service)
        {
            if (service == typeof(IContainer))
            {
                return this;
            }

            //return required interface type component if found
            foreach (IComponent component in
                from site in sites where site.Component != null select site.Component)
            {
                //component type
                if (component.GetType() == service)
                {
                    return component;
                }

                //interface type
                if (component.GetType().GetInterfaces().Any(intf => intf == service))
                {
                    return component;
                }
            }

            return null;
        }

        public virtual ComponentCollection Components
        {
            get
            {
                lock (syncObj)
                {
                    if (components == null)
                    {
                        var result = new IComponent[siteCount];
                        for (int i = 0; i < siteCount; i++)
                        {
                            result[i] = sites[i].Component;
                        }
                        components = new ComponentCollection(result);

                        // At each component add, if we don't yet have a filter, look for one.
                        // Components may add filters.
                        if (filter == null && checkedFilter)
                        {
                            checkedFilter = false;
                        }
                    }

                    if (!checkedFilter)
                    {
                        filter = GetService(typeof(ContainerFilterService)) as ContainerFilterService;
                        checkedFilter = true;
                    }

                    if (filter != null)
                    {
                        ComponentCollection filteredComponents = filter.FilterComponents(components);

                        if (filteredComponents != null)
                        {
                            components = filteredComponents;
                        }
                    }

                    return components;
                }
            }
        }

        public virtual void Remove(IComponent component)
        {
            Remove(component, false);
        }

        public void ValidateName(IComponent component, string name)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }

            if (name != null)
            {
                for (int i = 0; i < Math.Min(siteCount, sites.Length); i++)
                {
                    ISite s = sites[i];

                    if (s != null && s.Name != null && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase) &&
                        s.Component != component)
                    {
                        var inheritanceAttribute =
                            (InheritanceAttribute)TypeDescriptor.GetAttributes(s.Component)[typeof(InheritanceAttribute)];
                        if (inheritanceAttribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly)
                        {
                            // throw new ArgumentException(SR.GetString(SR.DuplicateComponentName, name));
                        }
                    }
                }
            }
        }

        ~ServiceContainer()
        {
            Dispose(false);
        }

        protected virtual ISite CreateSite(IComponent component, string name)
        {
            return new ServiceSite(component, this, name);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (syncObj)
                {
                    while (siteCount > 0)
                    {
                        ISite site = sites[--siteCount];
                        site.Component.Site = null;
                        site.Component.Dispose();
                    }
                    sites = null;
                    components = null;
                }
            }
        }

        private void Remove(IComponent component, bool preserveSite)
        {
            lock (syncObj)
            {
                if (component == null)
                {
                    return;
                }
                ISite site = component.Site;
                if (site == null || site.Container != this)
                {
                    return;
                }
                if (!preserveSite)
                {
                    component.Site = null;
                }
                for (int i = 0; i < siteCount; i++)
                {
                    if (sites[i] == site)
                    {
                        siteCount--;
                        Array.Copy(sites, i + 1, sites, i, siteCount - i);
                        sites[siteCount] = null;
                        components = null;
                        break;
                    }
                }
            }
        }

        protected void RemoveWithoutUnsiting(IComponent component)
        {
            Remove(component, true);
        }
    }
}
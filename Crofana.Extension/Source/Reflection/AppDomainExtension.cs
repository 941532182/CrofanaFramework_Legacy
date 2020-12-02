using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Crofana.Extension
{
    public static class AppDomainExtension
    {
        private static readonly string[] EMPTY_STRING_ARRAY = { };

        public static Type GetType(this AppDomain self, string typeName, string[] domainAssemblies)
        {
            Type result = null;
            foreach (var asm in self.GetAssemblies())
            {
                if (domainAssemblies == null || domainAssemblies.Contains(asm.FullName.Split(',')[0]))
                {
                    if ((result = asm.GetType(typeName)) != null) break;
                }
            }
            return result;
        }

        public static Type GetType(this AppDomain self, string typeName) => self.GetType(typeName, null);
    }
}

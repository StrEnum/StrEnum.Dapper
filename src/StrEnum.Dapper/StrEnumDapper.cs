using System;
using System.Linq;
using Dapper;

namespace StrEnum.Dapper
{
    public static class StrEnumDapper
    {
        /// <summary>
        /// Allows Dapper to handle string enums. Make sure that all of the assemblies that contain string enums have been loaded before calling this method.
        /// </summary>
        public static void UseStringEnums()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var stringEnums = assembly.GetTypes().Where(o => o.IsStringEnum());

                var genericTypeHandler = typeof(StrEnumTypeHandler<>);
                
                foreach (var stringEnumType in stringEnums)
                {
                    var handlerType = genericTypeHandler.MakeGenericType(stringEnumType);

                    var handlerInstance = Activator.CreateInstance(handlerType);

                    SqlMapper.AddTypeHandler(stringEnumType, handlerInstance as SqlMapper.ITypeHandler);
                }
            }
        }
    }
}
using System;
using System.Linq;

namespace Cosmere.Core.Extension;

public static class TypeExtension {
    public static bool InheritsOrImplements(this Type child, Type parent) {
        parent = ResolveGenericTypeDefinition(parent);

        Type? currentChild = child.IsGenericType
            ? child.GetGenericTypeDefinition()
            : child;

        while (currentChild != typeof(object)) {
            if (parent == currentChild || HasAnyInterfaces(parent, currentChild)) {
                return true;
            }

            currentChild = currentChild.BaseType != null && currentChild.BaseType.IsGenericType
                ? currentChild.BaseType.GetGenericTypeDefinition()
                : currentChild.BaseType;

            if (currentChild == null) {
                return false;
            }
        }

        return false;
    }

    private static bool HasAnyInterfaces(Type parent, Type child) {
        return child.GetInterfaces()
            .Any(childInterface => {
                    Type? currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                }
            );
    }

    private static Type ResolveGenericTypeDefinition(Type parent) {
        bool shouldUseGenericType = true;
        if (parent.IsGenericType && parent.GetGenericTypeDefinition() != parent) {
            shouldUseGenericType = false;
        }

        if (parent.IsGenericType && shouldUseGenericType) {
            parent = parent.GetGenericTypeDefinition();
        }

        return parent;
    }
}
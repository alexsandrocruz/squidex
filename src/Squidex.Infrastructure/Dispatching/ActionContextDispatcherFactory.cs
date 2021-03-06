// ==========================================================================
//  ActionContextDispatcherFactory.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Reflection;

// ReSharper disable UnusedMember.Local

namespace Squidex.Infrastructure.Dispatching
{
    internal static class ActionContextDispatcherFactory
    {
        public static Tuple<Type, Action<TTarget, object, TContext>> CreateActionHandler<TTarget, TContext>(MethodInfo methodInfo)
        {
            var inputType = methodInfo.GetParameters()[0].ParameterType;

            var factoryMethod =
                typeof(ActionContextDispatcherFactory)
                    .GetMethod("Factory", BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(TTarget), inputType, typeof(TContext));

            var handler = factoryMethod.Invoke(null, new object[] { methodInfo });

            return new Tuple<Type, Action<TTarget, object, TContext>>(inputType, (Action<TTarget, object, TContext>)handler);
        }

        private static Action<TTarget, object, TContext> Factory<TTarget, TIn, TContext>(MethodInfo methodInfo)
        {
            var type = typeof(Action<TTarget, TIn, TContext>);

            var handler = (Action<TTarget, TIn, TContext>)methodInfo.CreateDelegate(type);

            return (target, input, context) => handler(target, (TIn)input, context);
        }
    }
}
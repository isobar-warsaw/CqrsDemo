﻿using System;
using System.Reflection;
using System.Collections.Generic;

namespace CqrsDemo
{
    public class HandlersProvider
    {
        public List<Type> Services { get; } = new List<Type>();

        public HandlersProvider()
        {
            Type[] thisAssemblyTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type t in thisAssemblyTypes)
            {
                if (!t.IsClass || !t.IsPublic || (t.IsAbstract)) continue;

                var interfaces = t.GetInterfaces();

                foreach (var i in interfaces)
                {
                    if (!i.IsGenericType) continue;

                    if ((i.GetGenericTypeDefinition() != typeof(ICommandHandler<>))
                        && (i.GetGenericTypeDefinition() != typeof(IQueryHandler<,>))) continue;

                    Services.Add(t);
                }
            }
        }
    }
}

﻿// The MIT License (MIT)
// 
// Copyright (c) 2015-2016 Rasmus Mikkelsen
// Copyright (c) 2015-2016 eBay Software Foundation
// https://github.com/rasmus/EventFlow
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using EventFlow.Configuration;
using EventFlow.Extensions;

namespace EventFlow.Core.IoC.Factories
{
    internal class ConstructorFactory : IFactory
    {
        private readonly ConstructorInfo _constructorInfo;
        private readonly IReadOnlyCollection<ParameterInfo> _parameterInfos;

        public ConstructorFactory(Type type)
        {
            var constructorInfos = type
                .GetTypeInfo()
                .GetConstructors();

            if (constructorInfos.Length > 1)
            {
                throw new ConfigurationErrorsException($"Type {type.PrettyPrint()} has more than one constructor");
            }

            _constructorInfo = constructorInfos.Single();
            _parameterInfos = _constructorInfo.GetParameters();
        }

        public object Create(IResolverContext resolverContext)
        {
            var parameters = new object[_parameterInfos.Count];
            foreach (var parameterInfo in _parameterInfos)
            {
                var enumerableType = parameterInfo.ParameterType
                    .GetTypeInfo()
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.IsInterface && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (enumerableType == null)
                {
                    parameters[parameterInfo.Position] = resolverContext.Resolver.Resolve(parameterInfo.ParameterType);
                }
                else
                {
                    throw new NotImplementedException("TODO");
                }
            }

            return _constructorInfo.Invoke(parameters);
        }
    }
}
﻿using System;
using System.Threading.Tasks;

namespace SwCore.Common
{
    internal interface IEntrypoint
    {
        Type[] Export { get; }
        Type[] Import { get; }
        Type InstanceType { get; }
        SwComponentPriorityEnum Priority { get; }
        bool IsAsync { get; }

        int Execute(object instance, object[] args, object[] output);
        Task<int> ExecuteAsync(object instance, object[] args, object[] output);
    }
}
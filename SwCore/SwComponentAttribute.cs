using System;
using System.Collections.Generic;
using System.Text;

namespace SwCore
{
    public class SwComponentAttribute : Attribute
    {
        public SwComponentPriorityEnum Priority { get; private set; }
        public SwComponentAttribute(SwComponentPriorityEnum priority = SwComponentPriorityEnum.Medium)
        {
            Priority = priority;
        }
    }
}

﻿using Core;
using Messages.Dto;

namespace Messages.Queries
{
    public class SampleQuery : IQuery<SampleQueryResponse>
    {
        public string Foo { get; set; }
    }
}

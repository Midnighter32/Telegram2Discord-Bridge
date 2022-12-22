using System;
using Test.DataTypes;

namespace Test.Services
{
    public interface IMessageParserService
    {
        public event Action<News> PostReady;
    }
}

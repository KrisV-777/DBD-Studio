using System.Collections.Generic;
using DBDStudio.Models;

namespace DBDStudio.Interfaces
{
    public interface IRuleService
    {
        IReadOnlyList<Rule> GetRules();
        void Add(Rule rule);
        void Remove(Rule rule);
    }
}

using System.Collections.Generic;
using DBDStudio.Core.Models;

namespace DBDStudio.Core.Interfaces;

public interface IRuleService
{
    IReadOnlyList<Rule> GetRules();
    void Add(Rule rule);
    void Update(Rule rule);
    void Remove(Rule rule);
}

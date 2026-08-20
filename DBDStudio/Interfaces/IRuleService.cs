using System.Collections.ObjectModel;
using System.ComponentModel;
using DBDStudio.Models.Component;

namespace DBDStudio.Interfaces
{
    public interface IRuleService
    {
        ObservableCollection<RuleConstruct> Rules { get; }

        void Add(RuleConstruct? rule);
        void Remove(RuleConstruct rule);
        void Reset(RuleConstruct rule);
        void Save(RuleConstruct rule);
        void SaveAs(RuleConstruct rule, string filePath);
    }
}

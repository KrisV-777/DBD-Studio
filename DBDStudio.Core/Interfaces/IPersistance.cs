using System;

namespace DBDStudio.Core.Interfaces
{
    public interface IPersistable
    {
        string PersistenceKey { get; }
        Type PersistenceStateType { get; }
        object? SaveState();
        void RestoreState(object? state);
    }
}

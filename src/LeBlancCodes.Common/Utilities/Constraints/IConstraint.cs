using System;

namespace LeBlancCodes.Common.Utilities.Constraints
{
    public interface IConstraint : IResolveConstraint
    {
        string DisplayName { get; }

        string Description { get; }

        object[] Arguments { get; }

        ConstraintBuilder Builder { get; }

        ConstraintResult ApplyTo<TActual>(TActual actual);

        ConstraintResult ApplyTo<TActual>(Action<TActual> actualAction);

        ConstraintResult ApplyTo<TActual>(ref TActual actual);
    }
}

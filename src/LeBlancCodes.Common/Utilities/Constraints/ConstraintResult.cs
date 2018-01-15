namespace LeBlancCodes.Common.Utilities.Constraints
{
    public class ConstraintResult
    {
        private readonly IConstraint _constraint;

        public ConstraintResult(IConstraint constraint, object actualValue)
        {
            _constraint = constraint;
            ActualValue = actualValue;
        }

        public ConstraintResult(IConstraint constraint, object actualValue, ConstraintStatus status) :
            this(constraint, actualValue) => Status = status;

        public ConstraintResult(IConstraint constraint, object actualValue, bool isSuccess) :
            this(constraint, actualValue) => Status = isSuccess ? ConstraintStatus.Success : ConstraintStatus.Failure;

        public object ActualValue { get; }

        public ConstraintStatus Status { get; set; }

        public virtual bool IsSuccess => Status == ConstraintStatus.Success;

        public string Name => _constraint.DisplayName;

        public string Description => _constraint.Description;
    }
}

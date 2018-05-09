namespace QL.Database
{
    using System;

    public enum DbConditionCompare
    {
        LT,
        LTOrEqual,
        GT,
        GTOrEqual,
        Equal,
        Unequal,
        IsNull,
        IsNotNull
    }
}

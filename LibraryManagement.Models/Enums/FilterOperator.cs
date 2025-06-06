namespace LibraryManagement.Models.Enums;

public enum FilterOperator
{
    Eq, // Equals to
    Neq,  // Not equals to
    Gt,  // Greater than
    Gteq, // Greater than or equal to
    Lt, // Less than
    Lteq, // Less than or equal to
    Sw, // Starts with
    Ew, // End with
    Like // Contains
}

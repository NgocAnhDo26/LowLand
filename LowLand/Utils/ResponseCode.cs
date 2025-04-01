namespace LowLand.Utils
{
    public enum ResponseCode : int
    {
        Success = 1,
        EmptyName = 2,
        NameExists = 3,
        Error = 4,
        NegativeValueNotAllowed = 5,
        InvalidValue = 6,
        NotFound = 7,
        ItemHaveDependency = 8,
    }
}

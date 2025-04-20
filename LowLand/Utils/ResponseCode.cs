namespace LowLand.Utils
{
    public enum ResponseCode : int
    {
        Success,
        EmptyName,
        NameExists,
        Error,
        NegativeValueNotAllowed,
        InvalidValue,
        NotFound,
        ItemHaveDependency,
        CategoryEmpty,
        NoChildProduct,
        InvalidDate,
        InvalidStatus,
        EmptyDate,
    }
}

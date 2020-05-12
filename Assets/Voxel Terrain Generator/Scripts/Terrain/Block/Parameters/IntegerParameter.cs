public struct IntegerParameter
{
    public int value;
    public string name;

    public IntegerParameter(int value, string name)
    {
        this.value = value;
        this.name = name;
    }

    public string GetName()
    {
        return name;
    }

    public int GetValue()
    {
        return value;
    }
}

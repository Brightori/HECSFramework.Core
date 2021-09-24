using System;

public class AssertionException : Exception
{
    public AssertionException()
    {
    }
        
    public AssertionException(string message) : base(message)
    {
    }
}
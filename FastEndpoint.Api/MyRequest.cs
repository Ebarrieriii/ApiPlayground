public class MyRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}

public class MyResponse
{
    public string FullName { get; set; }
    public bool IsOver18 { get; set; }
}

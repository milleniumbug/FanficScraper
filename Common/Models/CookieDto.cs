namespace Common.Models;

public class CookieDto
{
    public string Name { get; set; }
    
    public string Value { get; set; }
    
    public string Domain { get; set; }
    
    public string Path { get; set; }
    
    public double Expires { get; set; }

    public bool HttpOnly { get; set; }
    
    public bool Secure { get; set; }
}

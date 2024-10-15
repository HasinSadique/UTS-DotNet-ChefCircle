using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myfirstapi.Model
{
    public class User
    {
        int UID {get;set;}
        string? Fullname {get;set;}
        string? Email {get;set;}
        string? Phone {get;set;}
        DateOnly DOB {get;set;}
        string? Role {get;set;}
        string? Password {get;set;}
    }
}
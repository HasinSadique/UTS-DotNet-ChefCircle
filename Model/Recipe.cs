namespace myfirstapi.Model
{
    public class Recipe
    {
        public int RID {get;set;}
        public int UID {get;set;}
        public string? Title {get;set;}
        public string? Description {get;set;}
        public int Likes {get;set;}
        public bool IsVerified {get;set;}
    }
}
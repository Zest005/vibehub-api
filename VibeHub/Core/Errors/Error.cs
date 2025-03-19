namespace Core.Errors;

public class EntityResult<T>
{
     public T? Entity;
     public string? Description { get; }
     public bool HaveErrors { get; } = false;
 
     public EntityResult(string? description = null, bool haveErrors = false)
     {
         Description = description;
         HaveErrors = haveErrors;
     }
 
     public override string? ToString()
     {
         return Description;
     }
 }

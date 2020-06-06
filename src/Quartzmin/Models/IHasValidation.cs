using System.Collections.Generic;

namespace SilkierQuartz.Models
{
    public interface IHasValidation
    {
        void Validate(ICollection<ValidationError> errors);
    }
}
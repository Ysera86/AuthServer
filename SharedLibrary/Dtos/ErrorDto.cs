using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Dtos
{
    public class ErrorDto
    {
        public List<string> Errors { get; private set; }= new List<string>();
        public bool IsShown { get; private set; } // set false for developer


        public ErrorDto(string error, bool isShown)
        {
            Errors.Add(error);
            IsShown = isShown;
        }

        public ErrorDto(List<string> errors, bool isShown)
        {
            Errors = errors;
            IsShown = isShown;
        }
    }
}

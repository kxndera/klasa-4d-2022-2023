﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LoginWpfApp.Validation.TypesOfValidation
{
    public interface ISpecyficValidation<T>
    {
        bool Validate(T value, out string message);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace iqoption.domain.IqOption.Exceptions
{
    public class LoginFailedException : Exception
    {
        public LoginFailedException(string message) : base(message) { }
    }
}

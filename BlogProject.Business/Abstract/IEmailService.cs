﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogProject.Business.Abstract
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlMessage);
    }
}
